// 
//  Copyright 2014  Deveel
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
using System.Collections;
using System.Collections.Generic;

using Deveel.Data.Expressions;
using Deveel.Data.Index;
using Deveel.Data.Types;

namespace Deveel.Data.DbSystem {
	public class FunctionTable : BaseDataTable {
		/// <summary>
		/// The table name given to all function tables.
		/// </summary>
		private static readonly ObjectName FunctionTableName = new ObjectName("FUNCTIONTABLE");

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

		private readonly ITable crossRefTable;

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

		private long rowCount;

		/// <summary>
		/// If the whole table is a simple enumeration (row index is 0 to 
		/// <see cref="Table.RowCount"/>) then this is true.
		/// </summary>
		private bool wholeTableIsSimpleEnum;

		public FunctionTable(ITable crossRefTable, Expression[] inExpList, string[] columnNames, IQueryContext context)
			: base(context.Connection.Database) {
			// Make sure we are synchronized over the class.
			lock (typeof(FunctionTable)) {
				uniqueId = UniqueKeySeq;
				++UniqueKeySeq;
			}
			uniqueId = (uniqueId & 0x0FFFFFFF) | 0x010000000;

			this.context = context;

			this.crossRefTable = crossRefTable;
			crResolver = new TableVariableResolver(crossRefTable);
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
					DataObject result = expr.Evaluate(context);
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
			rowCount = crossRefTable.RowCount;

			// Set schemes to 'blind search'.
			BlankSelectableSchemes(1);
		}

		///<summary>
		///</summary>
		///<param name="expList"></param>
		///<param name="columnNames"></param>
		///<param name="context"></param>
		public FunctionTable(Expression[] expList, String[] columnNames, IQueryContext context)
			: this(context.Connection.Database.SingleRowTable, expList, columnNames, context) {
		}

		///<summary>
		/// Returns the Table this function is based on.
		///</summary>
		/// <remarks>
		/// We need to provide this method for aggregate functions.
		/// </remarks>
		public ITable ReferenceTable {
			get { return crossRefTable; }
		}

		public override long RowCount {
			get { return rowCount; }
		}

		public override DataTableInfo TableInfo {
			get { return funTableInfo; }
		}

		private DataObject CalcValue(int column, long row, ICellCache cache) {
			crResolver.SetId = (int) row;
			if (groupResolver != null) {
				groupResolver.SetUpGroupForRow(row);
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
				wholeTableGroup = new List<long>((int) ReferenceTable.RowCount);
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

			ITable rootTable = ReferenceTable;
			long rowCount = rootTable.RowCount;
			int[] colLookup = new int[columns.Length];
			for (int i = columns.Length - 1; i >= 0; --i) {
				colLookup[i] = rootTable.TableInfo.IndexOfColumn(columns[i]);
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
			groupLinks = new List<long>((int) rowCount);
			int currentGroup = 0;
			long previousRow = -1;
			for (int i = 0; i < rowCount; ++i) {
				long rowIndex = rowList[i];

				if (previousRow != -1) {
					bool equal = true;
					// Compare cell in column in this row with previous row.
					for (int n = 0; n < colLookup.Length && equal; ++n) {
						DataObject c1 = rootTable.GetValue(colLookup[n], rowIndex);
						DataObject c2 = rootTable.GetValue(colLookup[n], previousRow);
						equal = (c1.CompareTo(c2) == 0);
					}

					if (!equal) {
						// If end of group, set bit 15
						groupLinks.Add(previousRow | 0x040000000);
						currentGroup = groupLinks.Count;
					} else {
						groupLinks.Add(previousRow);
					}
				}

				// groupLookup.Insert(row_index, current_group);
				PlaceAt(groupLookup, rowIndex, currentGroup);

				previousRow = rowIndex;
			}
			// Add the final row.
			groupLinks.Add(previousRow | 0x040000000);

			// Set up a group resolver for this method.
			groupResolver = new TableGroupResolver(this);
		}

		private static void PlaceAt(IList<long> list, long index, int value) {
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
		public long GetGroupSize(int groupNumber) {
			long groupSize = 1;
			long i = groupLinks[groupNumber];
			while ((i & 0x040000000) == 0) {
				++groupSize;
				++groupNumber;
				i = groupLinks[groupNumber];
			}
			return groupSize;
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

		public ITable MergeWithReference(ObjectName maxColumn) {
			ITable table = ReferenceTable;

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
					int col_num = ReferenceTable.TableInfo.IndexOfColumn(maxColumn);
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

			Table[] tabs = new Table[] { (Table)table, (Table) this };
			IList<long>[] rowSets = new IList<long>[] { rowList, rowList };

			VirtualTable outTable = new VirtualTable(tabs);
			outTable.Set(tabs, rowSets);

			// Output this as debugging information
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
			ITable refTab = ReferenceTable;

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
		public static ITable ResultTable(IQueryContext context, Expression expression) {
			var exp = new Expression[] { expression };
			var names = new String[] { "result" };
			Table functionTable = new FunctionTable(exp, names, context);
			var map = new int[] { 0 };
			var vars = new ObjectName[] { new ObjectName("result") };

			return new SubsetColumnTable(functionTable, map, vars);
		}

		///<summary>
		/// Returns a FunctionTable that has a single DataObject in it.
		///</summary>
		///<param name="context"></param>
		///<param name="ob"></param>
		/// <remarks>
		/// The column title is 'result'.
		/// </remarks>
		///<returns></returns>
		public static ITable ResultTable(IQueryContext context, DataObject ob) {
			return ResultTable(context, Expression.Constant(ob));
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
		public static ITable ResultTable(IQueryContext context, object obj) {
			return ResultTable(context, Expression.Constant(DataObject.From(obj)));
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
		public static ITable ResultTable(IQueryContext context, int value) {
			return ResultTable(context, (Number)value);
		}

		#region Nested type: TableGroupResolver

		/// <summary>
		/// Group resolver.  This is used to resolve group informations
		/// in the refering table.
		/// </summary>
		private sealed class TableGroupResolver : IGroupResolver {
			private readonly FunctionTable table;
			private IList<long> group;
			private int groupNumber = -1;

			private TableGVResolver tgvResolver;

			public TableGroupResolver(FunctionTable table) {
				this.table = table;
			}

			#region IGroupResolver Members

			public int GroupId {
				get { return (int) groupNumber; }
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
						return (int) table.GetGroupSize(groupNumber);
					}
				}
			}

			public DataObject Resolve(ObjectName variable, int setIndex) {
				//      String col_name = variable.getName();

				int colIndex = table.ReferenceTable.TableInfo.IndexOfColumn(variable);
				if (colIndex == -1) {
					throw new ApplicationException("Can't find column: " + variable);
				}

				EnsureGroup();

				long rowIndex = setIndex;
				if (group != null) {
					rowIndex = group[setIndex];
				}

				DataObject cell = table.ReferenceTable.GetValue(colIndex, rowIndex);

				return cell;
			}

			public IVariableResolver GetVariableResolver(int setIndex) {
				TableGVResolver resolver = CreateVariableResolver();
				resolver.SetId = setIndex;
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
						group = table.GetGroupRows(groupNumber);
					}
				}
			}

			/// <summary>
			/// Given a row index, this will setup the information in this resolver
			/// to solve for this group.
			/// </summary>
			/// <param name="rowIndex"></param>
			public void SetUpGroupForRow(long rowIndex) {
				if (table.wholeTableAsGroup) {
					if (groupNumber != -2) {
						groupNumber = -2;
						group = null;
					}
				} else {
					long g = table.GetRowGroup(rowIndex);
					if (g != groupNumber) {
						groupNumber = (int)g;
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
					get { return set_index; }
					set { set_index = value; }
				}

				public DataObject Resolve(ObjectName variable) {
					return tgr.Resolve(variable, set_index);
				}

				public DataType ReturnType(ObjectName variable) {
					int colIndex = tgr.table.ReferenceTable.TableInfo.IndexOfColumn(variable);
					if (colIndex == -1) {
						throw new ApplicationException("Can't find column: " + variable);
					}

					return tgr.table.ReferenceTable.TableInfo[colIndex].DataType;
				}

				#endregion
			}

			#endregion
		}

		#endregion
	}
}