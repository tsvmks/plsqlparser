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

using Deveel.Data.Index;

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// A <see cref="Table"/> class for forming <c>OUTER</c> type results.
	/// </summary>
	/// <remarks>
	/// This takes as its constructor the base table (with no outer
	/// <b>null</b> fields) that is what the result is based on. It is 
	/// then possible to merge in tables that are ancestors.
	/// </remarks>
	class OuterTable : VirtualTable, IRootTable {
		/// <summary>
		/// The merged rows.
		/// </summary>
		private readonly IList<long>[] outerRows;

		/// <summary>
		/// The row count of the outer rows.
		/// </summary>
		private int outerRowCount;

		public OuterTable(Table inputTable) {
			RawTableInformation baseTable = inputTable.ResolveToRawTable(new RawTableInformation());
			Table[] tables = baseTable.GetTables();
			IList<long>[] rows = baseTable.GetRows();

			outerRows = new IList<long>[rows.Length];

			// Set up the VirtualTable with this base table information,
			Init(tables);
			Set(tables, rows);

		}

		/// <summary>
		/// Merges the given table in with this table.
		/// </summary>
		/// <param name="outsideTable"></param>
		public void MergeIn(Table outsideTable) {
			RawTableInformation rawTableInfo = outsideTable.ResolveToRawTable(new RawTableInformation());

			// Get the base information,
			Table[] baseTables = ReferenceTables;
			IList<long>[] baseRows = ReferenceRows;

			// The tables and rows being merged in.
			Table[] tables = rawTableInfo.GetTables();
			IList<long>[] rows = rawTableInfo.GetRows();
			// The number of rows being merged in.
			outerRowCount = rows[0].Count;

			for (int i = 0; i < baseTables.Length; ++i) {
				Table btable = baseTables[i];
				int index = -1;
				for (int n = 0; n < tables.Length && index == -1; ++n) {
					if (btable == tables[n]) {
						index = n;
					}
				}

				// If the table wasn't found, then set 'NULL' to this base_table
				if (index == -1) {
					outerRows[i] = null;
				} else {
					List<long> list = new List<long>(outerRowCount);
					outerRows[i] = list;
					// Merge in the rows from the input table,
					IList<long> toMerge = rows[index];
					if (toMerge.Count != outerRowCount)
						throw new ApplicationException("Wrong size for rows being merged in.");

					list.AddRange(toMerge);
				}
			}
		}

		// ---------- Implemented from DefaultDataTable ----------

		/// <inheritdoc/>
		public override long RowCount {
			get { return base.RowCount + outerRowCount; }
		}

		/// <inheritdoc/>
		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {
			if (ColumnScheme[column] == null) {
				// EFFICIENCY: We implement this with a blind search...
				SelectableScheme scheme = new BlindSearch(this, column);
				ColumnScheme[column] = scheme.GetSubsetScheme(this, column);
			}

			if (table == this)
				return ColumnScheme[column];

			return ColumnScheme[column].GetSubsetScheme(table, originalColumn);
		}

		/// <inheritdoc/>
		public override DataObject GetValue(int column, long row) {
			int tableNum = JoinedTableInfo.IndexOfTable(column);
			Table parentTable = ReferenceTables[tableNum];
			if (row >= outerRowCount) {
				row = ReferenceRows[tableNum][(int)row - outerRowCount];
				return parentTable.GetValue(JoinedTableInfo.AdjustColumnOffset(column), row);
			}

			if (outerRows[tableNum] == null)
				// Special case, handling outer entries (NULL)
				return new DataObject(TableInfo[column].DataType, null);

			row = outerRows[tableNum][(int)row];
			return parentTable.GetValue(JoinedTableInfo.AdjustColumnOffset(column), row);
		}

		/// <inheritdoc/>
		bool IEquatable<IRootTable>.Equals(IRootTable table) {
			return (this == table);
		}
	}
}