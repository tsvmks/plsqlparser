using System;

using Deveel.Data.Query;
using Deveel.Data.Sql;

using Microsoft.SqlServer.Server;

using NUnit.Framework;

namespace Deveel.Data.Sql.Expressions {
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

		[Test]
		public void AnyNoEqual() {
			var exp = SqlParser.SqlExpression("3 <> ANY (58, 1.2)");
			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<AnyExpression>(exp);

			var result = exp.Evaluate();
			Assert.IsNotNull(result);
			Assert.IsTrue(result.DataType.IsPrimitive);
			Assert.AreEqual(DataObject.BooleanTrue, result);
		}

		[Test]
		public void AndOperatorBreak() {
			var exp = SqlParser.SqlExpression("(32 + 23 > 12) AND 45 > 12 AND 11 != 12");
			Assert.IsNotNull(exp);

			Console.Out.WriteLine(exp.ToString());

			var exps = exp.BreakByOperator(Operator.And);

			int i = 0;
			foreach (var expression in exps) {
				Console.Out.Write("[{0}] = ", ++i);
				Console.Out.WriteLine(expression);
			}

			Assert.AreEqual(4, exps.Count);
		}

		[Test]
		public void SimpleConditional() {
			var exp = SqlParser.SqlExpression("CASE 3 WHEN (1 + 2 = 3) THEN 1 END");

			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<ConditionalExpression>(exp);

			var condExp = (ConditionalExpression) exp;
			Assert.IsInstanceOf<ConstantExpression>(condExp.IfTrue);
			Assert.IsInstanceOf<SubsetExpression>(condExp.Test);
			Assert.IsInstanceOf<ConstantExpression>(condExp.IfFalse);

			var eval = condExp.Evaluate();
			Assert.AreEqual(3, eval.ToNumber().ToInt32());
		}

		[Test]
		public void ComplexConditional() {
			var exp = SqlParser.SqlExpression("CASE 3 WHEN (1 + 2 = 3) THEN 'fail' WHEN (22 = 1) THEN 1 END");

			Assert.IsNotNull(exp);
			Assert.IsInstanceOf<ConditionalExpression>(exp);

			var condExp = (ConditionalExpression)exp;

			var eval = condExp.Evaluate();
			Assert.AreEqual(3, eval.ToNumber().ToInt32());
		}
	}
}