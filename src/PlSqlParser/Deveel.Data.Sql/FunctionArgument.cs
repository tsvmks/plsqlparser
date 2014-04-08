using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public class FunctionArgument {
		public string VariableName { get; private set; }

		public Expression Expression { get; private set; }

		public FunctionArgument(Expression expression) 
			: this(null, expression) {
		}

		public FunctionArgument(string variableName, Expression expression) {
			VariableName = variableName;
			Expression = expression;
		}
	}
}