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

namespace Deveel.Data.Sql.Expressions {
	public static class ExpressionPreparer {
		public static Expression Prepare(this Expression expression, IExpressionPreparer preparer) {
			var visitor = new PrepareVisitor(preparer);
			return visitor.PrepareExpression(expression);
		}

		class PrepareVisitor : ExpressionVisitor {
			private readonly IExpressionPreparer preparer;

			public PrepareVisitor(IExpressionPreparer preparer) {
				this.preparer = preparer;
			}

			private Expression DoPrepare(Expression expression) {
				if (preparer.CanPrepare(expression))
					expression = preparer.Prepare(expression);

				return expression;
			}

			protected override Expression VisitVariable(VariableExpression expression) {
				return DoPrepare(expression);
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				return DoPrepare(expression);
			}

			protected override Expression VisitSubQuery(SubQueryExpression expression) {
				return DoPrepare(expression);
			}

			public Expression PrepareExpression(Expression expression) {
				return Visit(expression);
			}
		}
	}
}