using System;

using Deveel.Data.Types;

namespace Deveel.Data.Expressions {
	public sealed class TypeIsExpression : Expression {
		public TypeIsExpression(Expression expression, DataType typeOperand) {
			if (typeOperand == null)
				throw new ArgumentNullException("typeOperand");

			TypeOperand = typeOperand;
			Expression = expression;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Is; }
		}

		public Expression Expression { get; private set; }

		public DataType TypeOperand { get; private set; }
	}
}