using System;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Statements;

using NUnit.Framework;

namespace Deveel.Data.Sql {
	[TestFixture]
	public class AssignmentTests : ParserTestBase {
		[Test]
		public void AssignVariable() {
			var statement = ParseStatement(":var = 22");
			Assert.IsNotNull(statement);
			Assert.IsInstanceOf<AssignmentStatement>(statement);

			var assignStatement = (AssignmentStatement) statement;

			Assert.IsNotNull(assignStatement.Member);
			Assert.IsInstanceOf<VariableExpression>(assignStatement.Member);

			var varExp = (VariableExpression) assignStatement.Member;
			Assert.AreEqual("var", varExp.VariableName.ToString());

			Assert.IsNotNull(assignStatement.AssignExpression);
			Assert.IsInstanceOf<ConstantExpression>(assignStatement.AssignExpression);

			var assignExp = (ConstantExpression) assignStatement.AssignExpression;
			Assert.AreEqual(22, assignExp.Value);
		}
	}
}