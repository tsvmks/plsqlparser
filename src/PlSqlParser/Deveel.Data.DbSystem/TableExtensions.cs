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
using System.Linq;

using Deveel.Data.Expressions;
using Deveel.Data.Index;
using Deveel.Data.Query;
using Deveel.Data.Types;

namespace Deveel.Data.DbSystem {
	public static class TableExtenions {
		private static int RequireColumnIndex(DataTableInfo tableInfo, string columnName) {
			var offset = tableInfo.FindColumnName(columnName);
			if (offset == -1)
				throw new ArgumentException(String.Format("Table {0} has no column named {1}", tableInfo.Name, columnName));

			return offset;
		}


		private static DataObject[] SingleArrayCellMap(DataObject cell) {
			return cell == null ? null : new DataObject[] { cell };
		}

		public static DataObject GetValue(this ITable table, string columnName, long row) {
			return table.GetValue(RequireColumnIndex(table.TableInfo, columnName), row);
		}

		public static DataObject GetFirstCell(this ITable table, int column) {
			IEnumerable<long> rows = table.SelectFirst(column);
			return rows.Any() ?  table.GetValue(column, rows.First()) : null;
		}

		public static DataObject[] GetFirstCell(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ApplicationException("Multi-column GetLastCell not supported.");

			return SingleArrayCellMap(table.GetFirstCell(columns[0]));
		}

		public static DataObject GetLastCell(this ITable table, int column) {
			IEnumerable<long> rows = table.SelectLast(column);
			return rows.Any() ? table.GetValue(column, rows.First()) : null;
		}

		public static DataObject[] GetLastCell(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ApplicationException("Multi-column GetLastCellContent not supported.");

			return SingleArrayCellMap(table.GetLastCell(columns[0]));
		}

		public static DataObject GetSingleCell(this ITable table, int column) {
			IEnumerable<long> rows = table.SelectFirst(column);
			int sz = rows.Count();
			return sz == table.RowCount && sz > 0 ? table.GetValue(column, rows.First()) : null;
		}

		public static DataObject[] GetSingleCell(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ApplicationException("Multi-column GetSingleCellContent not supported.");

			return SingleArrayCellMap(table.GetSingleCell(columns[0]));
		}

		public static SelectableScheme GetScheme(this ITable table, string columnName) {
			return table.GetScheme(RequireColumnIndex(table.TableInfo, columnName));
		}

		public static bool ColumnContainsValue(this ITable table, int column, DataObject value) {
			return table.ColumnMatchesValue(column, Operator.Equal, value);
		}

		public static bool ColumnMatchesValue(this ITable table, int column, Operator op, DataObject value) {
			return table.SelectRows(column, op, value).Any();
		}

		public static ITable SimpleSelect(this ITable table, IQueryContext context, ObjectName columnName, Operator op, Expression expression) {
			throw new NotImplementedException();
		}

		public static ITable Union(this ITable table, ITable other) {
			throw new NotImplementedException();
		}

		public static Table Join(this ITable table, ITable other) {
			throw new NotImplementedException();
		}

		public static ITable NonCorrelated(this ITable table, ObjectName[] columnNames, Operator op, ITable other) {
			throw new NotImplementedException();
		}

		public static ITable SimpleJoin(this ITable table, IQueryContext context, ITable other, ObjectName columnName, Operator op, Expression expression) {
			throw new NotImplementedException();
		}

		public static ITable Outer(this ITable table, ITable other) {
			throw new NotImplementedException();
		}

		public static ITable ExhaustiveSelect(this ITable table, IQueryContext context, Expression expression) {
			throw new NotImplementedException();
		}

		public static ITable RangeSelect(this ITable table, ObjectName columnName, SelectableRange[] ranges) {
			// If this table is empty then there is no range to select so
			// trivially return this object.
			if (table.RowCount == 0)
				return table;

			// Are we selecting a black or null range?
			if (ranges == null || ranges.Length == 0)
				// Yes, so return an empty table
				return table.EmptySelect();

			// Are we selecting the entire range?
			if (ranges.Length == 1 &&
				ranges[0].Equals(SelectableRange.FullRange))
				// Yes, so return this table.
				return table;

			// Must be a non-trivial range selection.

			var t = (Table) table;

			// Find the column index of the column selected
			int column = t.FindFieldName(columnName);

			if (column == -1)
				throw new Exception("Unable to find the column given to select the range of: " + columnName.Name);

			// Select the range
			IList<long> rows =  table.SelectRange(column, ranges);

			// Make a new table with the range selected
			VirtualTable vTable = new VirtualTable((Table)table);
			vTable.Set((Table)table, rows);

			// We know the new set is ordered by the column.
			vTable.OptimisedPostSet(column);

			return vTable;


		}

		public static ITable OrderBy(this ITable table, ObjectName columnName, bool ascending) {
			throw new NotImplementedException();
		}

		public static ITable OrderBy(this ITable table, int column, bool ascending) {
			// Check the field can be sorted
			DataColumnInfo colInfo = table.TableInfo[column];

			List<long> rows = new List<long>(table.SelectAll(column));

			// Reverse the list if we are not ascending
			if (ascending == false)
				rows.Reverse();

			// We now has an int[] array of rows from this table to make into a
			// new table.

			VirtualTable vTable = new VirtualTable((Table)table);
			vTable.Set((Table)table, rows);

			return table;
		}

		public static ITable OrderBy(this ITable table, int[] columns) {
			// Sort by the column list.
			ITable result = table;
			for (int i = columns.Length - 1; i >= 0; --i) {
				result = result.OrderBy(columns[i], true);
			}

			// A nice post condition to check on.
			if (table.RowCount != result.RowCount)
				throw new ApplicationException("Internal Error, row count != sorted row count");

			return table;
		}

		public static IList<long> OrdereddRows(this ITable table, int[] columns) {
			Table work = (Table) table.OrderBy(columns);

			// 'work' is now sorted by the columns,
			// Get the rows in this tables domain,
			long rowCount = table.RowCount;
			List<long> rowList = new List<long>((int)rowCount);
			IEnumerator<long> e = work.GetRowEnumerator();
			while (e.MoveNext()) {
				rowList.Add(e.Current);
			}

			work.SetToRowTableDomain(0, rowList, table);
			return rowList;
		}


		public static IEnumerable<long> SelectFirst(this ITable table, int column) {
			SelectableScheme ss = table.GetScheme(column);
			return ss.SelectFirst();
		}

		public static IEnumerable<long> SelectLast(this ITable table, int column) {
			SelectableScheme ss = table.GetScheme(column);
			return ss.SelectLast();
		}

		public static IEnumerable<long> SelectAll(this ITable table, int column) {
			SelectableScheme ss = table.GetScheme(column);
			return ss.SelectAll();
		}

		public static IEnumerable<long> SelectAll(this ITable table) {
			return new SelectAllEnumerable(table.GetRowEnumerator());
		}

		public static IList<long> SelectRange(this ITable table, int column, SelectableRange[] ranges) {
			SelectableScheme ss = table.GetScheme(column);
			return ss.SelectRange(ranges);
		}

		public static IEnumerable<long> SelectRows(this ITable table, int[] cols, Operator op, DataObject[] cells) {
			// TODO: Look for an multi-column index to make this a lot faster,
			if (cols.Length > 1)
				throw new ApplicationException("Multi-column select not supported.");

			return table.SelectRows(cols[0], op, cells[0]);
		}


		private static bool CompareCells(DataObject ob1, DataObject ob2, Operator op) {
			var exp = Expression.Binary(Expression.Constant(ob1), op.AsExpressionType(), Expression.Constant(ob2));
			DataObject result = exp.Evaluate();
			// NOTE: This will be a NullPointerException if the result is not a
			//   boolean type.
			//TODO: check...
			bool? bresult = result.ToBoolean();
			if (!bresult.HasValue)
				throw new NullReferenceException();

			return bresult.Value;
		}

		public static IEnumerable<long> SelectRows(this ITable table, int column, Operator op, DataObject cell) {
			// If the cell is of an incompatible type, return no results,
			DataType colType = table.TableInfo[column].DataType;
			if (!cell.DataType.IsComparable(colType)) {
				// Types not comparable, so return 0
				return new List<long>(0);
			}

			// Get the selectable scheme for this column
			SelectableScheme ss = table.GetScheme(column);

			// If the operator is a standard operator, use the interned SelectableScheme
			// methods.
			if (op.IsEquivalent(Operator.Equal))
				return ss.SelectEqual(cell);
			if (op.IsEquivalent(Operator.NotEqual))
				return ss.SelectNotEqual(cell);
			if (op.IsEquivalent(Operator.Greater))
				return ss.SelectGreater(cell);
			if (op.IsEquivalent(Operator.Smaller))
				return ss.SelectLess(cell);
			if (op.IsEquivalent(Operator.GreaterOrEqual))
				return ss.SelectGreaterOrEqual(cell);
			if (op.IsEquivalent(Operator.SmallerOrEqual))
				return ss.SelectLessOrEqual(cell);

			// If it's not a standard operator (such as IS, NOT IS, etc) we generate the
			// range set especially.
			SelectableRangeSet rangeSet = new SelectableRangeSet();
			rangeSet.Intersect(op, cell);
			return ss.SelectRange(rangeSet.ToArray());
		}

		public static ITable EmptySelect(this ITable table) {
			if (table.RowCount == 0)
				return table;

			var theTable = table as Table;
			if (theTable == null)
				return table;

			VirtualTable vTable = new VirtualTable((Table) table);
			vTable.Set((Table) table, new List<long>(0));
			return vTable;
		}

		public static ITable Any(this ITable theTable, IQueryContext context, Expression expression, Operator op, Table rightTable) {
			ITable table = rightTable;

			// Check the table only has 1 column
			if (table.TableInfo.ColumnCount != 1)
				throw new ApplicationException("Input table <> 1 columns.");

			// Handle trivial case of no entries to select from
			if (theTable.RowCount == 0)
				return theTable;

			// If 'table' is empty then we return an empty set.  ANY { empty set } is
			// always false.
			if (table.RowCount == 0)
				return theTable.EmptySelect();

			// Is the lhs expression a constant?
			if (expression.IsConstant()) {
				// We know lhs is a constant so no point passing arguments,
				DataObject value = expression.Evaluate(null, context);
				// Select from the table.
				IEnumerable<long> list = table.SelectRows(0, op, value);
				if (list.Any())
					// There's some entries so return the whole table,
					return theTable;

				// No entries matches so return an empty table.
				return theTable.EmptySelect();
			}

			Table sourceTable;
			int lhsColIndex;
			// Is the lhs expression a single variable?
			ObjectName expVar = expression.AsVariable();
			// NOTE: It'll be less common for this part to be called.
			if (expVar == null) {
				// This is a complex expression so make a FunctionTable as our new
				// source.
				FunctionTable funTable = new FunctionTable((Table) theTable, new Expression[] { expression }, new String[] { "1" }, context);
				sourceTable = funTable;
				lhsColIndex = 0;
			} else {
				// The expression is an easy to resolve reference in this table.
				sourceTable = (Table) theTable;
				lhsColIndex = sourceTable.FindFieldName(expVar);
				if (lhsColIndex == -1) {
					throw new ApplicationException("Can't find column '" + expVar + "'.");
				}
			}

			// Check that the first column of 'table' is of a compatible type with
			// source table column (lhs_col_index).
			// ISSUE: Should we convert to the correct type via a FunctionTable?
			DataColumnInfo sourceCol = sourceTable.TableInfo[lhsColIndex];
			DataColumnInfo destCol = table.TableInfo[0];
			if (!sourceCol.DataType.IsComparable(destCol.DataType)) {
				throw new ApplicationException("The type of the sub-query expression " +
								sourceCol.DataType + " is incompatible " +
								"with the sub-query " + destCol.DataType +
								".");
			}

			// We now have all the information to solve this query.
			// We work output as follows:
			//   For >, >= type ANY we find the lowest value in 'table' and
			//   select from 'source' all the rows that are >, >= than the
			//   lowest value.
			//   For <, <= type ANY we find the highest value in 'table' and
			//   select from 'source' all the rows that are <, <= than the
			//   highest value.
			//   For = type ANY we use same method from INHelper.
			//   For <> type ANY we iterate through 'source' only including those
			//   rows that a <> query on 'table' returns size() != 0.

			IList<long> selectRows;
			if (op.IsEquivalent(Operator.Greater) ||
				op.IsEquivalent(Operator.GreaterOrEqual)) {
				// Select the first from the set (the lowest value),
				DataObject lowestCell = table.GetFirstCell(0);
				// Select from the source table all rows that are > or >= to the
				// lowest cell,
				selectRows = sourceTable.SelectRows(lhsColIndex, op, lowestCell).ToList();
			} else if (op.IsEquivalent(Operator.Smaller) ||
				op.IsEquivalent(Operator.SmallerOrEqual)) {
				// Select the last from the set (the highest value),
				DataObject highestCell = table.GetLastCell(0);
				// Select from the source table all rows that are < or <= to the
				// highest cell,
				selectRows = sourceTable.SelectRows(lhsColIndex, op, highestCell).ToList();
			} else if (op.IsEquivalent(Operator.Equal)) {
				// Equiv. to IN
				selectRows = sourceTable.In(table, lhsColIndex, 0);
			} else if (op.IsEquivalent(Operator.NotEqual)) {
				// Select the value that is the same of the entire column
				DataObject cell = table.GetSingleCell(0);
				if (cell != null) {
					// All values from 'source_table' that are <> than the given cell.
					selectRows = sourceTable.SelectRows(lhsColIndex, op, cell).ToList();
				} else {
					// No, this means there are different values in the given set so the
					// query evaluates to the entire table.
					return theTable;
				}
			} else {
				throw new ApplicationException("Don't understand operator '" + op + "' in ANY.");
			}

			// Make into a table to return.
			VirtualTable rtable = new VirtualTable((Table)theTable);
			rtable.Set((Table)theTable, selectRows);

			return rtable;
		}

		public static ITable All(this ITable table, IQueryContext context, Expression expression, Operator op, ITable other) {
			// Check the table only has 1 column
			if (other.TableInfo.ColumnCount != 1)
				throw new ApplicationException("Input table <> 1 columns.");

			// Handle trivial case of no entries to select from
			if (table.RowCount == 0)
				return table;

			var theTable = table as Table;
			if (theTable == null)
				return table;

			// If 'table' is empty then we return the complete set.  ALL { empty set }
			// is always true.
			if (other.RowCount == 0)
				return table;

			// Is the lhs expression a constant?
			if (expression.IsConstant()) {
				// We know lhs is a constant so no point passing arguments,
				DataObject value = expression.Evaluate(context);
				bool comparedToTrue;

				// The various operators
				if (op.IsEquivalent(Operator.Greater) || 
					op.IsEquivalent(Operator.GreaterOrEqual)) {
					// Find the maximum value in the table
					DataObject cell = other.GetLastCell(0);
					comparedToTrue = CompareCells(value, cell, op);
				} else if (op.IsEquivalent(Operator.Smaller) ||
					op.IsEquivalent(Operator.SmallerOrEqual)) {
					// Find the minimum value in the table
					DataObject cell = other.GetFirstCell(0);
					comparedToTrue = CompareCells(value, cell, op);
				} else if (op.IsEquivalent(Operator.Equal)) {
					// Only true if rhs is a single value
					DataObject cell = other.GetSingleCell(0);
					comparedToTrue = (cell != null && CompareCells(value, cell, op));
				} else if (op.IsEquivalent(Operator.NotEqual)) {
					// true only if lhs_cell is not found in column.
					comparedToTrue = !other.ColumnContainsValue(0, value);
				} else {
					throw new ApplicationException("Don't understand operator '" + op + "' in ALL.");
				}

				// If matched return this table
				if (comparedToTrue)
					return table;

				// No entries matches so return an empty table.
				return table.EmptySelect();
			}

			Table sourceTable;
			int colIndex;
			// Is the lhs expression a single variable?
			ObjectName expVar = expression.AsVariable();
			// NOTE: It'll be less common for this part to be called.
			if (expVar == null) {
				// This is a complex expression so make a FunctionTable as our new
				// source.
				DatabaseQueryContext dbContext = (DatabaseQueryContext)context;
				FunctionTable funTable = new FunctionTable((Table)table, new[] { expression }, new [] { "1" }, dbContext);
				sourceTable = funTable;
				colIndex = 0;
			} else {
				// The expression is an easy to resolve reference in this table.
				sourceTable = (Table) table;

				colIndex = sourceTable.FindFieldName(expVar);
				if (colIndex == -1)
					throw new ApplicationException("Can't find column '" + expVar + "'.");
			}

			// Check that the first column of 'table' is of a compatible type with
			// source table column (lhs_col_index).
			// ISSUE: Should we convert to the correct type via a FunctionTable?
			DataColumnInfo sourceCol = sourceTable.TableInfo[colIndex];
			DataColumnInfo destCol = other.TableInfo[0];
			if (!sourceCol.DataType.IsComparable(destCol.DataType))
				throw new ApplicationException("The type of the sub-query expression " +
				                               sourceCol.DataType + " is incompatible " +
				                               "with the sub-query " + destCol.DataType +
				                               ".");

			// We now have all the information to solve this query.
			// We work output as follows:
			//   For >, >= type ALL we find the highest value in 'table' and
			//   select from 'source' all the rows that are >, >= than the
			//   highest value.
			//   For <, <= type ALL we find the lowest value in 'table' and
			//   select from 'source' all the rows that are <, <= than the
			//   lowest value.
			//   For = type ALL we see if 'table' contains a single value.  If it
			//   does we select all from 'source' that equals the value, otherwise an
			//   empty table.
			//   For <> type ALL we use the 'not in' algorithm.

			IList<long> selectList;
			if (op.IsEquivalent(Operator.Greater) || 
				op.IsEquivalent(Operator.GreaterOrEqual)) {
				// Select the last from the set (the highest value),
				DataObject highestCell = other.GetLastCell(0);
				// Select from the source table all rows that are > or >= to the
				// highest cell,
				selectList = sourceTable.SelectRows(colIndex, op, highestCell).ToList();
			} else if (op.IsEquivalent(Operator.Smaller) ||
				op.IsEquivalent(Operator.SmallerOrEqual)) {
				// Select the first from the set (the lowest value),
				DataObject lowestCell = other.GetFirstCell(0);
				// Select from the source table all rows that are < or <= to the
				// lowest cell,
				selectList = sourceTable.SelectRows(colIndex, op, lowestCell).ToList();
			} else if (op.IsEquivalent(Operator.Equal)) {
				// Select the single value from the set (if there is one).
				DataObject singleCell = other.GetSingleCell(0);
				if (singleCell != null) {
					// Select all from source_table all values that = this cell
					selectList = sourceTable.SelectRows(colIndex, op, singleCell).ToList();
				} else {
					// No single value so return empty set (no value in LHS will equal
					// a value in RHS).
					return table.EmptySelect();
				}
			} else if (op.IsEquivalent(Operator.NotEqual)) {
				// Equiv. to NOT IN
				selectList = sourceTable.NotIn(other, colIndex, 0).ToList();
			} else {
				throw new ApplicationException("Don't understand operator '" + op + "' in ALL.");
			}

			// Make into a table to return.
			VirtualTable rtable = new VirtualTable(theTable);
			rtable.Set((Table)table, selectList);
			return rtable;
		}

		public static IList<long> In(this ITable table, ITable table2, int column1, int column2) {
			// First pick the the smallest and largest table.  We only want to iterate
			// through the smallest table.
			// NOTE: This optimisation can't be performed for the 'not_in' command.

			ITable smallTable;
			ITable largeTable;
			int smallColumn;
			int largeColumn;

			if (table.RowCount < table2.RowCount) {
				smallTable = table;
				largeTable = table2;

				smallColumn = column1;
				largeColumn = column2;

			} else {
				smallTable = table2;
				largeTable = table;

				smallColumn = column2;
				largeColumn = column1;
			}

			// Iterate through the small table's column.  If we can find identical
			// cells in the large table's column, then we should include the row in our
			// final result.

			BlockIndex resultRows = new BlockIndex();
			IEnumerator<long> e = smallTable.GetRowEnumerator();
			Operator op = Operator.Equal;

			while (e.MoveNext()) {
				long smallRowIndex = e.Current;
				DataObject cell = smallTable.GetValue(smallColumn, smallRowIndex);

				IEnumerable<long> selectedSet = largeTable.SelectRows(largeColumn, op, cell);

				// We've found cells that are IN both columns,

				if (selectedSet.Any()) {
					// If the large table is what our result table will be based on, append
					// the rows selected to our result set.  Otherwise add the index of
					// our small table.  This only works because we are performing an
					// EQUALS operation.

					if (largeTable == table) {
						// Only allow unique rows into the table set.
						foreach (var set in selectedSet) {
							if (!resultRows.UniqueInsertSort((int)set))
								break;
						}
					} else {
						// Don't bother adding in sorted order because it's not important.
						resultRows.Add((int)smallRowIndex);
					}
				}
			}

			return resultRows.Cast<long>().ToList();
		}

		public static IList<long> In(this ITable table, ITable table2, int[] t1Cols, int[] t2Cols) {
			if (t1Cols.Length > 1)
				throw new NotSupportedException("Multi-column 'in' not supported yet.");

			return table.In(table2, t1Cols[0], t2Cols[0]);
		}

		public static IEnumerable<long> NotIn(this ITable table, ITable table2, int col1, int col2) {
			// Handle trivial cases
			long t2RowCount = table2.RowCount;
			if (t2RowCount == 0)
				// No rows so include all rows.
				return table.SelectAll(col1);

			if (t2RowCount == 1) {
				// 1 row so select all from table1 that doesn't equal the value.
				IEnumerator<long> en = table2.GetRowEnumerator();
				if (!en.MoveNext())
					throw new InvalidOperationException("Cannot iterate through table rows.");

				DataObject cell = table2.GetValue(col2, en.Current);
				return table.SelectRows(col1, Operator.NotEqual, cell);
			}

			// Iterate through table1's column.  If we can find identical cell in the
			// tables's column, then we should not include the row in our final
			// result.
			List<long> resultRows = new List<long>();
			IEnumerator<long> e = table.GetRowEnumerator();

			while (e.MoveNext()) {
				long rowIndex = e.Current;
				DataObject cell = table.GetValue(col1, rowIndex);

				IEnumerable<long> selectedSet = table2.SelectRows(col2, Operator.Equal, cell);

				// We've found a row in table1 that doesn't have an identical cell in
				// table2, so we should include it in the result.

				if (selectedSet.Any())
					resultRows.Add(rowIndex);
			}

			return resultRows.AsReadOnly();
		}

		public static ITable Distinct(this ITable table, int[] columns) {
			var theTable = table as Table;
			if (theTable == null)
				return table;

			List<long> resultList = new List<long>();
			IList<long> rowList = theTable.OrdereddRows(columns);

			long rowCount = rowList.Count;
			long previousRow = -1;
			for (int i = 0; i < rowCount; ++i) {
				long rowIndex = rowList[i];

				if (previousRow != -1) {

					bool equal = true;
					// Compare cell in column in this row with previous row.
					for (int n = 0; n < columns.Length && equal; ++n) {
						DataObject c1 = table.GetValue(columns[n], rowIndex);
						DataObject c2 = table.GetValue(columns[n], previousRow);
						equal = (c1.CompareTo(c2) == 0);
					}

					if (!equal) {
						resultList.Add(rowIndex);
					}
				} else {
					resultList.Add(rowIndex);
				}

				previousRow = rowIndex;
			}

			// Return the new table with distinct rows only.
			VirtualTable vt = new VirtualTable(theTable);
			vt.Set(theTable, resultList);

			return vt;
		}

		#region SelectAllEnumerable

		class SelectAllEnumerable : IEnumerable<long> {
			private readonly IEnumerator<long> rowEnumerator;

			public SelectAllEnumerable(IEnumerator<long> rowEnumerator) {
				this.rowEnumerator = rowEnumerator;
			}

			public IEnumerator<long> GetEnumerator() {
				return rowEnumerator;
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}

		#endregion
	}
}