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
using Deveel.Data.Sql;

namespace Deveel.Data.DbSystem {
	public class CompositeTable : Table, IRootTable {
		private readonly Table masterTable;
		private readonly Table[] compositeTables;
		private IList<long>[] tableIndexes;

		private readonly SelectableScheme[] columnScheme;

		public CompositeTable(Table masterTable, Table[] compositeList) {
			this.masterTable = masterTable;
			compositeTables = compositeList;
			columnScheme = new SelectableScheme[masterTable.TableInfo.ColumnCount];
		}

		public CompositeTable(Table[] compositeList)
			: this(compositeList[0], compositeList) {
		}

		public override DataTableInfo TableInfo {
			get { return masterTable.TableInfo; }
		}

		public override long RowCount {
			get {
				int rowCount = 0;
				for (int i = 0; i < tableIndexes.Length; ++i) {
					rowCount += tableIndexes[i].Count;
				}
				return rowCount;
			}
		}

		public void SetupIndexesForCompositeFunction(CompositeFunction function, bool all) {
			int size = compositeTables.Length;
			tableIndexes = new IList<long>[size];

			if (function == CompositeFunction.Union) {
				// Include all row sets in all tables
				for (int i = 0; i < size; ++i) {
					tableIndexes[i] = new List<long>(compositeTables[i].SelectAll());
				}

				if (!all)
					RemoveDuplicates(false);
			} else {
				throw new ApplicationException("Unrecognised composite function");
			}
		}

		private void RemoveDuplicates(bool preSorted) {
			throw new NotImplementedException();
		}

		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {
			SelectableScheme scheme = columnScheme[column];
			if (scheme == null) {
				scheme = new BlindSearch(this, column);
				columnScheme[column] = scheme;
			}

			// If we are getting a scheme for this table, simple return the information
			// from the column_trees Vector.
			if (table == this)
				return scheme;

			// Otherwise, get the scheme to calculate a subset of the given scheme.
			return scheme.GetSubsetScheme(table, originalColumn);
		}

		/// <inheritdoc/>
		internal override void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor) {
			if (ancestor != this)
				throw new Exception("Method routed to incorrect table ancestor.");
		}

		/// <inheritdoc/>
		internal override RawTableInformation ResolveToRawTable(RawTableInformation info) {
			List<long> rowSet = new List<long>();
			IEnumerator<long> e = GetRowEnumerator();
			while (e.MoveNext()) {
				rowSet.Add(e.Current);
			}
			info.Add(this, rowSet);
			return info;
		}

		public override DataObject GetValue(int column, long row) {
			for (int i = 0; i < tableIndexes.Length; ++i) {
				IList<long> ivec = tableIndexes[i];
				int sz = ivec.Count;
				if (row < sz)
					return compositeTables[i].GetValue(column, ivec[(int)row]);
				row -= sz;
			}
			throw new ApplicationException("Row '" + row + "' out of bounds.");
		}

		public bool Equals(IRootTable table) {
			return (this == table);
		}
	}
}