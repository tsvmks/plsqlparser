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

using Deveel.Data.DbSystem;

namespace Deveel.Data.Transactions {
	public interface ITransaction {
		ObjectName[] GetTables();

		bool TableExists(ObjectName tableName);

		bool RealTableExists(ObjectName tableName);

		ObjectName ResolveToTableName(string currentSchema, string name, bool caseInsensitive);

		ObjectName TryResolveCase(ObjectName tableName);

		DataTableInfo GetTableInfo(ObjectName tableName);

		ITable GetTable(ObjectName tableName);

		string GetTableType(ObjectName tableName);
	}
}