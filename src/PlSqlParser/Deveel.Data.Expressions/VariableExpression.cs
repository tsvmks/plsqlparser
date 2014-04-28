using System;

namespace Deveel.Data.Expressions {
	public sealed class VariableExpression : Expression {
		public VariableExpression(VariableBind variableName) {
			VariableName = variableName;
		}

		public VariableBind VariableName { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Variable; }
		}
	}
}