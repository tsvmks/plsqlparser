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
	public sealed class SubsetColumnTable : FilterTable, IRootTable {
		private readonly SubsetTableInfo subsetTableInfo;

		public SubsetColumnTable(Table parent, int[] mapping, ObjectName[] aliases)
			: base(parent) {
			int[] reverseColumnMap = new int[Parent.TableInfo.ColumnCount];
			for (int i = 0; i < reverseColumnMap.Length; ++i) {
				reverseColumnMap[i] = -1;
			}

			DataTableInfo parentInfo = Parent.TableInfo;

			subsetTableInfo = new SubsetTableInfo(parentInfo.Name);

			for (int i = 0; i < mapping.Length; ++i) {
				int mapTo = mapping[i];
				DataColumnInfo colInfo = Parent.TableInfo[mapTo];
				var newColumn = subsetTableInfo.NewColumn(aliases[i].Name, colInfo.DataType);
				newColumn.DefaultExpression = colInfo.DefaultExpression;
				newColumn.IsNullable = colInfo.IsNullable;

				subsetTableInfo.AddColumn(colInfo);
				reverseColumnMap[mapTo] = i;
			}

			subsetTableInfo.Setup(mapping, aliases);
			subsetTableInfo.IsReadOnly = true;
		}


		/// <inheritdoc/>
		public override DataTableInfo TableInfo {
			get { return subsetTableInfo; }
		}

		/// <inheritdoc/>
		internal override SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table) {

			// We need to map the original_column if the original column is a reference
			// in this subset column table.  Otherwise we leave as is.
			// The reason is because FilterTable pretends the call came from its
			// parent if a request is made on this table.
			int mappedOriginalColumn = originalColumn;
			if (table == this) {
				mappedOriginalColumn = subsetTableInfo.MapColumn(originalColumn);
			}

			return base.GetSelectableSchemeFor(subsetTableInfo.MapColumn(column), mappedOriginalColumn, table);
		}

		/// <inheritdoc/>
		internal override void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor) {

			base.SetToRowTableDomain(subsetTableInfo.MapColumn(column), rowSet, ancestor);
		}

		/// <inheritdoc/>
		public override DataObject GetValue(int column, long row) {
			return Parent.GetValue(subsetTableInfo.MapColumn(column), row);
		}

		// ---------- Implemented from IRootTable ----------

		/// <inheritdoc/>
		public bool Equals(IRootTable table) {
			return (this == table);
		}


		/// <inheritdoc/>
		public override String ToString() {
			String name = "SCT" + GetHashCode();
			return name + "[" + RowCount + "]";
		}
	}
}