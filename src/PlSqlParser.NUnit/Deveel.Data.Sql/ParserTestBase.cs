using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public abstract class ParserTestBase {
		protected Expression ParseExpression(string s) {
			return SqlParser.SqlExpression(s);
		}
	}
}
