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
using System.Diagnostics;
using System.Text;

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
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

		protected virtual Expression Visit(IExpressionVisitor visitor) {
			return visitor.Visit(this);
		}

		public static Expression Visit(Expression expression, IExpressionVisitor visitor) {
			return expression.Visit(visitor);
		}

		#region Factories

		public static OrExpression Or(Expression first, Expression second) {
			return new OrExpression(first, second);
		}

		public static BinaryExpression And(Expression first, Expression second) {
			return new AndExpression(first, second);
		}

		public static NotExpression Not(Expression expression) {
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
				throw new ArgumentException("An IN expression requires an ARRAY argument");

			return Any(expression, ExpressionType.Equal, Constant(array));
		}

		public static Expression NotIn(Expression expression, IEnumerable<Expression> group) {
			return NotIn(expression, DataObject.Array(group));
		}

		public static Expression NotIn(Expression expression, DataObject array) {
			if (!(array.DataType is ArrayType))
				throw new ArgumentException("A NOT IN expression requires an ARRAY argument");

			return All(expression, ExpressionType.NotEqual, Constant(array));
		}

		public static Expression Between(Expression expression, Expression min, Expression max) {
			return Or(Greater(expression, min), Smaller(expression, max));
		}

		public static Expression NotBetween(Expression expression, Expression min, Expression max) {
			return Not(Between(expression, min, max));
		}

		public static GreaterExpression Greater(Expression first, Expression second) {
			return new GreaterExpression(first, second);
		}

		public static GreaterOrEqualExpression GreaterOrEqual(Expression left, Expression right) {
			return new GreaterOrEqualExpression(left, right);
		}

		public static SmallerExpression Smaller(Expression first, Expression second) {
			return new SmallerExpression(first, second);
		}

		public static LikeExpression Like(Expression expression, Expression searchExpression) {
			return Like(expression, searchExpression, Constant(DataObject.String("%")));
		}

		public static LikeExpression Like(Expression expression, Expression searchExpression, Expression escape) {
			return new LikeExpression(expression, searchExpression, escape);
		}

		public static BinaryExpression IsNull(Expression expression) {
			return Binary(expression, ExpressionType.Is, Expression.Constant(DataObject.Null));
		}

		public static AddExpression Add(Expression first, Expression second) {
			return new AddExpression(first, second);
		}

		public static SubtractExpression Subtract(Expression left, Expression right) {
			return new SubtractExpression(left, right);
		}

		public static MultiplyExpression Multiply(Expression first, Expression second) {
			return new MultiplyExpression(first, second);
		}

		public static DivideExpression Divide(Expression left, Expression right) {
			return new DivideExpression(left, right);
		}

		public static ModuloExpression Modulo(Expression left, Expression right) {
			return new ModuloExpression(left, right);
		}

		public static ExponentExpression Exponent(Expression left, Expression right) {
			return new ExponentExpression(left, right);
		}

		public static AnyExpression Any(Expression first, ExpressionType subType, Expression second) {
			return new AnyExpression(first, subType, second);
		}

		public static BinaryExpression Binary(Expression first, ExpressionType type, Expression second) {
			if (type == ExpressionType.Equal)
				return Equal(first, second);
			if (type == ExpressionType.NotEqual)
				return NotEqual(first, second);
			if (type == ExpressionType.Smaller)
				return Smaller(first, second);
			if (type == ExpressionType.Greater)
				return Greater(first, second);
			if (type == ExpressionType.SmallerOrEqual)
				return SmallerOrEqual(first, second);
			if (type == ExpressionType.GreaterOrEqual)
				return GreaterOrEqual(first, second);

			if (type == ExpressionType.Add)
				return Add(first, second);
			if (type == ExpressionType.Multiply)
				return Multiply(first, second);
			if (type == ExpressionType.Subset)
				return Subtract(first, second);
			if (type == ExpressionType.Divide)
				return Divide(first, second);
			if (type == ExpressionType.Modulo)
				return Modulo(first, second);
			if (type == ExpressionType.Exponent)
				return Exponent(first, second);

			if (type == ExpressionType.Like)
				return Like(first, second);

			if (type == ExpressionType.And)
				return And(first, second);
			if (type == ExpressionType.Or)
				return Or(first, second);

			throw new NotSupportedException();
		}

		private static SmallerOrEqualExpression SmallerOrEqual(Expression left, Expression right) {
			return new SmallerOrEqualExpression(left, right);
		}

		public static UnaryExpression Negative(Expression expression) {
			throw new NotImplementedException();
		}

		public static ConstantExpression Constant(DataObject value) {
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
			return Variable(new ObjectName(name));
		}

		public static Expression Variable(ObjectName name) {
			return new VariableExpression(name);
		}

		public static FunctionCallExpression FunctionCall(Expression obj, ObjectName functionName, IEnumerable<Expression> arguments) {
			return new FunctionCallExpression(obj, functionName, arguments);
		}

		public static FunctionCallExpression FunctionCall(ObjectName functionName, IEnumerable<Expression> args) {
			return new FunctionCallExpression(functionName, args);
		}

		public static FunctionCallExpression FunctionCall(ObjectName functionName, Expression arg) {
			return FunctionCall(functionName, new Expression[] {arg});
		}

		public static FunctionCallExpression FunctionCall(string functionName, IEnumerable<Expression> args) {
			return FunctionCall(ObjectName.Parse(functionName), args);
		}

		public static FunctionCallExpression FunctionCall(string functionName, Expression arg) {
			return FunctionCall(functionName, new Expression[] {arg});
		}

		public static Expression Conditional(Expression test, Expression ifTrue, Expression ifFalse) {
			return new ConditionalExpression(test, ifTrue, ifFalse);
		}


		public static Expression Conditional(Expression test, Expression ifTrue) {
			return Conditional(test, ifTrue, null);
		}

		public static TypeIsExpression Is(Expression expression, DataType type) {
			return new TypeIsExpression(expression, type);
		}

		public static UnaryExpression Unary(ExpressionType type, Expression expression) {
			if (type == ExpressionType.Negate)
				return Negative(expression);
			if (type == ExpressionType.Not)
				return Not(expression);

			throw new NotImplementedException();
		}

		#endregion

		protected virtual void DumpToString(StringBuilder sb) {
		}

		public override string ToString() {
			var builder = new StringBuilder();
			var dump = new ExpressionStringDump(builder);
			dump.Dump(this);
			return builder.ToString();
		}

		internal void DumpTo(StringBuilder builder) {
			var dump = new ExpressionStringDump(builder);
			dump.Dump(this);
		}

		#region ExpressionStringDump

		class ExpressionStringDump : ExpressionVisitor {
			private readonly StringBuilder builder;

			public ExpressionStringDump(StringBuilder builder) {
				this.builder = builder;
			}

			public void Dump(Expression expression) {
				Visit(expression);
			}

			protected override Expression VisitBinary(BinaryExpression expression) {
				expression.Left.DumpTo(builder);
				builder.AppendFormat(" {0} ", expression.Operator.AsString());
				expression.Right.DumpTo(builder);
				return expression;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				builder.Append(expression.Value);
				return expression;
			}

			protected override SubsetExpression VisitSubset(SubsetExpression expression) {
				builder.Append("(");
				expression.Operand.DumpTo(builder);
				builder.Append(")");
				return expression;
			}

			protected override Expression VisitVariable(VariableExpression expression) {
				builder.AppendFormat(":{0}", expression.VariableName);
				return expression;
			}

			protected override Expression VisitConditional(ConditionalExpression expression) {
				builder.Append("CASE ");
				expression.IfTrue.DumpTo(builder);
				builder.Append(" WHEN ");
				expression.Test.DumpTo(builder);

				if (expression.IfFalse != null) {
					builder.Append(" THEN ");
					expression.IfFalse.DumpTo(builder);
				}

				builder.Append(" END");

				return expression;
			}
		}

		#endregion

		public static SubQueryExpression Query(TableSelectExpression expression) {
			return new SubQueryExpression(expression);
		}

		public static Expression Operator(DataObject first, Operator op, DataObject second) {
			if (op == Expressions.Operator.Add)
				return Add(Constant(first), Constant(second));

			if (op == Expressions.Operator.Equal)
				return Equal(Constant(first), Constant(second));
			if (op == Expressions.Operator.NotEqual)
				return NotEqual(Constant(first), Constant(second));

			if (op == Expressions.Operator.Smaller)
				return Smaller(Constant(first), Constant(second));

			throw new ArgumentException();
		}

		public static Expression All(Expression first, ExpressionType subType, Expression second) {
			throw new NotImplementedException();
		}

		public static ConstantExpression Array(IEnumerable<Expression> list) {
			return Constant(new DataObject(new ArrayType(), list));
		}
	}
}
