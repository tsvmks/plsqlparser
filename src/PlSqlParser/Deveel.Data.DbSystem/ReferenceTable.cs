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

namespace Deveel.Data.DbSystem {
	public sealed class ReferenceTable : FilterTable, IRootTable {
		/// <summary>
		/// This represents the new name of the table.
		/// </summary>
		private readonly ObjectName table_name;

		/// <summary>
		/// The modified DataTableInfo object for this reference.
		/// </summary>
		private readonly DataTableInfo modifiedTableInfo;


		internal ReferenceTable(Table table, ObjectName tname)
			: base(table) {
			table_name = tname;

			// Create a modified table info based on the parent info.
			modifiedTableInfo = table.TableInfo.Clone(tname);
			modifiedTableInfo.IsReadOnly = true;
		}

		internal ReferenceTable(Table table, DataTableInfo info)
			: base(table) {
			table_name = info.TableName;

			modifiedTableInfo = info;
		}

		public ObjectName TableName {
			get { return table_name; }
		}

		/// <inheritdoc/>
		public override DataTableInfo TableInfo {
			get { return modifiedTableInfo; }
		}

		/// <inheritdoc/>
		public override int FindFieldName(ObjectName v) {
			ObjectName tableName = v.Parent;
			if (tableName != null && tableName.Equals(TableName)) {
				return TableInfo.FastFindColumnName(v.Name);
			}
			return -1;
		}

		/// <inheritdoc/>
		public override ObjectName GetResolvedVariable(int column) {
			return new ObjectName(TableName, TableInfo[column].Name);
		}

		/// <inheritdoc/>
		public bool Equals(IRootTable table) {
			return (this == table);
		}
	}
}