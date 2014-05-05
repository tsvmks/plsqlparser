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

namespace Deveel.Data.DbSystem {
	class VirtualTable : JoinedTable {
		private IList<long>[] rowList;
		private long rowCount;

		public VirtualTable(Table[] tables)
			: base(tables) {
		}

		public VirtualTable(Table table)
			: base(table) {
		}

		protected VirtualTable() {
		}

		protected override void Init(Table[] tables) {
			base.Init(tables);

			int tableCount = tables.Length;
			rowList = new IList<long>[tableCount];
			for (int i = 0; i < tableCount; ++i) {
				rowList[i] = new List<long>();
			}
		}

		protected IList<long>[] ReferenceRows {
			get { return rowList; }
		}

		/// <inheritdoc/>
		public override long RowCount {
			get { return rowCount; }
		}

		internal void Set(ITable table, IEnumerable<long> rows) {
			rowList[0] = new List<long>(rows);
			rowCount = rowList[0].Count;
		}

		internal void Set(Table[] tables, IEnumerable<long>[] rows) {
			for (int i = 0; i < tables.Length; ++i) {
				rowList[i] = new List<long>(rows[i]);
			}
			if (rows.Length > 0) {
				rowCount = rowList[0].Count;
			}
		}

		protected override void ResolveAllRowsForTableAt(IList<long> rowSet, int tableNum) {
			IList<long> curRowList = rowList[tableNum];
			for (int n = rowSet.Count - 1; n >= 0; --n) {
				long aa = rowSet[n];
				long bb = curRowList[(int) aa];
				rowSet[n] = bb;
			}
		}

		protected override long ResolveRowForTableAt(long rowNumber, int tableNum) {
			return rowList[tableNum][(int)rowNumber];
		}
	}
}