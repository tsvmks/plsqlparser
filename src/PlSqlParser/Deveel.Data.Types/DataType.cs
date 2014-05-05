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
using System.Diagnostics;
using System.IO;

using Deveel.Data.Sql.Parser;

namespace Deveel.Data.Types {
	[Serializable]
	[DebuggerDisplay("{ToString(), nq}")]
	public abstract class DataType : IComparer<DataObject> {
		private static readonly PlSql Parser;

		protected DataType(string name, SqlType sqlType) {
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			Name = name;
			SqlType = sqlType;
		}

		protected DataType(SqlType sqlType)
			: this(sqlType.ToString(), sqlType) {
		}

		static DataType() {
			Parser = new PlSql(new StringReader(""));
		}

		public string Name { get; private set; }

		public SqlType SqlType { get; private set; }

		public bool IsPrimitive {
			get { return PrimitiveTypes.IsPrimitive(SqlType); }
		}

		public virtual bool IsIndexable {
			get { return true; }
		}

		public virtual bool IsComparable(DataType type) {
			return SqlType.Equals(type.SqlType);
		}

		public int Compare(DataObject x, DataObject y) {
			// TODO: verify if they are comparable first
			return CompareValues(x.Value, y.Value);
		}

		protected virtual int CompareValues(object x, object y) {
			throw new NotSupportedException();
		}

		public override string ToString() {
			return Name;
		}

		public override bool Equals(object obj) {
			var dataType = obj as DataType;
			if (dataType == null)
				return false;

			return Name.Equals(dataType.Name);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}

		public static DataType Parse(string s) {
			if (String.IsNullOrEmpty(s))
				throw new ArgumentNullException("s");

			lock (Parser) {
				Parser.ReInit(new StringReader(s));
				return Parser.TypeDefinition();
			}
		}

		public virtual DataType Wider(DataType otherType) {
			return this;
		}

		public DataObject CastValueTo(DataObject value, DataType destType) {
			if (value.IsNull)
				return new DataObject(destType, null);

			// If the two types equal, no need to cast
			if (value.DataType.Equals(destType))
				return value;

			object result;

			try {
				result = CastObjectTo(value.Value, destType);
			} catch (Exception e) {
				throw new InvalidCastException(String.Format("Cannot cast an object from Type {0} to Type {1}", ToString(), destType), e);
			}
			
			return new DataObject(destType, result);
		}

		protected virtual object CastObjectTo(object value, DataType destType) {
			throw new InvalidCastException();
		}

		internal virtual string ValueToString(object obj) {
			return obj.ToString();
		}
	}
}