using System;
using System.Linq;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Statements;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Deveel.Data.Sql {
	[TestFixture]
	public class SelectTests {
		[Test]
		public void SimpleTableSelect() {
			const string sql = "SELECT * FROM Person WHERE Name = 'Antonello';";
			var statement = SqlParser.Statements(sql).FirstOrDefault();

			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<SelectStatement>(statement);

			var selectStatement = (SelectStatement) statement;
			Assert.IsNotNull(selectStatement.SelectExpression);
			Assert.IsEmpty(selectStatement.OrderBy);

			var fromTables = selectStatement.SelectExpression.From.AllTables;
			Assert.AreEqual(1, fromTables.Count);
			Assert.AreEqual("Person", fromTables.First().Name);

			var whereExp = selectStatement.SelectExpression.Where;
			Assert.IsNotNull(whereExp);
			Assert.IsInstanceOf<EqualExpression>(whereExp);
			Assert.IsInstanceOf<VariableExpression>(((EqualExpression)whereExp).First);
			Assert.IsInstanceOf<ConstantExpression>(((EqualExpression)whereExp).Second);

			Assert.IsNotEmpty(whereExp.AllVariables());
			Assert.AreEqual(1, whereExp.AllVariables().Count());
			Assert.AreEqual("Name", whereExp.AllVariables().First());
		}
	}
}
