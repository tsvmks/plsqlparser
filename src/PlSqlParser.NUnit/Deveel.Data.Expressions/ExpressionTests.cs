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

		[Test]
		public void NumericNotEqual() {
			var exp = SqlParser.SqlExpression("NOT ((25 + 11) <> 58)");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<NotExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);

			var boolResult = result.ToBoolean();
			Assert.IsNotNull(boolResult);
			Assert.IsFalse(boolResult.Value);
		}

		[Test]
		public void StringLike() {
			var exp = SqlParser.SqlExpression("'Antonello' LIKE '*ntonello' ESCAPE '*'");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<LikeExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);

			var boolResult = result.ToBoolean();
			Assert.IsNotNull(boolResult);
			Assert.IsTrue(boolResult.Value);
		}
	}
}