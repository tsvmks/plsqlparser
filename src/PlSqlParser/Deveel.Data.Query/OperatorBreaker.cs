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

using System.Collections.Generic;

using Deveel.Data.Expressions;

namespace Deveel.Data.Query {
	public static class OperatorBreaker {
		public static IList<Expression> BreakByOperator(this Expression expression, Operator op) {
			return BreakByOperator(expression, new List<Expression>(), op);
		}

		public static IList<Expression> BreakByOperator(this Expression expression, IList<Expression> list, Operator op) {
			var visitor = new Visitor(list, op);
			return visitor.Break(expression);
		}

		class Visitor : ExpressionVisitor {
			private IList<Expression> list;
			private readonly Operator op;

			public IList<Expression> Break(Expression expression) {
				Visit(expression);
				return list;
			}

			public Visitor(IList<Expression> list, Operator op) {
				this.op = op;
				this.list = list;
			}

			protected override Expression VisitBinary(BinaryExpression expression) {
				if (expression.Operator == op) {
					list = expression.First.BreakByOperator(list, op);
					list = expression.Second.BreakByOperator(list, op);
				} else {
					list.Add(expression);
				}

				return expression;
			}
		}
	}
}