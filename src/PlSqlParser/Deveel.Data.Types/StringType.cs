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
using System.Globalization;
using System.Text;

namespace Deveel.Data.Types {
	[Serializable]
	public sealed class StringType : DataType {
		public StringType(SqlType sqlType) 
			: this(sqlType, -1) {
		}

		public StringType(SqlType sqlType, int maxSize) 
			: base("STRING", sqlType) {
			AssertIsString(sqlType);
			MaxSize = maxSize;
		}

		private static void AssertIsString(SqlType sqlType) {
			if (sqlType != SqlType.String &&
				sqlType != SqlType.VarChar &&
				sqlType != SqlType.Char &&
				sqlType != SqlType.LongVarChar &&
				sqlType != SqlType.Clob)
				throw new ArgumentException(String.Format("The type {0} is not a valid STRING type.", sqlType), "sqlType");
		}

		public int MaxSize { get; private set; }

		public CultureInfo Locale { get; private set; }

		public override string ToString() {
			var sb = new StringBuilder(Name);
			if (MaxSize >= 0)
				sb.AppendFormat("({0})", MaxSize);

			return sb.ToString();
		}

		private static Number ToBigNumber(String str) {
			try {
				return Number.Parse(str);
			} catch (Exception) {
				return Number.Zero;
			}
		}


		/// <summary>
		/// Parses a String as an SQL date.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime ToDate(string str) {
			DateTime result;
			if (!DateTime.TryParseExact(str, DateType.DateFormatSql, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				throw new InvalidCastException(DateErrorMessage(str, SqlType.Date, DateType.DateFormatSql));

			return result;
		}

		/// <summary>
		/// Parses a String as an SQL time.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime ToTime(String str) {
			DateTime result;
			if (!DateTime.TryParseExact(str, DateType.TimeFormatSql, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out result))
				throw new InvalidCastException(DateErrorMessage(str, SqlType.Time, DateType.TimeFormatSql));

			return result;

		}

		/// <summary>
		/// Parses a String as an SQL timestamp.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static DateTime ToTimeStamp(String str) {
			DateTime result;
			if (!DateTime.TryParseExact(str, DateType.TsFormatSql, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				throw new InvalidCastException(DateErrorMessage(str, SqlType.TimeStamp, DateType.TsFormatSql));

			return result;
		}

		private static string DateErrorMessage(string str, SqlType sqlType, string[] formats) {
			return String.Format("The input string {0} is not compatible with any of the formats for SQL Type {1} ( {2} )",
				str,
				sqlType.ToString().ToUpperInvariant(),
				String.Join(", ", formats));
		}


		protected override object CastObjectTo(object value, DataType destType) {
			string str = value.ToString();
			var sqlType = destType.SqlType;

			switch (sqlType) {
				case (SqlType.Bit):
				case (SqlType.Boolean):
					return (String.Compare(str, "true", StringComparison.OrdinalIgnoreCase) == 0 ||
							String.Compare(str, "1", StringComparison.OrdinalIgnoreCase) == 0);
				case (SqlType.TinyInt):
				// fall through
				case (SqlType.SmallInt):
				// fall through
				case (SqlType.Integer):
					return (Number)ToBigNumber(str).ToInt32();
				case (SqlType.BigInt):
					return (Number)ToBigNumber(str).ToInt64();
				case (SqlType.Float):
					return Number.Parse(Convert.ToString(ToBigNumber(str).ToDouble()));
				case (SqlType.Real):
					return ToBigNumber(str);
				case (SqlType.Double):
					return Number.Parse(Convert.ToString(ToBigNumber(str).ToDouble()));
				case (SqlType.Numeric):
				// fall through
				case (SqlType.Decimal):
					return ToBigNumber(str);
				case (SqlType.Char):
					return new StringObject(CastUtil.PaddedString(str, ((StringType)destType).MaxSize));
				case (SqlType.VarChar):
				case (SqlType.LongVarChar):
				case (SqlType.String):
					return new StringObject(str);
				case (SqlType.Date):
					return ToDate(str);
				case (SqlType.Time):
					return ToTime(str);
				case (SqlType.TimeStamp):
					return ToTimeStamp(str);
				case (SqlType.Blob):
				// fall through
				case (SqlType.Binary):
				// fall through
				case (SqlType.VarBinary):
				// fall through
				case (SqlType.LongVarBinary):
					return new BinaryObject(Encoding.Unicode.GetBytes(str));
				case (SqlType.Null):
					return null;
				case (SqlType.Clob):
					return new StringObject(str);
				default:
					throw new InvalidCastException();
			}
		}
	}
}