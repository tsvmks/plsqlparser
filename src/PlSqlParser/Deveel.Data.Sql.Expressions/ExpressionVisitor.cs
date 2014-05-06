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
using System.Collections.ObjectModel;
using System.Linq;

namespace Deveel.Data.Sql.Expressions {
	public abstract class ExpressionVisitor : IExpressionVisitor {
		protected ExpressionVisitor() {
		}

		Expression IExpressionVisitor.Visit(Expression expression) {
			return Visit(expression);
		}

		protected virtual Expression Visit(Expression exp) {
			if (exp == null)
				return null;

			switch (exp.ExpressionType) {
				case ExpressionType.Negate:
				case ExpressionType.Not:
				case ExpressionType.Cast:
				case ExpressionType.TypeIs:
					return VisitUnary((UnaryExpression) exp);
				case ExpressionType.Add:
				case ExpressionType.Subtract:
				case ExpressionType.Multiply:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.And:
				case ExpressionType.Or:
				case ExpressionType.Smaller:
				case ExpressionType.SmallerOrEqual:
				case ExpressionType.Greater:
				case ExpressionType.GreaterOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Is:
				case ExpressionType.Like:
				case ExpressionType.Any:
				case ExpressionType.All:
					return VisitBinary((BinaryExpression) exp);
				case ExpressionType.Conditional:
					return VisitConditional((ConditionalExpression) exp);
				case ExpressionType.Constant:
					return VisitConstant((ConstantExpression) exp);
				case ExpressionType.Variable:
					return VisitVariable((VariableExpression) exp);
				case ExpressionType.CorrelatedVariable:
					return VisitCorrelatedVariable((CorrelatedVariableExpression) exp);
				case ExpressionType.Call:
					return VisitMethodCall((FunctionCallExpression) exp);
				case ExpressionType.Query:
					return VisitSubQuery((SubQueryExpression) exp);
				case ExpressionType.Subset:
					return VisitSubset((SubsetExpression) exp);
				default:
					throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.ExpressionType));
			}
		}

		protected virtual Expression VisitCorrelatedVariable(CorrelatedVariableExpression expression) {
			return expression;
		}

		protected virtual SubsetExpression VisitSubset(SubsetExpression expression) {
			var child = Visit(expression.Operand);
			if (child != expression.Operand)
				return new SubsetExpression(child);

			return expression;
		}

		protected virtual Expression VisitSubQuery(SubQueryExpression expression) {
			return expression;
		}

		protected virtual Expression VisitUnary(UnaryExpression expression) {
			Expression operand = Visit(expression.Operand);
			if (operand != expression.Operand)
				return Expression.Unary(expression.ExpressionType, operand);

			return expression;
		}

		protected virtual Expression VisitBinary(BinaryExpression expression) {
			Expression left = Visit(expression.First);
			Expression right = Visit(expression.Second);

			if (left != expression.First || right != expression.Second)
				return Expression.Binary(left, expression.ExpressionType, right);

			return expression;
		}

		protected virtual Expression VisitTypeIs(TypeIsExpression expression) {
			Expression expr = Visit(expression.Expression);
			if (expr != expression.Expression) {
				return Expression.Is(expr, expression.TypeOperand);
			}
			return expression;
		}

		protected virtual Expression VisitConstant(ConstantExpression expression) {
			return expression;
		}

		protected virtual Expression VisitConditional(ConditionalExpression expression) {
			Expression test = Visit(expression.Test);
			Expression ifTrue = Visit(expression.IfTrue);
			Expression ifFalse = Visit(expression.IfFalse);
			if (test != expression.Test || ifTrue != expression.IfTrue || ifFalse != expression.IfFalse) {
				return Expression.Conditional(test, ifTrue, ifFalse);
			}
			return expression;
		}

		protected virtual Expression VisitVariable(VariableExpression expression) {
			return expression;
		}

		protected virtual Expression VisitMethodCall(FunctionCallExpression expression) {
			Expression obj = Visit(expression.Object);

			IEnumerable<Expression> args = VisitExpressionList(expression.Arguments.ToList().AsReadOnly());
			if (obj != expression.Object || args != expression.Arguments) {
				return Expression.FunctionCall(obj, expression.FunctionName, args);
			}
			return expression;
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original) {
			List<Expression> list = null;
			for (int i = 0, n = original.Count; i < n; i++) {
				Expression p = this.Visit(original[i]);
				if (list != null) {
					list.Add(p);
				} else if (p != original[i]) {
					list = new List<Expression>(n);
					for (int j = 0; j < i; j++) {
						list.Add(original[j]);
					}
					list.Add(p);
				}
			}
			if (list != null) {
				return list.AsReadOnly();
			}
			return original;
		}
	}
}