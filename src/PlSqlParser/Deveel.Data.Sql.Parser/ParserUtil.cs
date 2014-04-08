using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql.Parser {
	static class ParserUtil {
		public static double Number(string s) {
			double value;
			if (!Double.TryParse(s, out value))
				return value;

			throw new FormatException();
		}

		public static string Unquote(string s) {
			if (String.IsNullOrEmpty(s))
				return null;

			if (s[0] == '\'')
				s = s.Substring(1);

			if (s[s.Length - 1] == '\'')
				s = s.Substring(0, s.Length - 1);

			return s;
		}

		public static FunctionArgument FunctionArgument(TableSelectExpression query) {
			return new FunctionArgument(null);
		}

		public static FunctionArgument FunctionArgument(Expression expression) {
			return new FunctionArgument(expression);
		}

		public static Expression Binary(Expression first, Operator op, Expression second) {
			if (op == Operator.Equals)
				return Expression.Equal(first, second);

			throw new ParseException();
		}
	}
}