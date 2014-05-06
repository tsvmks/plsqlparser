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
using System.Text;

using Deveel.Data.Expressions;
using Deveel.Data.Types;

namespace Deveel.Data {
	[Serializable]
	public sealed class DataObject : IComparable<DataObject>, IComparable {
		public DataObject(DataType dataType, object value) {
			if (dataType == null)
				throw new ArgumentNullException("dataType");

			Value = value;
			DataType = dataType;
		}

		public DataType DataType { get; private set; }

		public object Value { get; private set; }

		public bool IsNull {
			get { return Value == null; }
		}

		public static readonly DataObject BooleanFalse = new DataObject(PrimitiveTypes.Boolean(), false);
		public static readonly  DataObject BooleanTrue = new DataObject(PrimitiveTypes.Boolean(), true);
		public static readonly DataObject BooleanNull = new DataObject(PrimitiveTypes.Boolean(), null);
		public static readonly DataObject Null = new DataObject(PrimitiveTypes.Null(), null);

		public static DataObject Array(IEnumerable<Expression> expressions) {
			return new DataObject(new ArrayType(), expressions);
		}

		public bool ValuesEqual(DataObject obj) {
			if (this == obj) {
				return true;
			}
			if (DataType.IsComparable(obj.DataType)) {
				return CompareTo(obj) == 0;
			}
			return false;
		}

		public bool IsComparableTo(DataObject obj) {
			return DataType.IsComparable(obj.DataType);
		}

		int IComparable.CompareTo(object obj) {
			if (!(obj is DataObject))
				throw new ArgumentException();

			return CompareTo((DataObject) obj);
		}

		public int CompareTo(DataObject tob) {
			// If this is null
			if (IsNull) {
				// and value is null return 0 return less
				if (tob.IsNull)
					return 0;
				return -1;
			}

			// If this is not null and value is null return +1
			if (tob.IsNull)
				return 1;

			// otherwise both are non null so compare normally.
			return CompareToNoNulls(tob);
		}

		public int CompareToNoNulls(DataObject tob) {
			DataType ttype = DataType;
			// Strings must be handled as a special case.
			if (ttype is StringType) {
				// We must determine the locale to compare against and use that.
				var stype = (StringType)ttype;

				// If there is no locale defined for this type we use the locale in the
				// given type.
				if (stype.Locale == null) {
					ttype = tob.DataType;
				}
			}

			return ttype.Compare(this, tob);
		}

		public bool? ToBoolean() {
			if (DataType is BooleanType)
				return (bool?)Value;
			return null;
		}

		public DataObject Add(DataObject value) {
			if (DataType is NumericType) {
				Number v1 = ToNumber();
				Number v2 = value.ToNumber();
				DataType resultType = DataType.Wider(value.DataType);

				if (v1 == null || v2 == null) {
					return new DataObject(resultType, null);
				}

				return new DataObject(resultType, v1.Add(v2));
			} else if (DataType is StringType) {
				if (!(value.DataType is StringType))
					value = value.CastTo(PrimitiveTypes.String());

				return Concat(value);
			}

			throw new InvalidOperationException();
		}

		public Number ToNumber() {
			if (!(DataType is NumericType))
				return null;

			return (Number) Value;
		}

		public static DataObject Boolean(bool value) {
			return new DataObject(PrimitiveTypes.Boolean(), value);
		}

		public override string ToString() {
			return IsNull ? "NULL" : DataType.ValueToString(Value);
		}

		public DataObject Concat(DataObject value) {
			// If this or val is null then return the null value
			if (IsNull)
				return this;
			if (value.IsNull)
				return value;

			DataType tt1 = DataType;
			DataType tt2 = value.DataType;

			if (tt1 is StringType &&
				tt2 is StringType) {
				// Pick the first locale,
				var st1 = (StringType)tt1;
				var st2 = (StringType)tt2;

				var destType = st1.Wider(st2);

				var obj1 = (IStringObject) Value;
				var obj2 = (IStringObject) value.Value;

				return new DataObject(destType, obj1.Concat(obj2));
			}

			// Return null if LHS or RHS are not strings
			return new DataObject(tt1, null);
		}

		public DataObject Divide(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject Modulus(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject IsEqual(DataObject value) {
			// Check the types are comparable
			if (IsComparableTo(value) && !IsNull && !value.IsNull) {
				return Boolean(CompareToNoNulls(value) == 0);
			}
			// Not comparable types so return null
			return BooleanNull;
		}

		public DataObject SoundsLike(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject GreaterEquals(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject Greater(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject Is(DataObject value) {
			if (IsNull && value.IsNull)
				return BooleanTrue;
			if (IsComparableTo(value))
				return Boolean(CompareTo(value) == 0);

			// Not comparable types so return false
			return BooleanFalse;
		}

		public DataObject Not() {
			// If type is null
			if (IsNull)
				return this;

			bool? b = ToBoolean();
			return b.HasValue ? Boolean(!b.Value) : BooleanNull;
		}

		public DataObject LessEquals(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject Less(DataObject value) {
			// Check the types are comparable
			if (IsComparableTo(value) && !IsNull && !value.IsNull)
				return Boolean(CompareToNoNulls(value) < 0);

			// Not comparable types so return null
			return BooleanNull;
		}

		public DataObject Multiply(DataObject value) {
			Number v1 = ToNumber();
			Number v2 = value.ToNumber();
			DataType resultType = DataType.Wider(value.DataType);

			if (v1 == null || v2 == null) {
				return new DataObject(resultType, null);
			}

			return new DataObject(resultType, v1.Multiply(v2));
		}

		public DataObject IsNotEqual(DataObject value) {
			// Check the types are comparable
			if (IsComparableTo(value) && !IsNull && !value.IsNull) {
				return Boolean(CompareToNoNulls(value) != 0);
			}
			// Not comparable types so return null
			return BooleanNull;
		}

		public DataObject CastTo(DataType dataType) {
			return DataType.CastValueTo(this, dataType);
		}

		public DataObject Subtract(DataObject value) {
			throw new NotImplementedException();
		}

		public string ToStringValue() {
			return Value.ToString();
		}

		public static DataObject From(object obj) {
			if (obj is string)
				return String((string) obj);
			if (obj is bool)
				return Boolean((bool) obj);

			throw new NotSupportedException();
		}

		public static DataObject String(string s) {
			return String(new StringObject(s));
		}

		public static DataObject String(IStringObject s) {
			return new DataObject(PrimitiveTypes.String(), s);
		}

		public static DataObject Number(int value) {
			return new DataObject(PrimitiveTypes.Numeric(SqlType.Integer), (Number)value);
		}

		public static DataObject Date(DateTime value) {
			return new DataObject(PrimitiveTypes.Date(), value);
		}

		public static DataObject Now(bool utc) {
			var date = utc ? DateTime.UtcNow : DateTime.Now;
			return Date(date);
		}

		public static DataObject Now() {
			return Now(true);
		}
	}
}