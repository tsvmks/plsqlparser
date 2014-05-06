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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql {
	[Serializable]
	public sealed class FromTable : IPreparable {
		public FromTable(ObjectName tableName, ObjectName tableAlias) {
			Name = tableName;
			Alias = tableAlias;
			SubSelect = null;
			IsSubQueryTable = false;
		}

		public FromTable(ObjectName tableName)
			: this(tableName, null) {
		}

		public FromTable(TableSelectExpression select, ObjectName tableAlias) {
			SubSelect = select;
			Name = tableAlias;
			Alias = tableAlias;
			IsSubQueryTable = true;
		}

		public FromTable(TableSelectExpression select) {
			SubSelect = select;
			Name = null;
			Alias = null;
			IsSubQueryTable = true;
		}

		private FromTable() {
		}

		public ObjectName Name { get; private set; }

		public ObjectName Alias { get; private set; }

		internal string UniqueKey { get; set; }

		public bool IsSubQueryTable { get; private set; }

		public TableSelectExpression SubSelect { get; private set; }

		public FromTable Prepare(IExpressionPreparer preparer) {
			var fromTable = new FromTable {
				Name = Name, 
				Alias = 
				Alias, 
				UniqueKey = UniqueKey, 
				IsSubQueryTable = IsSubQueryTable
			};
			if (SubSelect != null)
				fromTable.SubSelect = SubSelect.Prepare(preparer);

			return fromTable;
		}

		object IPreparable.Prepare(IExpressionPreparer preparer) {
			return Prepare(preparer);
		}
	}
}