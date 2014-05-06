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
using System.IO;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// Used to managed all the informations about a column in a table
	/// (<see cref="DataTableInfo"/>).
	/// </summary>
	[Serializable]
	public sealed class DataColumnInfo : ICloneable {
		private DataTableInfo tableInfo;

		/// <summary>
		/// A flag indicating if the column must allow only not-null values.
		/// </summary>
		private bool notNull;

		/// <summary>
		/// If this is an object column, this is a constraint that the object
		/// must be derived from to be added to this column.  If not specified,
		/// it defaults to <see cref="object"/>.
		/// </summary>
		private String baseTypeConstraint = "";

		/// <summary>
		/// The default expression string.
		/// </summary>
		private string defaultExpressionString;

		/// <summary>
		/// The type of index to use on this column.
		/// </summary>
		private string indexType = "";

		/// <summary>
		/// The name of the column.
		/// </summary>
		private string name;

		/// <summary>
		/// The TType object for this column.
		/// </summary>
		private readonly DataType type;

		internal DataColumnInfo(DataTableInfo tableInfo, string name, DataType type) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (type == null) 
				throw new ArgumentNullException("type");

			this.tableInfo = tableInfo;
			this.name = name;
			this.type = type;
		}

		public DataTableInfo TableInfo {
			get { return tableInfo; }
			internal set { tableInfo = value; }
		}

		public string Name {
			get { return name; }
			internal set { name = value; }
		}


		public bool IsNotNull {
			get { return notNull; }
			set { notNull = value; }
		}


		public bool IsIndexableType {
			get { return !(type is BinaryType); }
		}

		public DataType DataType {
			get { return type; }
		}

		/// <summary>
		/// Dumps information about this object to the <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="output"></param>
		public void Dump(TextWriter output) {
			output.Write(Name);
			output.Write(" ");
			output.Write(type.ToString());
		}


		object ICloneable.Clone() {
			return Clone();
		}

		public DataColumnInfo Clone() {
			DataColumnInfo clone = new DataColumnInfo(tableInfo, (string)name.Clone(), type);
			clone.notNull = notNull;
			if (!String.IsNullOrEmpty(defaultExpressionString)) {
				clone.defaultExpressionString = (string) defaultExpressionString.Clone();
			}
			clone.indexType = (string)indexType.Clone();
			clone.baseTypeConstraint = (string)baseTypeConstraint.Clone();
			return clone;
		}
	}
}