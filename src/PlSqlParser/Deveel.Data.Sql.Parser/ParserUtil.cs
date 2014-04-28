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

			return new DataObject(PrimitiveTypes.String(), s);
		}

		public static DataObject String(string s) {
			return new DataObject(PrimitiveTypes.String(), s);
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