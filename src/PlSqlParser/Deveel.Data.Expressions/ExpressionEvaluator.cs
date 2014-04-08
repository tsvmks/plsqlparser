using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public static class ExpressionEvaluator {
		public static object Evaluate(Expression expression, IQueryContext context) {
			var visitor = new EvaluateVisitor(context);
			Expression.Visit(expression, visitor);
			return visitor.Result;
		}

		#region EvaluateVisitor

		class EvaluateVisitor : IExpressionVisitor {
			public EvaluateVisitor(IQueryContext context) {
				Context = new EvaluationContext(context);
			}

			public EvaluationContext Context { get; private set; }

			public object Result { get; private set; }

			public void Visit(Expression expression) {
				if (expression is BinaryExpression) {
					var binary = (BinaryExpression) expression;
				} else {
					
				}
			}
		}

		#endregion
	}
}