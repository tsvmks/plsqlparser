using System;

namespace Deveel.Data.Expressions {
	public abstract class UnaryExpression : Expression {
		protected UnaryExpression(Expression operand) {
			Operand = operand;
		}

		public Expression Operand { get; private set; }
	}
}