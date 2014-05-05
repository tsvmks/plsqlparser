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
using System.CodeDom;

namespace Deveel.Data.DbSystem {
	class JoinedTableInfo : DataTableInfo {
		private readonly DataTableInfo[] sourceInfos;

		// These two arrays are lookup tables created in the constructor.  They allow
		// for quick resolution of where a given column should be 'routed' to in
		// the ancestors.

		/// <summary>
		/// Maps the column number in this table to the reference_list array to route to.
		/// </summary>
		private int[] columnTable;

		/// <summary>
		/// Gives a column filter to the given column to route correctly to the ancestor.
		/// </summary>
		private int[] columnFilter;

		public JoinedTableInfo(ObjectName name, DataTableInfo[] tableInfos) 
			: base(name) {
			sourceInfos = tableInfos;
			CallInit(tableInfos);
		}

		/// <summary>
		/// Maps the column number in this table to the reference_list array to route to.
		/// </summary>
		protected int[] ColumnTable {
			get { return columnTable; }
		}

		/// <summary>
		/// Gives a column filter to the given column to route correctly to the ancestor.
		/// </summary>
		protected int[] ColumnFilter {
			get { return columnFilter; }
		}


		private void CallInit(DataTableInfo[] tableInfos) {
			Init(tableInfos);
		}

		internal int IndexOfTable(int column) {
			return columnTable[column];
		}

		protected virtual void Init(DataTableInfo[] tableInfos) {
			// Generate look up tables for column_table and column_filter information
			int colCount = 0;
			for (int i = 0; i < tableInfos.Length; i++) {
				colCount += tableInfos[i].ColumnCount;
			}

			columnTable = new int[colCount];
			columnFilter = new int[colCount];
			int index = 0;
			for (int i = 0; i < tableInfos.Length; ++i) {
				DataTableInfo curTableInfo = tableInfos[i];
				int refColCount = curTableInfo.ColumnCount;

				// For each column
				for (int n = 0; n < refColCount; ++n) {
					columnFilter[index] = n;
					columnTable[index] = i;
					++index;

					// Add this column to the data table info of this table.
					var sourceColumn = curTableInfo[n];
					var newColumn = NewColumn(sourceColumn.Name, sourceColumn.DataType);
					newColumn.DefaultExpression = sourceColumn.DefaultExpression;
					newColumn.IsNullable = sourceColumn.IsNullable;
					AddColumn(newColumn);
				}
			}

			IsReadOnly = true;			
		}

		public override int IndexOfColumn(string columnName) {
			int colIndex = 0;
			for (int i = 0; i < sourceInfos.Length; ++i) {
				int col = sourceInfos[i].IndexOfColumn(columnName);
				if (col != -1)
					return col + colIndex;

				colIndex += sourceInfos[i].ColumnCount;
			}

			return -1;
		}

		public int AdjustColumnOffset(int column) {
		return 	columnFilter[column];
		}
	}
}