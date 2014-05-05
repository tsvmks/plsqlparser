﻿// 
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

using Deveel.Data.Types;

namespace Deveel.Data.DbSystem {
	[Serializable]
	public class DataTableInfo : IEnumerable<DataColumnInfo> {
		private readonly List<DataColumnInfo> columns;
		private readonly Dictionary<string, int> columnNamesCache;
 
		public DataTableInfo(string schemaName, string name)
			: this(new ObjectName(new ObjectName(schemaName), name)) {
		}

		public DataTableInfo(ObjectName name) {
			if (name == null)
				throw new ArgumentNullException("name");

			Name = name;

			columns = new List<DataColumnInfo>();
			columnNamesCache = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
		}

		public ObjectName Name { get; private set; }

		public bool IsReadOnly { get; set; }

		public DataColumnInfo this[int offset] {
			get { return columns[offset]; }
		}

		public virtual int ColumnCount {
			get { return columns.Count; }
		}

		public DataColumnInfo NewColumn(string name, DataType type) {
			AssertNotReadOnly();

			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (type == null)
				throw new ArgumentNullException("type");

			if (HasColumn(name))
				throw new InvalidOperationException(String.Format("A column named {0} already exists in table {1}.", name, Name));

			return new DataColumnInfo(this, name, type);
		}

		private void AssertNotReadOnly() {
			if (IsReadOnly)
				throw new InvalidOperationException(String.Format("Table {0} is read-only.", Name));
		}

		public void AddColumn(DataColumnInfo column) {
			if (column == null)
				throw new ArgumentNullException("column");

			if (!Name.Equals(column.TableInfo.Name))
				throw new ArgumentException("The column was not generated by this table.", "column");

			// TODO: Additional checks to see that it's possible to add the column...

			columns.Add(column);
		}

		public DataColumnInfo AddColumn(string name, DataType type) {
			return AddColumn(name, type, true);
		}

		public DataColumnInfo AddColumn(string name, DataType type, bool nullable) {
			var column = NewColumn(name, type);
			column.IsNullable = nullable;
			AddColumn(column);
			return column;
		}

		public virtual int IndexOfColumn(string columnName) {
			if (String.IsNullOrEmpty(columnName))
				throw new ArgumentNullException("columnName");

			int offset;
			if (!columnNamesCache.TryGetValue(columnName, out offset)) {
				offset = -1;

				for (int i = 0; i < columns.Count; i++) {
					var column = columns[i];
					if (columnName.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)) {
						offset = i;
						break;
					}
				}

				if (offset != -1)
					columnNamesCache[columnName] = offset;
			}

			return offset;
		}

		public int IndexOfColumn(ObjectName columnName) {
			if (columnName.Parent != null &&
			    !columnName.Parent.Equals(Name))
				return -1;

			return IndexOfColumn(columnName.Name);
		}

		public bool HasColumn(string columnName) {
			return IndexOfColumn(columnName) != -1;
		}

		public ObjectName ResolveColumnName(string columnName) {
			return new ObjectName(Name, columnName);
		}

		public int[] IndexOfColumns(ObjectName[] columnNames) {
			var index = new int[columnNames.Length];
			for (int i = 0; i < columnNames.Length; i++) {
				index[i] = IndexOfColumn(columnNames[i]);
			}

			return index;
		}

		public DataType GetColumnType(ObjectName columnName) {
			var index = IndexOfColumn(columnName);
			if (index == -1)
				return null;

			return columns[index].DataType;
		}

		internal void CopyColumnsTo(DataTableInfo tableInfo) {
			foreach (var column in columns) {
				var newColumn = new DataColumnInfo(tableInfo, column.Name, column.DataType) {
					DefaultExpression = column.DefaultExpression,
					IsNullable = column.IsNullable
				};

				tableInfo.AddColumn(newColumn);
			}
		}

		public IEnumerator<DataColumnInfo> GetEnumerator() {
			return columns.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}