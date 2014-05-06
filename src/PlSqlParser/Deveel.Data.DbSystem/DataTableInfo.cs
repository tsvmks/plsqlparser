// 
//  Copyright 2010  Deveel
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
using System.IO;

using Deveel.Data.Sql.Types;

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// Defines meta information about a table.
	/// </summary>
	/// <remarks>
	/// Every table in the database has a definition that describes how it is stored 
	/// on disk, the column definitions, primary keys/foreign keys, and any 
	/// check constraints.
	/// </remarks>
	[Serializable]
	public sealed class DataTableInfo : ICloneable {
		/// <summary>
		///  A TableName object that represents this data table info.
		/// </summary>
		private readonly ObjectName tableName;

		/// <summary>
		/// The type of table this is (this is the class name of the object that
		/// maintains the underlying database files).
		/// </summary>
		private string tableTypeName;

		/// <summary>
		/// The list of DataColumnInfo objects that are the definitions of each
		/// column input the table.
		/// </summary>
		private List<DataColumnInfo> columns;

		/// <summary>
		/// Set to true if this data table info is immutable.
		/// </summary>
		private bool readOnly;

		///<summary>
		///</summary>
		public DataTableInfo(ObjectName tableName) {
			if (tableName == null)
				throw new ArgumentNullException("tableName");

			this.tableName = tableName;
			columns = new List<DataColumnInfo>();
			tableTypeName = "";
			readOnly = false;
		}

		public DataTableInfo(string schema, string tableName)
			: this(new ObjectName(new ObjectName(schema), tableName)) {
		}

		public DataTableInfo(string tableName)
			: this(ObjectName.Parse(tableName)) {
		}

		public bool IsReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}

		private void CheckMutable() {
			if (IsReadOnly) {
				throw new ApplicationException("Tried to mutate immutable object.");
			}
		}

		public void Dump(TextWriter output) {
			for (int i = 0; i < ColumnCount; ++i) {
				this[i].Dump(output);
				output.WriteLine();
			}
		}


		public string ResolveColumnName(string columnName, bool ignoreCase) {
			// Can we resolve this to a column input the table?
			string found = null;
			foreach (DataColumnInfo columnInfo in columns) {
				// If this is a column name (case ignored) then set the column
				// to the correct cased name.
				if (String.Compare(columnInfo.Name, columnName, ignoreCase) == 0) {
					if (found != null)
						throw new ApplicationException("Ambiguous reference to column '" + columnName + "'");

					found = columnInfo.Name;
				}
			}

			if (found != null)
				return found;

			throw new ApplicationException("Column '" + columnName + "' not found");
		}

		internal void ResolveColumnsInArray(IDatabaseConnection connection,IList<string> list) {
			bool ignoreCase = connection.IsInCaseInsensitive;
			for (int i = 0; i < list.Count; ++i) {
				string colName = list[i];
				list[i] = ResolveColumnName(colName, ignoreCase);
			}
		}

		internal void AddColumn(DataColumnInfo column) {
			CheckMutable();
			column.TableInfo = this;
			columns.Add(column);
		}

		public DataColumnInfo AddColumn(string name, DataType type, bool notNull) {
			DataColumnInfo column = AddColumn(name, type);
			column.IsNotNull = notNull;
			return column;
		}

		public DataColumnInfo AddColumn(string name, DataType type) {
			CheckMutable();

			if (name == null)
				throw new ArgumentNullException("name");
			if (type == null)
				throw new ArgumentNullException("type");

			foreach (DataColumnInfo column in columns) {
				if (column.Name.Equals(name))
					throw new ArgumentException("Column '" + name + "' already exists in table '" + tableName + "'.");
			}

			DataColumnInfo newColumn = new DataColumnInfo(this, name, type);
			columns.Add(newColumn);
			return newColumn;
		}

		/// <summary>
		/// Gets the name of the schema the table belongs to if any,
		/// otherwise returns <see cref="String.Empty"/>.
		/// </summary>
		public string Schema {
			get { return tableName.Parent != null ? tableName.Parent.Name : ""; }
		}

		/// <summary>
		/// Gets the name of the table.
		/// </summary>
		public string Name {
			get { return tableName.Name; }
		}

		/// <summary>
		/// Gets the <see cref="TableName"/> object representing the full name 
		/// of the table.
		/// </summary>
		public ObjectName TableName {
			get { return tableName; }
		}

		public int ColumnCount {
			get { return columns.Count; }
		}

		public DataColumnInfo this[int column] {
			get { return columns[column]; }
		}

		public int FindColumnName(string columnName) {
			int size = ColumnCount;
			for (int i = 0; i < size; ++i) {
				if (this[i].Name.Equals(columnName)) {
					return i;
				}
			}
			return -1;
		}

		private Dictionary<string, int> colNameLookup;
		private readonly object colLookupLock = new Object();

		///<summary>
		/// A faster way to find a column index given a string column name.
		///</summary>
		///<param name="columnName"></param>
		/// <remarks>
		/// This caches column name -> column index input a hashtable.
		/// </remarks>
		///<returns></returns>
		public int FastFindColumnName(string columnName) {
			lock (colLookupLock) {
				if (colNameLookup == null)
					colNameLookup = new Dictionary<string, int>(30);

				int index;
				if (!colNameLookup.TryGetValue(columnName, out index)) {
					index = FindColumnName(columnName);
					colNameLookup[columnName] = index;
				}

				return index;
			}
		}


		/// <summary>
		/// Copies the object, excluding the columns and the constraints
		/// contained in it.
		/// </summary>
		/// <returns></returns>
		public DataTableInfo NoColumnClone() {
			DataTableInfo info = new DataTableInfo(tableName);
			info.tableTypeName = tableTypeName;
			return info;
		}



		object ICloneable.Clone() {
			return Clone();
		}

		public DataTableInfo Clone() {
			return Clone(tableName);
		}

		public DataTableInfo Clone(ObjectName newTableName) {
			DataTableInfo clone = new DataTableInfo(newTableName);
			clone.tableTypeName = (string)tableTypeName.Clone();
			clone.columns = new List<DataColumnInfo>();
			foreach (DataColumnInfo column in columns) {
				clone.columns.Add(column.Clone());
			}

			return clone;
		}
	}
}