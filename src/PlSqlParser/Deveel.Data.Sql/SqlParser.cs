using System;
using System.Collections.Generic;
using System.IO;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Parser;
using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Sql {
	public static class SqlParser {
		public static Expression PlSqlExpression(string s) {
			using (var stringReader = new StringReader(s)) {
				var parser = new PlSql(stringReader);
				return parser.PlSqlExpression();
			}
		}

		public static Expression SqlExpression(string s) {
			using (var stringReader = new StringReader(s)) {
				var parser = new PlSql(stringReader);
				return parser.SQLExpression();
			}
		}

		public static IEnumerable<Statement> Statements(string sql) {
			using (var stringReader = new StringReader(sql)) {
				var parser = new PlSql(stringReader);
				return parser.SequenceOfStatements();
			}
		}
	}
}