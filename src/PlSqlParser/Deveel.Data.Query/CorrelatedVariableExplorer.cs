// 
//  Copyright 2014  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Query {
	static class CorrelatedVariableExplorer {
		public static IList<CorrelatedVariable> DiscoverCorrelatedVariables(this Expression expression, ref int level, IList<CorrelatedVariable> list) {
			var visitor = new Visitor();
			return visitor.Discover(expression, ref level, list);
		}

		#region Visitor

		class Visitor : ExpressionVisitor {
			private IList<CorrelatedVariable> variables = new List<CorrelatedVariable>();
			private int queryLevel;

			public IList<CorrelatedVariable> Discover(Expression expression, ref int level, IList<CorrelatedVariable> list) {
				variables = list;
				queryLevel = level;

				Visit(expression);
				return variables;
			}

			protected override Expression VisitCorrelatedVariable(CorrelatedVariableExpression expression) {
				if (expression.CorrelatedVariable.Level == queryLevel)
					variables.Add(expression.CorrelatedVariable);

				return expression;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				var value = expression.Value;
				if (value.DataType is QueryType) {
					var planNode = (IQueryPlanNode) value.Value;
					variables = planNode.DiscoverCorrelatedVariables(queryLevel + 1, variables);
				}

				return expression;
			}
		}

		#endregion
	}
}