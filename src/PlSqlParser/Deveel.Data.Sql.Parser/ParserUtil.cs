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

using Deveel.Data.Expressions;
using Deveel.Data.Types;

namespace Deveel.Data.Sql.Parser {
	static class ParserUtil {
		public static string BindVariable(string name) {
			if (name[0] == ':')
				name = name.Substring(1);

			return name;
		}

		public static DataObject Number(string s) {
			Number number;

			try {
				number = Data.Number.Parse(s);
			} catch (FormatException) {
				throw new ParseException();
			}

			return new DataObject(PrimitiveTypes.Numeric(), number);
		}

		public static DataObject Unquote(string s) {
			if (System.String.IsNullOrEmpty(s))
				return null;

			if (s[0] == '\'')
				s = s.Substring(1);

			if (s[s.Length - 1] == '\'')
				s = s.Substring(0, s.Length - 1);

			return DataObject.String(s);
		}

		public static DataObject String(string s) {
			return DataObject.String(s);
		}

		public static DataObject Null() {
			return new DataObject(PrimitiveTypes.Null(), null);
		}

		public static FunctionArgument FunctionArgument(TableSelectExpression query) {
			return new FunctionArgument(null);
		}

		public static FunctionArgument FunctionArgument(Expression expression) {
			return new FunctionArgument(expression);
		}

		public static ObjectName ObjectName(string s) {
			return Data.ObjectName.Parse(s);
		}

		public static DataType PrimitiveType(SqlType sqlType, Token sizeToken, Token scaleToken) {
			int size = -1;
			byte scale = 0;
			if (sizeToken != null) {
				if (!Int32.TryParse(sizeToken.image, out size))
					throw new ParseException();
			}
			if (scaleToken != null) {
				if (!Byte.TryParse(scaleToken.image, out scale))
					throw new ParseException();
			}

			return PrimitiveTypes.Type(sqlType, size, scale);
		}

		public static DataType RefType(Token tokRef, ObjectName objRef, bool rowType, bool extRef) {
			if (extRef)
				throw new NotSupportedException();

			if (objRef == null && tokRef != null)
				objRef = ObjectName(tokRef.image);

			return rowType ? (DataType) PrimitiveTypes.RowType(objRef) : PrimitiveTypes.ColumnType(objRef);
		}
	}
}