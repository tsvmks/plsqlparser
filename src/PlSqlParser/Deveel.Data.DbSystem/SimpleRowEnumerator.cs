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

using Deveel.Math;

namespace Deveel.Data.DbSystem {
	public sealed class SimpleRowEnumerator : IEnumerator<long> {
		private ITable table;
		private long index = -1;
		private long rowCountStore;

		public SimpleRowEnumerator(ITable table) {
			this.table = table;
			rowCountStore = table.RowCount;
		}

		/// <inheritdoc/>
		public bool MoveNext() {
			return (++index < rowCountStore);
		}

		/// <inheritdoc/>
		public void Reset() {
			index = -1;
			rowCountStore = table.RowCount;
		}

		object IEnumerator.Current {
			get { return Current; }
		}

		public long Current {
			get { return index; }
		}

		public void Dispose() {
			table = null;
		}
	}
}