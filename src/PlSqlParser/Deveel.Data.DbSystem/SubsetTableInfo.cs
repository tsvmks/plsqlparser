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

namespace Deveel.Data.DbSystem {
	class SubsetTableInfo : DataTableInfo {
		private int[] column_map;
		private ObjectName[] aliases;


		public SubsetTableInfo(ObjectName name) 
			: base(name) {
		}

		public override int ColumnCount {
			get { return aliases.Length; }
		}

		public void Setup(int[] mapping, ObjectName[] aliases) {
			column_map = mapping;
			this.aliases = aliases;
		}

		public int MapColumn(int column) {
			return column_map[column];
		}
	}
}