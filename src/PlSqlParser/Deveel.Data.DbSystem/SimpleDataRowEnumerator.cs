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

namespace Deveel.Data.DbSystem {
	public class SimpleDataRowEnumerator : IEnumerator<DataRow> {
		private readonly ITable table;
		private readonly IEnumerator<long> rowIndexEnum;

		public SimpleDataRowEnumerator(ITable table, IEnumerator<long> rowIndexEnum) {
			this.table = table;
			this.rowIndexEnum = rowIndexEnum;
		}

		public void Dispose() {
			rowIndexEnum.Dispose();
		}

		public bool MoveNext() {
			return rowIndexEnum.MoveNext();
		}

		public void Reset() {
			rowIndexEnum.Reset();
		}

		public DataRow Current {
			get { throw new NotImplementedException(); }
		}

		object IEnumerator.Current {
			get { return Current; }
		}
	}
}