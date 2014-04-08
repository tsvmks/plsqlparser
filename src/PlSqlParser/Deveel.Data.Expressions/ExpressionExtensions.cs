using System;
using System.Collections.Generic;

namespace Deveel.Data.Expressions {
	public static class ExpressionExtensions {
		public static IEnumerable<string> AllVariables(this Expression expression) {
			return VariableExplorer.AllVariables(expression);
		}
	}
}