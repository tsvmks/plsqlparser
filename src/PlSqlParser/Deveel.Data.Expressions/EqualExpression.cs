using System;

namespace Deveel.Data.Expressions {
	public sealed class EqualExpression : BinaryExpression {
		public EqualExpression(Expression first, Expression second) 
			: base(first, second) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Equals; }
		}

		protected override object Evaluate(EvaluationContext context) {
			throw new NotImplementedException();
		}
	}
}