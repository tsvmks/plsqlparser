using System;

namespace Deveel.Data.Expressions {
	public sealed class ConditionalExpression : Expression {
		public ConditionalExpression(Expression test, Expression ifTrue) 
			: this(test, ifTrue, null) {
		}

		public ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse) {
			IfFalse = ifFalse;
			IfTrue = ifTrue;
			Test = test;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Conditional; }
		}

		public Expression Test { get; private set; }

		public Expression IfTrue { get; private set; }

		public Expression IfFalse { get; set; }
	}
}