using System;

using Deveel.Data.Sql;

using NUnit.Framework;

namespace Deveel.Data.Expressions {
	[TestFixture]
	public class ExpressionTests {
		[Test]
		public void NumericAdd() {
			var exp = SqlParser.SqlExpression("33 + 46.95");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<AddExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);
			Assert.AreEqual("79.95", result.ToString());
		}

		[Test]
		public void NumericAddWithPrecedence() {
			var exp = SqlParser.SqlExpression("33 + 46.95 + (162 + 7)");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<AddExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);
			Assert.AreEqual("248.95", result.ToString());
		}

		[Test]
		public void AddAndMultiply() {
			var exp = SqlParser.SqlExpression("54.08 + 26.15 + (82 * 3)");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<AddExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);
			Assert.AreEqual("326.23", result.ToString());
		}
	}
}