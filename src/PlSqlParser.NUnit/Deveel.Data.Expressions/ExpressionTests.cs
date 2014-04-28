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
	}
}