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
	sealed class TemporaryTable : BaseDataTable {
		/// <summary>
		/// The DataTableInfo object that describes the columns in this table.
		/// </summary>
		private readonly DataTableInfo tableInfo;

		/// <summary>
		/// A list that represents the storage of TObject[] arrays for each row of the table.
		/// </summary>
		private readonly List<DataObject[]> tableStorage;

		private long rowCount;

		///<summary>
		///</summary>
		///<param name="database"></param>
		///<param name="name"></param>
		///<param name="fields"></param>
		public TemporaryTable(IDatabase database, String name, DataColumnInfo[] fields)
			: base(database) {

			tableStorage = new List<DataObject[]>();

			tableInfo = new DataTableInfo(new ObjectName(name));
			
			foreach (DataColumnInfo field in fields) {
				var newColumn = tableInfo.NewColumn(field.Name, field.DataType);
				newColumn.DefaultExpression = field.DefaultExpression;
				newColumn.IsNullable = field.IsNullable;
				tableInfo.AddColumn(newColumn);
			}
			tableInfo.IsReadOnly = true;
		}

		private ObjectName ResolveToVariable(String col_name) {
			return ObjectName.Parse(col_name);
		}

		public override long RowCount {
			get { return rowCount; }
		}

		/// <summary>
		/// Creates a new row where cells can be inserted into.
		/// </summary>
		public void NewRow() {
			tableStorage.Add(new DataObject[TableInfo.ColumnCount]);
			++rowCount;
		}

		public void SetRowCell(DataObject cell, int column, long row) {
			DataObject[] cells = tableStorage[(int)row];
			cells[column] = cell;
		}

		public void SetRowCell(DataObject cell, string col_name) {
			ObjectName v = ResolveToVariable(col_name);
			SetRowCell(cell, TableInfo.IndexOfColumn(v), rowCount - 1);
		}

		public void SetRowObject(DataObject ob, int col_index, long row) {
			SetRowCell(ob, col_index, row);
		}

		public void SetRowObject(DataObject ob, String col_name) {
			ObjectName v = ResolveToVariable(col_name);
			SetRowObject(ob, TableInfo.IndexOfColumn(v));
		}

		public void SetRowObject(DataObject ob, int col_index) {
			SetRowObject(ob, col_index, rowCount - 1);
		}

		public void SetCellFrom(Table table, int src_col, int src_row, string to_col) {
			ObjectName v = ResolveToVariable(to_col);
			DataObject cell = table.GetValue(src_col, src_row);
			SetRowCell(cell, TableInfo.IndexOfColumn(v), rowCount - 1);
		}


		public void SetupAllSelectableSchemes() {
			BlankSelectableSchemes(1);   // <- blind search
			for (int row_number = 0; row_number < rowCount; ++row_number) {
				AddRowToColumnSchemes(row_number);
			}
		}

		public override DataTableInfo TableInfo {
			get { return tableInfo; }
		}

		/// <inheritdoc/>
		public override DataObject GetValue(int column, long row) {
			DataObject[] cells = tableStorage[(int)row];
			DataObject cell = cells[column];
			if (cell == null)
				throw new ApplicationException("NULL cell!  (" + column + ", " + row + ")");

			return cell;
		}

		/// <inheritdoc/>
		public override IEnumerator<long> GetRowEnumerator() {
			return new SimpleRowEnumerator(this);
		}
	}
}