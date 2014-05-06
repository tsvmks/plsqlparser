// 
//  Copyright 2010  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// A table that has a number of columns and as many rows as the 
	/// refering table.
	/// </summary>
	/// <remarks>
	/// Tables of this type are used to construct aggregate and function
	/// columns based on an expression.
	/// They are joined with the result table in the last part of the query 
	/// processing.
	/// <para>
	/// For example, a query like <c>SELECT id, id * 2, 8 * 9 FROM Part</c> the
	/// columns <c>id * 2</c> and <c>8 * 9</c> would be formed from this table.
	/// </para>
	/// <para>
	/// <b>Synchronization Issue:</b> Instances of this object are <b>not</b> 
	/// thread safe. The reason it's not is because if <see cref="GetCell"/> 
	/// is used concurrently it's possible for the same value to be added into 
	/// the cache causing an error.
	/// It is not expected that this object will be shared between threads.
	/// </para>
	/// </remarks>
	public class FunctionTable : DefaultDataTable {
		/// <summary>
		/// The table name given to all function tables.
		/// </summary>
		private static readonly ObjectName FunctionTableName = new ObjectName(null, "FUNCTIONTABLE");

		/// <summary>
		/// The key used to make distinct unique ids for FunctionTables.
		///</summary>
		/// <remarks>
		/// <b>Note</b>: This is a thread-safe static mutable variable.
		/// </remarks>
		private static int UniqueKeySeq = 0;

		/// <summary>
		/// The context of this function table.
		/// </summary>
		private readonly IQueryContext context;

		/// <summary>
		/// The TableVariableResolver for the table we are cross referencing.
		/// </summary>
		private readonly TableVariableResolver crResolver;

		private readonly Table crossRefTable;

		/// <summary>
		/// Some information about the expression list.  If the value is 0 then the
		/// column is simple to solve and shouldn't be cached.
		/// </summary>
		private readonly byte[] expInfo;

		/// <summary>
		/// The list of expressions that are evaluated to form each column.
		/// </summary>
		private readonly Expression[] expList;

		/// <summary>
		/// The DataTableInfo object that describes the columns in this function
		/// table.
		/// </summary>
		private readonly DataTableInfo funTableInfo;

		/// <summary>
		/// A unique id given to this FunctionTable when it is created.  No two
		/// FunctionTable objects may have the same number.  This number is between
		/// 0 and 260 million.
		/// </summary>
		private readonly int uniqueId;

		/// <summary>
		/// The group row links.
		/// </summary>
		/// <remarks>
		/// Iterate through this to find all the rows in a group until bit 31 set.
		/// </remarks>
		private IList<long> groupLinks;

		/// <summary>
		/// The lookup mapping for row->group_index used for grouping.
		/// </summary>
		private IList<long> groupLookup;

		/// <summary>
		/// The TableGroupResolver for the table.
		/// </summary>
		private TableGroupResolver groupResolver;

		/// <summary>
		/// Whether the whole table is a group.
		/// </summary>
		private bool wholeTableAsGroup;

		/// <summary>
		/// If the whole table is a group, this is the grouping rows.
		/// </summary>
		/// <remarks>
		/// This is obtained via <see cref="Table.SelectAll()"/> of the reference table.
		/// </remarks>
		private IList<long> wholeTableGroup;

		/// <summary>
		/// The total size of the whole table group size.
		/// </summary>
		private long wholeTableGroupSize;

		/// <summary>
		/// If the whole table is a simple enumeration (row index is 0 to 
		/// <see cref="Table.RowCount"/>) then this is true.
		/// </summary>
		private bool wholeTableIsSimpleEnum;


		///<summary>
		///</summary>
		///<param name="crossRefTable"></param>
		///<param name="inExpList"></param>
		///<param name="columnNames"></param>
		///<param name="context"></param>
		public FunctionTable(Table crossRefTable, Expression[] inExpList, string[] columnNames, IQueryContext context)
			: base(context.Connection.Database) {
			// Make sure we are synchronized over the class.
			lock (typeof (FunctionTable)) {
				uniqueId = UniqueKeySeq;
				++UniqueKeySeq;
			}
			uniqueId = (uniqueId & 0x0FFFFFFF) | 0x010000000;

			this.context = context;

			this.crossRefTable = crossRefTable;
			crResolver = crossRefTable.GetVariableResolver();
			crResolver.SetId = 0;

			// Create a DataTableInfo object for this function table.
			funTableInfo = new DataTableInfo(FunctionTableName);

			expList = new Expression[inExpList.Length];
			expInfo = new byte[inExpList.Length];

			// Create a new DataColumnInfo for each expression, and work out if the
			// expression is simple or not.
			for (int i = 0; i < inExpList.Length; ++i) {
				Expression expr = inExpList[i];
				// Examine the expression and determine if it is simple or not
				if (expr.IsConstant() && !expr.HasAggregateFunction(context)) {
					// If expression is a constant, solve it
					DataObject result = expr.Evaluate(null, null, context);
					expr = Expression.Constant(result);
					expList[i] = expr;
					expInfo[i] = 1;
				} else {
					// Otherwise must be dynamic
					expList[i] = expr;
					expInfo[i] = 0;
				}
				// Make the column info
				funTableInfo.AddColumn(columnNames[i], expr.ReturnType(crResolver, context));
			}

			// Make sure the table info isn't changed from this point on.
			funTableInfo.IsReadOnly = true;

			// Function tables are the size of the referring table.
			row_count = crossRefTable.RowCount;

			// Set schemes to 'blind search'.
			BlankSelectableSchemes(1);
		}

		///<summary>
		///</summary>
		///<param name="expList"></param>
		///<param name="columnNames"></param>
		///<param name="context"></param>
		public FunctionTable(Expression[] expList, String[] columnNames, IQueryContext context)
			: this((Table) context.Connection.Database.SingleRowTable, expList, columnNames, context) {
		}

		///<summary>
		/// Returns the Table this function is based on.
		///</summary>
		/// <remarks>
		/// We need to provide this method for aggregate functions.
		/// </remarks>
		public Table ReferenceTable {
			get { return crossRefTable; }
		}

		public override DataTableInfo TableInfo {
			get { return funTableInfo; }
		}

		public override bool HasRootsLocked {
			get { return ReferenceTable.HasRootsLocked; }
		}

		private DataObject CalcValue(int column, long row, ICellCache cache) {
			crResolver.SetId = (int)row;
			if (groupResolver != null) {
				groupResolver.SetUpGroupForRow((int)row);
			}
			Expression expr = expList[column];
			DataObject cell = expr.Evaluate(groupResolver, crResolver, context);
			if (cache != null) {
				cache.Set(uniqueId, row, column, cell);
			}
			return cell;
		}

		// ------ Public methods ------

		///<summary>
		/// Sets the whole reference table as a single group.
		///</summary>
		public void SetWholeTableAsGroup() {
			wholeTableAsGroup = true;

			wholeTableGroupSize = ReferenceTable.RowCount;

			// Set up 'whole_table_group' to the list of all rows in the reference
			// table.
			IEnumerator<long> en = ReferenceTable.GetRowEnumerator();
			wholeTableIsSimpleEnum = en is SimpleRowEnumerator;
			if (!wholeTableIsSimpleEnum) {
				wholeTableGroup = new List<long>((int)ReferenceTable.RowCount);
				while (en.MoveNext()) {
					wholeTableGroup.Add(en.Current);
				}
			}

			// Set up a group resolver for this method.
			groupResolver = new TableGroupResolver(this);
		}

		/// <summary>
		/// Creates a grouping matrix for the given columns.
		/// </summary>
		/// <param name="columns"></param>
		/// <remarks>
		/// The grouping matrix is arranged so that each row of the refering 
		/// table that is in the group is given a number that refers to the top 
		/// group entry in the group list. The group list is a linked integer 
		/// list that chains through each row item in the list.
		/// </remarks>
		public void CreateGroupMatrix(ObjectName[] columns) {
			// If we have zero rows, then don't bother creating the matrix.
			if (RowCount <= 0 || columns.Length <= 0)
				return;

			Table rootTable = ReferenceTable;
			long rowCount = rootTable.RowCount;
			int[] colLookup = new int[columns.Length];
			for (int i = columns.Length - 1; i >= 0; --i) {
				colLookup[i] = rootTable.FindFieldName(columns[i]);
			}

			IList<long> rowList = rootTable.OrdereddRows(colLookup);

			// 'row_list' now contains rows in this table sorted by the columns to
			// group by.

			// This algorithm will generate two lists.  The group_lookup list maps
			// from rows in this table to the group number the row belongs in.  The
			// group number can be used as an index to the 'group_links' list that
			// contains consequtive links to each row in the group until -1 is reached
			// indicating the end of the group;

			groupLookup = new List<long>((int) rowCount);
			groupLinks = new List<long>((int)rowCount);
			int current_group = 0;
			long previous_row = -1;
			for (int i = 0; i < rowCount; ++i) {
				long rowIndex = rowList[i];

				if (previous_row != -1) {
					bool equal = true;
					// Compare cell in column in this row with previous row.
					for (int n = 0; n < colLookup.Length && equal; ++n) {
						DataObject c1 = rootTable.GetValue(colLookup[n], rowIndex);
						DataObject c2 = rootTable.GetValue(colLookup[n], previous_row);
						equal = (c1.CompareTo(c2) == 0);
					}

					if (!equal) {
						// If end of group, set bit 15
						groupLinks.Add(previous_row | 0x040000000);
						current_group = groupLinks.Count;
					} else {
						groupLinks.Add(previous_row);
					}
				}

				// groupLookup.Insert(row_index, current_group);
				PlaceAt(groupLookup, rowIndex, current_group);

				previous_row = rowIndex;
			}
			// Add the final row.
			groupLinks.Add(previous_row | 0x040000000);

			// Set up a group resolver for this method.
			groupResolver = new TableGroupResolver(this);
		}

		private static void PlaceAt(IList<long> list, long index, long value) {
			while (index > list.Count) {
				list.Add(0);
			}

			list.Insert((int)index, value);
		}


		// ------ Methods intended for use by grouping functions ------

		///<summary>
		/// Returns the group of the row at the given index.
		///</summary>
		///<param name="row_index"></param>
		///<returns></returns>
		public long GetRowGroup(long row_index) {
			return groupLookup[(int)row_index];
		}

		///<summary>
		/// The size of the group with the given number.
		///</summary>
		///<param name="groupNumber"></param>
		///<returns></returns>
		public int GetGroupSize(int groupNumber) {
			int group_size = 1;
			long i = groupLinks[groupNumber];
			while ((i & 0x040000000) == 0) {
				++group_size;
				++groupNumber;
				i = groupLinks[groupNumber];
			}
			return group_size;
		}

		///<summary>
		/// Returns an IntegerVector that represents the list of all rows in 
		/// the group the index is at.
		///</summary>
		///<param name="groupNumber"></param>
		///<returns></returns>
		public IList<long> GetGroupRows(int groupNumber) {
			List<long> ivec = new List<long>();
			long i = groupLinks[groupNumber];
			while ((i & 0x040000000) == 0) {
				ivec.Add(i);
				++groupNumber;
				i = groupLinks[groupNumber];
			}
			ivec.Add(i & 0x03FFFFFFF);
			return ivec;
		}

		///<summary>
		/// Returns a Table that is this function table merged with the cross
		/// reference table.
		///</summary>
		///<param name="maxColumn"></param>
		/// <remarks>
		/// The result table includes only one row from each group.
		/// <para>
		/// The 'max_column' argument is optional (can be null).  If it's set to a
		/// column in the reference table, then the row with the max value from the
		/// group is used as the group row.  For example, 'Part.id' will return the
		/// row with the maximum part.id from each group.
		/// </para>
		/// </remarks>
		///<returns></returns>
		public Table MergeWithReference(ObjectName maxColumn) {
			Table table = ReferenceTable;

			IList<long> rowList;

			if (wholeTableAsGroup) {
				// Whole table is group, so take top entry of table.

				rowList = new List<long>(1);
				IEnumerator<long> rowEnum = table.GetRowEnumerator();
				if (rowEnum.MoveNext()) {
					rowList.Add(rowEnum.Current);
				} else {
					// MAJOR HACK: If the referencing table has no elements then we choose
					//   an arbitary index from the reference table to merge so we have
					//   at least one element in the table.
					//   This is to fix the 'SELECT COUNT(*) FROM empty_table' bug.
					rowList.Add(Int32.MaxValue - 1);
				}
			} else if (table.RowCount == 0) {
				rowList = new List<long>(0);
			} else if (groupLinks != null) {
				// If we are grouping, reduce down to only include one row from each
				// group.
				if (maxColumn == null) {
					rowList = GetTopFromEachGroup();
				} else {
					int col_num = ReferenceTable.FindFieldName(maxColumn);
					rowList = GetMaxFromEachGroup(col_num);
				}
			} else {
				// OPTIMIZATION: This should be optimized.  It should be fairly trivial
				//   to generate a Table implementation that efficiently merges this
				//   function table with the reference table.

				// This means there is no grouping, so merge with entire table,
				long rowCount = table.RowCount;
				rowList = new List<long>((int)rowCount);
				IEnumerator<long> en = table.GetRowEnumerator();
				while (en.MoveNext()) {
					rowList.Add(en.Current);
				}
			}

			// Create a virtual table that's the new group table merged with the
			// functions in this...

			Table[] tabs = new Table[] { table, this };
			IList<long>[] rowSets = new IList<long>[] { rowList, rowList };

			VirtualTable outTable = new VirtualTable(tabs);
			outTable.Set(tabs, rowSets);
			table = outTable;
			return table;
		}

		// ------ Package protected methods -----

		/// <summary>
		/// Returns a list of rows that represent one row from each distinct group
		/// in this table.
		/// </summary>
		/// <remarks>
		/// This should be used to construct a virtual table of rows from 
		/// each distinct group.
		/// </remarks>
		/// <returns></returns>
		private IList<long> GetTopFromEachGroup() {
			List<long> extractRows = new List<long>();
			int size = groupLinks.Count;
			bool take = true;
			for (int i = 0; i < size; ++i) {
				long r = groupLinks[i];
				if (take) {
					extractRows.Add(r & 0x03FFFFFFF);
				}
				take = (r & 0x040000000) != 0;
			}

			return extractRows;
		}


		/// <summary>
		/// Returns a list of rows that represent the maximum row of the given column
		/// from each distinct group in this table.
		/// </summary>
		/// <param name="colNum"></param>
		/// <remarks>
		/// This should be used to construct a virtual table of rows from 
		/// each distinct group.
		/// </remarks>
		/// <returns></returns>
		private IList<long> GetMaxFromEachGroup(int colNum) {
			Table refTab = ReferenceTable;

			List<long> extractRows = new List<long>();
			int size = groupLinks.Count;

			long toTakeInGroup = -1;
			DataObject max = null;

			bool take = true;
			for (int i = 0; i < size; ++i) {
				long r = groupLinks[i];

				long actRIndex = r & 0x03FFFFFFF;
				DataObject cell = refTab.GetValue(colNum, actRIndex);
				if (max == null || cell.CompareTo(max) > 0) {
					max = cell;
					toTakeInGroup = actRIndex;
				}
				if ((r & 0x040000000) != 0) {
					extractRows.Add(toTakeInGroup);
					max = null;
				}
			}

			return extractRows;
		}

		// ------ Methods that are implemented for Table interface ------

		public override DataObject GetValue(int column, long row) {
			// [ FUNCTION TABLE CACHING NOW USES THE GLOBAL CELL CACHING MECHANISM ]
			// Check if in the cache,
			ICellCache cache = Database.CellCache;
			// Is the column worth caching, and is caching enabled?
			if (expInfo[column] == 0 && cache != null) {
				DataObject cell = cache.Get(uniqueId, row, column);
				if (cell != null)
					// In the cache so return the cell.
					return cell;

				// Not in the cache so calculate the value and WriteByte it in the cache.
				cell = CalcValue(column, row, cache);
				return cell;
			}

			// Caching is not enabled
			return CalcValue(column, row, null);
		}

		public override IEnumerator<long> GetRowEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		public override void LockRoot(int lockKey) {
			// We Lock the reference table.
			// NOTE: This cause the reference table to Lock twice when we use the
			//  'MergeWithReference' method.  While this isn't perfect behaviour, it
			//  means if 'MergeWithReference' isn't used, we still maintain a safe
			//  level of locking.
			ReferenceTable.LockRoot(lockKey);
		}

		public override void UnlockRoot(int lockKey) {
			// We unlock the reference table.
			// NOTE: This cause the reference table to unlock twice when we use the
			//  'MergeWithReference' method.  While this isn't perfect behaviour, it
			//  means if 'MergeWithReference' isn't used, we still maintain a safe
			//  level of locking.
			ReferenceTable.UnlockRoot(lockKey);
		}

		// ---------- Convenience statics ----------

		///<summary>
		/// Returns a FunctionTable that has a single Expression evaluated in it.
		///</summary>
		///<param name="context"></param>
		///<param name="expression"></param>
		/// <remarks>
		/// The column name is 'result'.
		/// </remarks>
		///<returns></returns>
		public static Table ResultTable(IQueryContext context, Expression expression) {
			Expression[] exp = new Expression[] { expression };
			string[] names = new String[] {"result"};
			Table function_table = new FunctionTable(exp, names, context);
			SubsetColumnTable result = new SubsetColumnTable(function_table);

			int[] map = new int[] {0};
			ObjectName[] vars = new[] {new ObjectName("result")};
			result.SetColumnMap(map, vars);

			return result;
		}

		///<summary>
		/// Returns a FunctionTable that has a single TObject in it.
		///</summary>
		///<param name="context"></param>
		///<param name="ob"></param>
		/// <remarks>
		/// The column title is 'result'.
		/// </remarks>
		///<returns></returns>
		public static Table ResultTable(IQueryContext context, DataObject ob) {
			Expression resultExp = Expression.Constant(ob);
			return ResultTable(context, resultExp);
		}

		///<summary>
		/// Returns a FunctionTable that has a single Object in it.
		///</summary>
		///<param name="context"></param>
		///<param name="obj"></param>
		/// <remarks>
		/// The column title is 'result'.
		/// </remarks>
		///<returns></returns>
		public static Table ResultTable(IQueryContext context, object obj) {
			return ResultTable(context, DataObject.From(obj));
		}

		///<summary>
		/// Returns a FunctionTable that has an int value made into a BigNumber.
		///</summary>
		///<param name="context"></param>
		///<param name="value"></param>
		/// <remarks>
		/// The column title is 'result'.
		/// </remarks>
		///<returns></returns>
		public static Table ResultTable(IQueryContext context, int value) {
			return ResultTable(context, (Number)value);
		}


		// ---------- Inner classes ----------

		#region Nested type: TableGroupResolver

		/// <summary>
		/// Group resolver.  This is used to resolve group informations
		/// in the refering table.
		/// </summary>
		private sealed class TableGroupResolver : IGroupResolver {
			private readonly FunctionTable table;
			/**
			 * The list that represents the group we are currently
			 * processing.
			 */
			private IList<long> group;

			/**
			 * The current group number.
			 */
			private long groupNumber = -1;

			/**
			 * A IVariableResolver that can resolve variables within a set of a group.
			 */
			private TableGVResolver tgvResolver;

			public TableGroupResolver(FunctionTable table) {
				this.table = table;
			}

			#region IGroupResolver Members

			public int GroupId {
				get { return (int)groupNumber; }
			}

			public int Count {
				get {
					if (groupNumber == -2) {
						return (int) table.wholeTableGroupSize;
						//        return whole_table_group.size();
						//        // ISSUE: Unsafe call if reference table is a DataTable.
						//        return getReferenceTable().getRowCount();
					} else if (group != null) {
						return group.Count;
					} else {
						return table.GetGroupSize((int)groupNumber);
					}
				}
			}

			public DataObject Resolve(ObjectName variable, int set_index) {
				//      String col_name = variable.getName();

				int col_index = table.ReferenceTable.FastFindFieldName(variable);
				if (col_index == -1) {
					throw new ApplicationException("Can't find column: " + variable);
				}

				EnsureGroup();

				long row_index = set_index;
				if (group != null) {
					row_index = group[set_index];
				}
				DataObject cell = table.ReferenceTable.GetValue(col_index, row_index);

				return cell;
			}

			public IVariableResolver GetVariableResolver(int set_index) {
				TableGVResolver resolver = CreateVariableResolver();
				resolver.SetIndex(set_index);
				return resolver;
			}

			#endregion

			/// <summary>
			/// Creates a resolver that resolves variables within a set of the group.
			/// </summary>
			/// <returns></returns>
			private TableGVResolver CreateVariableResolver() {
				if (tgvResolver != null) {
					return tgvResolver;
				}
				tgvResolver = new TableGVResolver();
				return tgvResolver;
			}


			/// <summary>
			/// Ensures that 'group' is set up.
			/// </summary>
			private void EnsureGroup() {
				if (group == null) {
					if (groupNumber == -2) {
						group = table.wholeTableGroup;
						//          // ISSUE: Unsafe calls if reference table is a DataTable.
						//          group = new IntegerVector(getReferenceTable().getRowCount());
						//          IRowEnumerator renum = getReferenceTable().GetRowEnumerator();
						//          while (renum.hasMoreRows()) {
						//            group.Add(renum.nextRowIndex());
						//          }
					} else {
						group = table.GetGroupRows((int)groupNumber);
					}
				}
			}

			/// <summary>
			/// Given a row index, this will setup the information in this resolver
			/// to solve for this group.
			/// </summary>
			/// <param name="row_index"></param>
			public void SetUpGroupForRow(int row_index) {
				if (table.wholeTableAsGroup) {
					if (groupNumber != -2) {
						groupNumber = -2;
						group = null;
					}
				} else {
					long g = table.GetRowGroup(row_index);
					if (g != groupNumber) {
						groupNumber = g;
						group = null;
					}
				}
			}

			// ---------- Inner classes ----------

			#region Nested type: TableGVResolver

			private class TableGVResolver : IVariableResolver {
				private int set_index;
				private TableGroupResolver tgr;

				// ---------- Implemented from IVariableResolver ----------

				#region IVariableResolver Members

				public int SetId {
					get { throw new ApplicationException("setID not implemented here..."); }
				}

				public DataObject Resolve(ObjectName variable) {
					return tgr.Resolve(variable, set_index);
				}

				public DataType ReturnType(ObjectName variable) {
					int col_index = tgr.table.ReferenceTable.FastFindFieldName(variable);
					if (col_index == -1) {
						throw new ApplicationException("Can't find column: " + variable);
					}

					return tgr.table.ReferenceTable.TableInfo[col_index].DataType;
				}

				#endregion

				internal void SetIndex(int set_index) {
					this.set_index = set_index;
				}
			}

			#endregion
		}

		#endregion
	}
}