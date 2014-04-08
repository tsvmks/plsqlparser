using System;

namespace Deveel.Data.Expressions {
	public sealed class ConstantExpression : Expression {
		public object Value { get; set; }

		public ConstantExpression(object value) {
			Value = value;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Constant; }
		}
	}
}