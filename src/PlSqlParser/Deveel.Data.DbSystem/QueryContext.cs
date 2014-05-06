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
using System.Globalization;

namespace Deveel.Data.DbSystem {
	public abstract class QueryContext : IQueryContext {
		private Dictionary<string, ITable> markedTables;

		public virtual IDatabaseConnection Connection {
			get { return null; }
		}

		public void ClearCache() {
			if (markedTables != null)
				markedTables.Clear();
		}

		public ITable GetCachedNode(long id) {
			return GetMarkedTable(id.ToString(CultureInfo.InvariantCulture));
		}

		public void PutCachedNode(long id, ITable table) {
			AddMarkedTable(id.ToString(CultureInfo.InvariantCulture), table);
		}

		public void AddMarkedTable(string markName, ITable table) {
			if (markedTables == null)
				markedTables = new Dictionary<string, ITable>();

			markedTables.Add(markName, table);
		}

		public ITable GetMarkedTable(string markerName) {
			if (markedTables == null)
				return null;

			ITable table;
			if (!markedTables.TryGetValue(markerName, out table))
				return null;

			return table;
		}

		public virtual ITable GetTable(ObjectName tableName) {
			if (Connection == null)
				return null;

			// TODO: Add Selected Table
			return Connection.GetTable(tableName);
		}
	}
}