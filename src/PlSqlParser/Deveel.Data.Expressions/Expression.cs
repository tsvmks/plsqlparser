using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Deveel.Data.Sql;
using Deveel.Data.Types;

namespace Deveel.Data.Expressions {
	[DebuggerDisplay("{ToString()}")]
	public abstract class Expression {
		public abstract ExpressionType ExpressionType { get; }

		internal int Precedence {
			get { return GetPrecedence(); }
		}

		private int GetPrecedence() {
			if (ExpressionType == ExpressionType.Subset)
				return 58;
			if (ExpressionType == ExpressionType.Query)
				return 56;
			if (ExpressionType == ExpressionType.Cast ||
				ExpressionType == ExpressionType.Is)
				return 40;
			if (ExpressionType == ExpressionType.Multiply ||
			    ExpressionType == ExpressionType.Divide ||
			    ExpressionType == ExpressionType.Modulo)
				return 30;
			if (ExpressionType == ExpressionType.Add ||
			    ExpressionType == ExpressionType.Subtract)
				return 29;
			if (ExpressionType == ExpressionType.Equal ||
			    ExpressionType == ExpressionType.NotEqual)
				return 28;

			if (ExpressionType == ExpressionType.And ||
			    ExpressionType == ExpressionType.Or)
				return 20;

			if (ExpressionType == ExpressionType.Assign)
				return 18;

			// TODO:
			return 1;
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
			return new AndExpression(first, second);
		}

		public static Expression Not(Expression expression) {
			return new NotExpression(expression);
		}

		public static Expression Relational(Expression first, ExpressionType op, Expression second) {
			if (op == ExpressionType.Equal)
				return Equal(first, second);

			throw new ArgumentException(String.Format("Expression type {0} is not relational", op));
		}

		public static Expression In(Expression expression, IEnumerable<Expression> group) {
			return In(expression, DataObject.Array(group));
		}

		public static Expression In(Expression expression, DataObject array) {
			if (!(array.DataType is ArrayType))
				throw new ArgumentException();

			return null;
		}

		public static Expression Between(Expression expression, Expression min, Expression max) {
			return Or(Greater(expression, min), Smaller(expression, max));
		}

		public static Expression NotBetween(Expression expression, Expression min, Expression max) {
			return null;
		}

		public static Expression Greater(Expression first, Expression second) {
			return null;
		}

		public static Expression Smaller(Expression first, Expression second) {
			return null;
		}

		public static Expression Like(Expression expression, Expression searchExpression, Expression escape) {
			return new LikeExpression(expression, searchExpression, escape);
		}

		public static BinaryExpression IsNull(Expression expression) {
			return Binary(expression, ExpressionType.Is, Expression.Constant(DataObject.Null));
		}

		public static AddExpression Add(Expression first, Expression second) {
			return new AddExpression(first, second);
		}

		public static MultiplyExpression Multiply(Expression first, Expression second) {
			return new MultiplyExpression(first, second);
		}

		public static BinaryExpression Binary(Expression first, ExpressionType type, Expression second) {
			if (type == ExpressionType.Equal)
				return Equal(first, second);
			if (type == ExpressionType.NotEqual)
				return NotEqual(first, second);

			if (type == ExpressionType.Add)
				return Add(first, second);
			if (type == ExpressionType.Multiply)
				return Multiply(first, second);

			return null;
		}

		public static Expression Negative(Expression expression) {
			return null;
		}

		public static Expression Constant(DataObject value) {
			return new ConstantExpression(value);
		}

		public static EqualExpression Equal(Expression first, Expression second) {
			return new EqualExpression(first, second);
		}

		public static NotEqualExpression NotEqual(Expression first, Expression second) {
			return new NotEqualExpression(first, second);
		}

		public static Expression Subset(Expression expression) {
			return new SubsetExpression(expression);
		}

		public static Expression Variable(string name) {
			return Variable(new VariableBind(name));
		}

		public static VariableExpression Variable(VariableBind name) {
			return new VariableExpression(name);
		}

		public static Expression Variable(ObjectName name) {
			return Variable(new VariableBind(name.ToString()));
		}

		public static FunctionCallExpression FunctionCall(Expression obj, ObjectName functionName, IEnumerable<Expression> arguments) {
			return new FunctionCallExpression(obj, functionName, arguments);
		}

		public static Expression FunctionCall(ObjectName functionName, FunctionArgument arg) {
			return FunctionCall(functionName, new[] { arg});
		}

		public static FunctionCallExpression FunctionCall(ObjectName functionName, IEnumerable<FunctionArgument> args) {
			return null;
		}

		public static FunctionCallExpression FunctionCall(string functionName, IEnumerable<FunctionArgument> args) {
			return FunctionCall(ObjectName.Parse(functionName), args);
		}

		public static FunctionCallExpression FunctionCall(string functionName, FunctionArgument arg) {
			return FunctionCall(functionName, new[] {arg});
		}

		public static Expression Conditional(Expression test, Expression ifTrue, Expression ifFalse) {
			return new ConditionalExpression(test, ifTrue, ifFalse);
		}


		public static Expression Conditional(Expression test, Expression ifTrue) {
			return Conditional(test, ifTrue, null);
		}

		#endregion

		public static TypeIsExpression Is(Expression expression, DataType type) {
			return new TypeIsExpression(expression, type);
		}

		public static Expression Unary(ExpressionType type, Expression expression) {
			throw new NotImplementedException();
		}

		protected virtual void DumpToString(StringBuilder sb) {
		}

		public override string ToString() {
			var dump = new ExpressionStringDump();
			return dump.ToString(this);
		}

		internal void DumpTo(StringBuilder builder) {
			var dump = new ExpressionStringDump(builder);
			dump.ToString(this);
		}

		#region ExpressionStringDump

		class ExpressionStringDump : ExpressionVisitor {
			private readonly StringBuilder builder;

			public ExpressionStringDump(StringBuilder builder) {
				this.builder = builder;
			}

			public ExpressionStringDump()
				: this(new StringBuilder()) {
			}

			protected override Expression Visit(Expression exp) {
				exp.DumpToString(builder);
				return base.Visit(exp);
			}

			public string ToString(Expression expression) {
				Visit(expression);
				return builder.ToString();
			}
		}

		#endregion

		public static SubQueryExpression Query(TableSelectExpression expression) {
			return new SubQueryExpression(expression);
		}
	}
}
