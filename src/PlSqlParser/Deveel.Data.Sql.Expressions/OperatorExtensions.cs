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
using System.Text;

using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	static class OperatorExtensions {
		public static ExpressionType AsExpressionType(this Operator op) {
			if (op == Operator.Add)
				return ExpressionType.Add;
			if (op == Operator.Subtract)
				return ExpressionType.Subtract;
			if (op == Operator.Divide)
				return ExpressionType.Divide;
			if (op == Operator.Multiply)
				return ExpressionType.Multiply;

			if (op == Operator.Equal)
				return ExpressionType.Equal;
			if (op == Operator.NotEqual)
				return ExpressionType.NotEqual;

			throw new NotSupportedException(String.Format("Operator {0} does not resolve into an expression type.", op.AsString()));
		}

		public static DataType ReturnType(this Operator op) {
			if (op == Operator.Concat)
				return PrimitiveTypes.String();
			if (op.IsMathematical())
				return PrimitiveTypes.Numeric();

			return PrimitiveTypes.Boolean();
		}

		public static Operator AsOperator(this ExpressionType expressionType) {
			if (expressionType == ExpressionType.Add)
				return Operator.Add;
			if (expressionType == ExpressionType.Multiply)
				return Operator.Multiply;
			if (expressionType == ExpressionType.Divide)
				return Operator.Divide;
			if (expressionType == ExpressionType.Modulo)
				return Operator.Modulo;

			if (expressionType == ExpressionType.Equal)
				return Operator.Equal;
			if (expressionType == ExpressionType.NotEqual)
				return Operator.NotEqual;
			if (expressionType == ExpressionType.Greater)
				return Operator.Greater;
			if (expressionType == ExpressionType.GreaterOrEqual)
				return Operator.GreaterOrEqual;
			if (expressionType == ExpressionType.Smaller)
				return Operator.Smaller;
			if (expressionType == ExpressionType.SmallerOrEqual)
				return Operator.SmallerOrEqual;

			if (expressionType == ExpressionType.Like)
				return Operator.Like;

			if (expressionType == ExpressionType.Is)
				return Operator.Is;

			if (expressionType == ExpressionType.All ||
				expressionType == ExpressionType.Any)
				throw new ArgumentException("Expressions ANY or ALL are not supported from this form.");

			if (expressionType == ExpressionType.And)
				return Operator.And;
			if (expressionType == ExpressionType.Or)
				return Operator.Or;

			throw new ArgumentException(String.Format("The expression type {0} does not resolve into a binary operator.", expressionType));
		}

		public static string AsString(this Operator op) {
			var sb = new StringBuilder();
			var plainOp = op.Plain();
			if (plainOp == Operator.Add)
				sb.Append("+");
			if (plainOp == Operator.Divide)
				sb.Append("/");
			if (plainOp == Operator.Multiply)
				sb.Append("*");
			if (plainOp == Operator.Modulo)
				sb.Append("%");
			if (plainOp == Operator.Concat)
				sb.Append("||");

			if (plainOp == Operator.Equal)
				sb.Append("=");
			if (plainOp == Operator.NotEqual)
				sb.Append("<>");
			if (plainOp == Operator.Greater)
				sb.Append(">");
			if (plainOp == Operator.GreaterOrEqual)
				sb.Append(">=");
			if (plainOp == Operator.Smaller)
				sb.Append("<");
			if (plainOp == Operator.SmallerOrEqual)
				sb.Append("<=");

			if (plainOp == Operator.Is)
				sb.Append("IS");
			if (plainOp == Operator.IsNot)
				sb.Append("IS NOT");
			if (plainOp == Operator.Like)
				sb.Append("LIKE");
			if (plainOp == Operator.NotLike)
				sb.Append("NOT LIKE");

			if ((op & Operator.All) != 0)
				sb.Append(" ALL");
			if ((op & Operator.Any) != 0)
				sb.Append(" ANY");

			if (op == Operator.And)
				sb.Append("AND");
			if (op == Operator.Or)
				sb.Append("OR");

			return sb.ToString();
		}

		public static bool IsEquivalent(this Operator op, Operator otherOp) {
			return (op & otherOp) != 0;
		}

		public static bool IsMathematical(this Operator op) {
			return (op == Operator.Add ||
			        op == Operator.Subtract ||
			        op == Operator.Multiply ||
			        op == Operator.Divide ||
			        op == Operator.Modulo ||
			        op == Operator.Concat);
		}

		public static bool IsLogical(this Operator op) {
			return op == Operator.And || op == Operator.Or;
		}

		public static bool IsSubQuery(this Operator op) {
			return (op & Operator.All) != 0 ||
			       (op & Operator.Any) != 0 ;
		}

		public static bool IsPattern(this Operator op) {
			return op == Operator.Like || op == Operator.NotLike;
		}

		public static Operator Plain(this Operator op) {
			if ((op & Operator.All) != 0)
				return (op & ~Operator.All);
			if ((op & Operator.Any) != 0)
				return (op & ~Operator.Any);

			return op;
		}

		public static Operator SubType(this Operator op, Operator subType) {
			return (op & subType);
		}

		public static Operator Reverse(this Operator op) {
			if (op == Operator.Equal || 
				op == Operator.NotEqual || 
				op == Operator.Is ||
				op == Operator.IsNot)
				return op;
			if (op == Operator.Greater)
				return Operator.Smaller;
			if (op == Operator.Smaller)
				return Operator.Greater;
			if (op == Operator.GreaterOrEqual)
				return Operator.SmallerOrEqual;
			if (op == Operator.SmallerOrEqual)
				return Operator.GreaterOrEqual;

			throw new ApplicationException("Can't reverse a non conditional operator.");
		}

		public static Operator Inverse(this Operator op) {
			if ((op & Operator.Any) != 0 ||
				(op & Operator.All) != 0) {
				Operator invType;
				if ((op & Operator.Any) != 0) {
					invType = Operator.All;
				} else if ((op & Operator.All) != 0) {
					invType = Operator.Any;
				} else {
					throw new Exception("Can not handle sub-query form.");
				}

				op = op.Plain();
				Operator invOp = op.Inverse();

				return invOp & invType;
			}

			if (op == Operator.Equal)
				return Operator.NotEqual;
			if (op == Operator.NotEqual)
				return Operator.Equal;
			if (op == Operator.Greater)
				return Operator.SmallerOrEqual;
			if (op == Operator.Smaller)
				return Operator.GreaterOrEqual;
			if (op == Operator.GreaterOrEqual)
				return Operator.Smaller;
			if (op == Operator.SmallerOrEqual)
				return Operator.Greater;
			if (op == Operator.And)
				return Operator.Or;
			if (op == Operator.Or)
				return Operator.And;
			if (op == Operator.Like)
				return Operator.NotLike;
			if (op == Operator.NotLike)
				return Operator.Like;
			if (op == Operator.Is)
				return Operator.IsNot;
			if (op == Operator.IsNot)
				return Operator.Is;

			throw new ApplicationException("Can't inverse operator '" + op + "'");
		}
	}
}