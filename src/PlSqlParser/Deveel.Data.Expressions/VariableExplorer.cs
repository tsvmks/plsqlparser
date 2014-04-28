using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Deveel.Data.Expressions {
	static class VariableExplorer {
		public static IEnumerable<VariableBind> AllVariables(Expression expression) {
			var visitor = new VariableVisitor();
			visitor.VisitExpression(expression);
			return visitor.AllVariables.AsEnumerable();
		}

		#region VariableVisitor

		class VariableVisitor : ExpressionVisitor {
			public VariableVisitor() {
				AllVariables = new Collection<VariableBind>();
			}

			public ICollection<VariableBind> AllVariables { get; private set; }

			public void VisitExpression(Expression expression) {
				Visit(expression);
			}

			protected override Expression VisitVariable(VariableExpression p) {
				if (!AllVariables.Contains(p.VariableName))
					AllVariables.Add(p.VariableName);

				return base.VisitVariable(p);
			}
		}

		#endregion
	}
}