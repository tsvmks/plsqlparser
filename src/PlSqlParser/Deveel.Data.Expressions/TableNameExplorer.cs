using System;
using System.Collections.Generic;

using Deveel.Data.Query;

namespace Deveel.Data.Expressions {
	static class TableNameExplorer {
		public static IList<string> DiscoverTableNams(Expression expression, IList<string> tableNames) {
			var visitor = new TableNameVisitor(tableNames);
			Expression.Visit(expression, visitor);
			return visitor.TableNames;
		}

		#region TableNameVisitor

		class TableNameVisitor : IExpressionVisitor {
			public IList<string> TableNames { get; private set; }

			public TableNameVisitor(IList<string> tableNames) {
				TableNames = tableNames;
			}

			public void Visit(Expression expression) {
				if (expression is ConstantExpression) {
					var constant = (ConstantExpression) expression;
					if (constant.Value is IQueryPlanNode) {
						TableNames = ((IQueryPlanNode) constant.Value).DiscoverTableNames(TableNames);
					}
				}
			}
		}

		#endregion
	}
}