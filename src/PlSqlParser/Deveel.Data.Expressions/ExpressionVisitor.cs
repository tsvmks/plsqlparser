using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Deveel.Data.Expressions {
	public abstract class ExpressionVisitor {
		protected ExpressionVisitor() {
		}

		protected virtual Expression Visit(Expression exp) {
			if (exp == null)
				return null;

			switch (exp.ExpressionType) {
				case ExpressionType.Negate:
				case ExpressionType.Not:
				case ExpressionType.Cast:
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
				case ExpressionType.Equals:
				case ExpressionType.NotEquals:
					return VisitBinary((BinaryExpression) exp);
				case ExpressionType.Is:
					return this.VisitTypeIs((TypeIsExpression) exp);
				case ExpressionType.Conditional:
					return VisitConditional((ConditionalExpression) exp);
				case ExpressionType.Constant:
					return this.VisitConstant((ConstantExpression) exp);
				case ExpressionType.Variable:
					return this.VisitVariable((VariableExpression) exp);
				case ExpressionType.Call:
					return this.VisitMethodCall((FunctionCallExpression) exp);
				case ExpressionType.Query:
					return VisitSubQuery((SubQueryExpression) exp);
				default:
					throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.ExpressionType));
			}
		}

		private Expression VisitSubQuery(SubQueryExpression expression) {
			throw new NotImplementedException();
		}

		protected virtual Expression VisitUnary(UnaryExpression u) {
			Expression operand = Visit(u.Operand);
			if (operand != u.Operand) {
				return Expression.Unary(u.ExpressionType, operand);
			}
			return u;
		}

		protected virtual Expression VisitBinary(BinaryExpression b) {
			Expression left = Visit(b.First);
			Expression right = Visit(b.Second);

			if (left != b.First || right != b.Second) {
				return Expression.Binary(left, b.ExpressionType, right);
			}

			return b;
		}

		protected virtual Expression VisitTypeIs(TypeIsExpression b) {
			Expression expr = Visit(b.Expression);
			if (expr != b.Expression) {
				return Expression.Is(expr, b.TypeOperand);
			}
			return b;
		}

		protected virtual Expression VisitConstant(ConstantExpression c) {
			return c;
		}

		protected virtual Expression VisitConditional(ConditionalExpression c) {
			Expression test = Visit(c.Test);
			Expression ifTrue = Visit(c.IfTrue);
			Expression ifFalse = Visit(c.IfFalse);
			if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse) {
				return Expression.Conditional(test, ifTrue, ifFalse);
			}
			return c;
		}

		protected virtual Expression VisitVariable(VariableExpression p) {
			return p;
		}

		protected virtual Expression VisitMethodCall(FunctionCallExpression m) {
			Expression obj = Visit(m.Object);
			IEnumerable<Expression> args = VisitExpressionList(m.Arguments.ToList().AsReadOnly());
			if (obj != m.Object || args != m.Arguments) {
				return Expression.FunctionCall(obj, m.FunctionName, args);
			}
			return m;
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