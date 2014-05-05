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
using Deveel.Data.Types;

namespace Deveel.Data.DbSystem {
	public class FilterTable : Table {
		private readonly ChildTableInfo tableInfo;
		private SelectableScheme[] columnScheme;

		protected FilterTable(Table parent) {
			tableInfo = new ChildTableInfo(parent.TableInfo);
			Parent = parent;
		}

		public override DataTableInfo TableInfo {
			get { return tableInfo; }
		}

		/// <summary>
		/// Returns the parent table.
		/// </summary>
		protected Table Parent { get; private set; }

		public override long RowCount {
			get { return Parent.RowCount; }
		}

		internal override RawTableInformation ResolveToRawTable(RawTableInformation info) {
			return Parent.ResolveToRawTable(info);
		}

		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {
			if (columnScheme == null) {
				columnScheme = new SelectableScheme[Parent.TableInfo.ColumnCount];
			}

			// Is there a local scheme available?
			SelectableScheme scheme = columnScheme[column];
			if (scheme == null) {
				// If we are asking for the selectable schema of this table we must
				// tell the parent we are looking for its selectable scheme.
				Table t = table;
				if (table == this) {
					t = Parent;
				}

				// Scheme is not cached in this table so ask the parent.
				scheme = Parent.GetSelectableSchemeFor(column, originalColumn, t);
				if (table == this) {
					columnScheme[column] = scheme;
				}
			} else {
				// If this has a cached scheme and we are in the correct domain then
				// return it.
				if (table == this) {
					return scheme;
				} else {
					// Otherwise we must calculate the subset of the scheme
					return scheme.GetSubsetScheme(table, originalColumn);
				}
			}
			return scheme;
		}

		internal override void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor) {
			if (ancestor == this || ancestor == Parent)
				return;

			Parent.SetToRowTableDomain(column, rowSet, ancestor);
		}

		public override DataObject GetValue(int column, long row) {
			return Parent.GetValue(column, row);
		}

		public override IEnumerator<long> GetRowEnumerator() {
			return Parent.GetRowEnumerator();
		}
	}
}