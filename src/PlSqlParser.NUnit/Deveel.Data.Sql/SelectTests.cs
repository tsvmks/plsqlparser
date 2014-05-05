using System;
using System.Linq;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Statements;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Deveel.Data.Sql {
	[TestFixture]
	public class SelectTests : ParserTestBase {
		[Test]
		public void SimpleTableSelect() {
			const string sql = "SELECT * FROM App.Person WHERE Name = 'Antonello';";
			var statement = ParseStatement(sql);

			Console.Out.WriteLine(statement.ToString());

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<SelectStatement>(statement);

			var selectStatement = (SelectStatement) statement;
			Assert.IsNotNull(selectStatement.SelectExpression);
			Assert.IsEmpty(selectStatement.OrderBy);

			var fromTables = selectStatement.SelectExpression.From.AllTables;
			Assert.AreEqual(1, fromTables.Count);
			Assert.AreEqual("App.Person", fromTables.First().Name.ToString());

			var whereExp = selectStatement.SelectExpression.Where;
			Assert.IsNotNull(whereExp);
			Assert.IsInstanceOf<FilterExpression>(whereExp);

			Assert.IsNotEmpty(whereExp.Expression.AllVariables());
			Assert.AreEqual(1, whereExp.Expression.AllVariables().Count());
			Assert.AreEqual("Name", whereExp.Expression.AllVariables().First().ToString());
		}

		[Test]
		public void SelectFromSubQuery() {
			const string sql = "SELECT Name FROM (SELECT * FROM App.Person WHERE Age < 32) ORDER BY Id DESC;";
			var statement = ParseStatement(sql);

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<SelectStatement>(statement);

			var selectStatement = (SelectStatement)statement;
			Assert.IsNotNull(selectStatement.SelectExpression);
			Assert.IsNotNull(selectStatement.OrderBy);
		}
	}
}
