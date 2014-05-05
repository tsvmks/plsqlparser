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
using System.IO;
using System.Text;

using Deveel.Data.DbSystem;
using Deveel.Data.Expressions;
using Deveel.Data.Sql.Parser;
using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Sql {
	public static class SqlExecutor {
		private static readonly PlSql SqlParser;

		static SqlExecutor() {
			SqlParser = new PlSql(new StringReader(""));
		}

		public static ITable[] Execute(IDatabaseConnection connection, SqlQuery query) {
			// StatementTree caching

			// Substitute all parameter substitutions in the statement tree.
			// TODO: IExpressionPreparer preparer = new QueryPreparer(query);

			// Create a new parser and set the parameters...
			IEnumerable<Statement> statements;

			string commandText = query.Text;

			try {
				lock (SqlParser) {
					SqlParser.ReInit(new StreamReader(new MemoryStream(Encoding.Unicode.GetBytes(commandText)), Encoding.Unicode));
					SqlParser.Reset();
					// Parse the statement.
					statements = SqlParser.SequenceOfStatements();
				}
			} catch (ParseException e) {
				var tokens = SqlParser.token_source.tokenHistory;
				throw new SqlParseException(e, commandText, tokens);
			}


			List<ITable> results = new List<ITable>();
			foreach (Statement parsedStatement in statements) {
				var statement = parsedStatement;
				// TODO: statement = statement.PrepareExpressions(preparer);

				// Convert the StatementTree to a statement object
				statement.Query = query;

				DatabaseQueryContext context = new DatabaseQueryContext(connection);

				// Prepare the statement
				statement = statement.PrepareStatement(context);

				// Evaluate the SQL statement.
				results.Add(statement.Evaluate(context));
			}

			return results.ToArray();
		}

	}
}