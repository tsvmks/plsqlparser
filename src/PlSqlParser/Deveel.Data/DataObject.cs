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
using System.Text;

using Deveel.Data.Types;

namespace Deveel.Data {
	[Serializable]
	public sealed class DataObject {
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

		public bool? ToBoolean() {
			throw new NotImplementedException();
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

		private Number ToNumber() {
			if (!(DataType is NumericType))
				return null;

			return (Number) Value;
		}

		public static DataObject Boolean(bool value) {
			return new DataObject(PrimitiveTypes.Boolean(), value);
		}

		public override string ToString() {
			return IsNull ? "NULL" : Value.ToString();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public DataObject Not() {
			throw new NotImplementedException();
		}

		public DataObject LessEquals(DataObject value) {
			throw new NotImplementedException();
		}

		public DataObject Less(DataObject value) {
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public DataObject CastTo(DataType dataType) {
			throw new NotImplementedException();
		}

		public DataObject Subtract(DataObject value) {
			throw new NotImplementedException();
		}

		public string ToStringValue() {
			return Value.ToString();
		}
	}
}