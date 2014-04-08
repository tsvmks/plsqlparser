using System;

namespace Deveel.Data.Expressions {
	public sealed class TypeIsExpression : Expression {
		public TypeIsExpression(Expression expression, Type typeOperand) {
			TypeOperand = typeOperand;
			Expression = expression;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Is; }
		}

		public Expression Expression { get; private set; }

		public Type TypeOperand { get; private set; }
	}
}