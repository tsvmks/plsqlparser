using System;

namespace Deveel.Data.Expressions {
	public sealed class VariableExpression : Expression {
		public VariableExpression(string variableName) {
			VariableName = variableName;
		}

		public string VariableName { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Variable; }
		}
	}
}