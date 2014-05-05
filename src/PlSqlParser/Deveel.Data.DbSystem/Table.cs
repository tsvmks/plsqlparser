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

namespace Deveel.Data.DbSystem {
	public abstract class Table : ITable {
		public abstract long RowCount { get; }

		public virtual IEnumerator<long> GetRowEnumerator() {
			throw new NotImplementedException();
		}

		public abstract DataObject GetValue(int column, long row);

		public SelectableScheme GetScheme(int column) {
			return GetSelectableSchemeFor(column, column, this);
		}

		internal abstract SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table);

		internal abstract void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor);

		internal abstract RawTableInformation ResolveToRawTable(RawTableInformation info);

		public abstract DataTableInfo TableInfo { get; }

		public IEnumerator<DataRow> GetEnumerator() {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}