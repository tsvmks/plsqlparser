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

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	public static class ExpressionReturnType {
		public static DataType ReturnType(this Expression expression, IVariableResolver resolver, IQueryContext context) {
			var visitor = new Visitor(resolver, context);
			return visitor.FindReturnType(expression);
		}

		#region Visitor

		class Visitor : ExpressionVisitor {
			private readonly IQueryContext context;
			private readonly IVariableResolver resolver;
			private DataType returnType;

			public DataType FindReturnType(Expression expression) {
				Visit(expression);
				return returnType;
			}

			public Visitor(IVariableResolver resolver, IQueryContext context) {
				this.resolver = resolver;
				this.context = context;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				returnType = expression.Value.DataType;
				return expression;
			}

			protected override Expression VisitMethodCall(FunctionCallExpression expression) {
				// TODO:
				return base.VisitMethodCall(expression);
			}

			protected override Expression VisitCorrelatedVariable(CorrelatedVariableExpression expression) {
				returnType = expression.CorrelatedVariable.ReturnType;
				return expression;
			}

			protected override Expression VisitVariable(VariableExpression expression) {
				returnType = resolver.ReturnType(expression.VariableName);
				return expression;
			}

			protected override Expression VisitBinary(BinaryExpression expression) {
				returnType = expression.Operator.ReturnType();
				return expression;
			}
		}

		#endregion
	}
}