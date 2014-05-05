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
	public abstract class BaseDataTable : RootTable {
		/// <summary>
		/// The number of rows in the table.
		/// </summary>
		private int rowCount;

		/// <summary>
		/// A list of schemes for managing the data relations of each column.
		/// </summary>
		private SelectableScheme[] columnScheme;

		protected BaseDataTable(IDatabase database) {
			Database = database;
		}

		protected IDatabase Database { get; private set; }

		public override long RowCount {
			get { return rowCount; }
		}

		protected virtual SelectableScheme GetRootColumnScheme(int column) {
			return columnScheme[column];
		}

		protected void ClearColumnScheme(int column) {
			columnScheme[column] = null;
		}

		protected void BlankSelectableSchemes() {
			BlankSelectableSchemes(0);
		}

		protected virtual void BlankSelectableSchemes(int type) {
			columnScheme = new SelectableScheme[TableInfo.ColumnCount];
			for (int i = 0; i < columnScheme.Length; ++i) {
				if (type == 0) {
					columnScheme[i] = new InsertSearch(this, i);
				} else if (type == 1) {
					columnScheme[i] = new BlindSearch(this, i);
				}
			}
		}

		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {
			SelectableScheme scheme = GetRootColumnScheme(column);

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

		internal override RawTableInformation ResolveToRawTable(RawTableInformation info) {
			List<long> rowSet = new List<long>();
			IEnumerator<long> e = GetRowEnumerator();
			while (e.MoveNext()) {
				rowSet.Add(e.Current);
			}
			info.Add(this, rowSet);
			return info;
		}

		/// <summary>
		/// This is called when a row is in the table, and the SelectableScheme
		/// objects for each column need to be notified of the rows existance,
		/// therefore build up the relational model for the columns.
		/// </summary>
		/// <param name="rowNumber"></param>
		internal void AddRowToColumnSchemes(int rowNumber) {
			int colCount = TableInfo.ColumnCount;
			DataTableInfo tableInfo = TableInfo;
			for (int i = 0; i < colCount; ++i) {
				if (tableInfo[i].DataType.IsIndexable) {
					SelectableScheme ss = GetRootColumnScheme(i);
					ss.Insert(rowNumber);
				}
			}
		}
	}
}