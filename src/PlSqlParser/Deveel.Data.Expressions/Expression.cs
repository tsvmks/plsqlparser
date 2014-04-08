using System;
using System.Collections.Generic;

using Deveel.Data.Sql;

namespace Deveel.Data.Expressions {
	public abstract class Expression {
		public abstract ExpressionType ExpressionType { get; }

		internal int Precedence {
			get { return GetPrecedence(); }
		}

		private int GetPrecedence() {
			// TODO:
			return -1;
		}

		protected virtual void Visit(IExpressionVisitor visitor) {
			visitor.Visit(this);
		}

		public static void Visit(Expression expression, IExpressionVisitor visitor) {
			expression.Visit(visitor);
		}

		#region Factories

		public static Expression Or(Expression first, Expression second) {
			return null;
		}

		public static Expression And(Expression first, Expression second) {
			return null;
		}

		public static Expression Not(Expression expression) {
			return null;
		}

		public static Expression Relational(Expression first, ExpressionType op, Expression second) {
			if (op == ExpressionType.Equals)
				return Equal(first, second);

			throw new ArgumentException(String.Format("Expression type {0} is not relational", op));
		}

		public static Expression In(Expression expression, IEnumerable<Expression> group) {
			return null;
		}

		public static Expression Between(Expression expression, Expression min, Expression max) {
			return null;
		}

		public static Expression Like(Expression expression, Expression searchExpression, Expression escape) {
			return null;
		}

		public static Expression IsNull(Expression expression) {
			return null;
		}

		public static BinaryExpression Binary(Expression first, ExpressionType type, Expression second) {
			if (type == ExpressionType.Equals)
				return Equal(first, second);

			return null;
		}

		public static Expression Negative(Expression expression) {
			return null;
		}

		public static Expression Constant(object value) {
			return new ConstantExpression(value);
		}

		public static Expression Subquery(TableSelectExpression selectExpression) {
			return null;
		}

		public static EqualExpression Equal(Expression first, Expression second) {
			return new EqualExpression(first, second);
		}

		public static Expression Subset(Expression expression) {
			return null;
		}

		public static Expression Variable(string name) {
			return new VariableExpression(name);
		}

		public static FunctionCallExpression FunctionCall(Expression obj, string functionName, IEnumerable<Expression> arguments) {
			return new FunctionCallExpression(obj, functionName, arguments);
		}

		public static Expression FunctionCall(string functionName, FunctionArgument arg) {
			return null;
		}

		public static Expression Conditional(Expression test, Expression ifTrue) {
			return null;
		}

		public static Expression FunctionCall(string functionName, IEnumerable<FunctionArgument> args) {
			return null;
		}

		public static Expression Conditional(Expression test, Expression ifTrue, Expression ifFalse) {
			return null;
		}

		#endregion

		public static TypeIsExpression Is(Expression expression, Type type) {
			return new TypeIsExpression(expression, type);
		}

		public static Expression Unary(ExpressionType type, Expression expression) {
			throw new NotImplementedException();
		}
	}
}
