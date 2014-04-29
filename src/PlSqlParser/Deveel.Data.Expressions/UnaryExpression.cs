using System;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public abstract class UnaryExpression : Expression {
		protected UnaryExpression(Expression operand) {
			Operand = operand;
		}

		public Expression Operand { get; private set; }

		internal abstract DataObject Evaluate(DataObject obj, IGroupResolver group, IVariableResolver resolver, IQueryContext context);
	}
}