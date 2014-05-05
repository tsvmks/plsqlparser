using System.Collections;
using System.Collections.Generic;

using Deveel.Data.Index;

namespace Deveel.Data.DbSystem {
	public sealed class TestTable : ITable {
		private readonly List<DataObject[]> rows;
		private readonly DataTableInfo tableInfo;

		public TestTable(DataTableInfo tableInfo) {
			this.tableInfo = tableInfo;
			rows = new List<DataObject[]>();
		}

		public DataTableInfo TableInfo {
			get { return tableInfo; }
		}

		public long RowCount {
			get { return rows.Count; }
		}

		public IEnumerator<long> GetRowEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		public DataObject GetValue(int column, long row) {
			var rowObj = rows[(int) row];
			return rowObj[column];
		}

		public SelectableScheme GetScheme(int column) {
			return new BlindSearch(this, column);
		}

		public long NewRow() {
			return AddRow(new DataObject[TableInfo.ColumnCount]);
		}

		public long AddRow(DataObject[] row) {
			rows.Add(row);
			return rows.Count - 1;
		}

		public void SetValue(int column, long row, DataObject value) {
			DataObject[] rowObject;
			if (row >= rows.Count) {
				rowObject = new DataObject[TableInfo.ColumnCount];
			} else {
				rowObject = rows[(int) row];
			}

			rowObject[column] = value;
		}

		public void SetValue(string columnName, long row, DataObject value) {
			SetValue(TableInfo.IndexOfColumn(columnName), row, value);
		}

		public void SetValue(int column, DataObject value) {
			SetValue(column, rows.Count - 1, value);
		}

		public void SetValue(string columnName, DataObject value) {
			SetValue(TableInfo.IndexOfColumn(columnName), value);
		}

		public IEnumerator<DataRow> GetEnumerator() {
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}