using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Sql {
	public abstract class ParserTestBase {
		protected Expression ParseExpression(string s) {
			return SqlParser.SqlExpression(s);
		}

		protected IEnumerable<Statement> ParseStatements(string s) {
			return SqlParser.Statements(s);
		}

		protected Statement ParseStatement(string s) {
			return ParseStatements(s).FirstOrDefault();
		}
	}
}
