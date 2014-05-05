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
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Index;

namespace Deveel.Data.DbSystem {
	abstract class JoinedTable : Table {
		private Table[] referenceList;
		private SelectableScheme[] columnScheme;

		private int sortedAgainstColumn = -1;

		private JoinedTableInfo vtTableInfo;

		protected JoinedTable(Table[] tables) {
			CallInit(tables);
		}

		internal JoinedTable(Table table)
			: this(new Table[] { table }) {
		}

		protected JoinedTable() {
		}

		protected JoinedTableInfo JoinedTableInfo {
			get { return TableInfo as JoinedTableInfo; }
		}

		private void CallInit(Table[] tables) {
			vtTableInfo = new JoinedTableInfo(new ObjectName("#VIRTUAL TABLE#"), tables.Select(x => x.TableInfo).ToArray());
			Init(tables);
		}

		/// <summary>
		/// Helper function for initializing the variables in the joined table.
		/// </summary>
		/// <param name="tables"></param>
		protected virtual void Init(Table[] tables) {
			referenceList = tables;

			int colCount = TableInfo.ColumnCount;
			columnScheme = new SelectableScheme[colCount];
		}

		private IList<long> CalculateRowReferenceList() {
			long size = RowCount;
			List<long> allList = new List<long>((int)size);
			for (int i = 0; i < size; ++i) {
				allList.Add(i);
			}
			return allList;
		}

		protected Table[] ReferenceTables {
			get { return referenceList; }
		}

		internal void OptimisedPostSet(int column) {
			sortedAgainstColumn = column;
		}

		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {

			// First check if the given SelectableScheme is in the column_scheme array
			SelectableScheme scheme = columnScheme[column];
			if (scheme != null) {
				if (table == this)
					return scheme;

				return scheme.GetSubsetScheme(table, originalColumn);
			}

			// If it isn't then we need to calculate it
			SelectableScheme ss;

			// Optimization: The table may be naturally ordered by a column.  If it
			// is we don't try to generate an ordered set.
			if (sortedAgainstColumn != -1 &&
				sortedAgainstColumn == column) {
				InsertSearch isop = new InsertSearch(this, column, CalculateRowReferenceList().Cast<int>());
				isop.RecordUid = false;
				ss = isop;
				columnScheme[column] = ss;
				if (table != this) {
					ss = ss.GetSubsetScheme(table, originalColumn);
				}

			} else {
				// Otherwise we must generate the ordered set from the information in
				// a parent index.
				Table parentTable = referenceList[vtTableInfo.IndexOfTable(column)];
				ss = parentTable.GetSelectableSchemeFor(vtTableInfo.AdjustColumnOffset(column), originalColumn, table);
				if (table == this) {
					columnScheme[column] = ss;
				}
			}

			return ss;
		}

		private RawTableInformation ResolveToRawTable(RawTableInformation info, IList<long> rowSet) {
			if (this is IRootTable) {
				info.Add((IRootTable)this, CalculateRowReferenceList());
			} else {
				for (int i = 0; i < referenceList.Length; ++i) {

					List<long> newRowSet = new List<long>(rowSet);

					// Resolve the rows into the parents indices.
					ResolveAllRowsForTableAt(newRowSet, i);

					Table table = referenceList[i];
					if (table is IRootTable) {
						info.Add((IRootTable)table, newRowSet);
					} else {
						((JoinedTable)table).ResolveToRawTable(info, newRowSet);
					}
				}
			}

			return info;
		}

		internal override RawTableInformation ResolveToRawTable(RawTableInformation info) {
			List<long> allList = new List<long>();
			long size = RowCount;
			for (int i = 0; i < size; ++i) {
				allList.Add(i);
			}
			return ResolveToRawTable(info, allList);
		}

		/// <inheritdoc/>
		internal override void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor) {
			if (ancestor == this)
				return;

			int tableNum = vtTableInfo.IndexOfTable(column);
			Table parentTable = referenceList[tableNum];

			// Resolve the rows into the parents indices.  (MANGLES row_set)
			ResolveAllRowsForTableAt(rowSet, tableNum);

			parentTable.SetToRowTableDomain(vtTableInfo.AdjustColumnOffset(column), rowSet, ancestor);
		}

		public override DataTableInfo TableInfo {
			get { return vtTableInfo; }
		}

		public override DataObject GetValue(int column, long row) {
			int tableNum = vtTableInfo.IndexOfTable(column);
			ITable parentTable = referenceList[tableNum];
			row = ResolveRowForTableAt(row, tableNum);
			return parentTable.GetValue(vtTableInfo.AdjustColumnOffset(column), row);
		}

		public override IEnumerator<long> GetRowEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		/// <summary>
		/// The schemes to describe the entity relation in the given column.
		/// </summary>
		protected SelectableScheme[] ColumnScheme {
			get { return columnScheme; }
		}

		// ---------- Abstract methods ----------

		protected abstract long ResolveRowForTableAt(long rowNumber, int tableNum);

		protected abstract void ResolveAllRowsForTableAt(IList<long> rowSet, int tableNum);
	}
}