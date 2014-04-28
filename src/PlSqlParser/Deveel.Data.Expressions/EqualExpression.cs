using System;

namespace Deveel.Data.Expressions {
	public sealed class EqualExpression : BinaryExpression {
		public EqualExpression(Expression first, Expression second) 
			: base(first, second) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Equal; }
		}

		internal override Operator Operator {
			get { return Operator.Equal; }
		}
	}
}