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

using Deveel.Data.Types;

namespace Deveel.Data.DbSystem {
	internal class TableVariableResolver : IVariableResolver {
		public TableVariableResolver(ITable table) {
			this.table = table;
		}

		private readonly ITable table;
		private int rowIndex = -1;

		private int FindColumnName(ObjectName variable) {
			int colIndex = table.TableInfo.IndexOfColumn(variable);
			if (colIndex == -1) {
				throw new ApplicationException("Can't find column: " + variable);
			}
			return colIndex;
		}

		// --- Implemented ---

		public int SetId {
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		public DataObject Resolve(ObjectName variable) {
			return table.GetValue(FindColumnName(variable), rowIndex);
		}

		public DataType ReturnType(ObjectName variable) {
			return table.TableInfo.GetColumnType(variable);
		}

	}
}