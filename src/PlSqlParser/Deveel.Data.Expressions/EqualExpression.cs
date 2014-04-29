using System;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public sealed class EqualExpression : BinaryExpression {
		public EqualExpression(Expression first, Expression second) 
			: base(first, second) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Equal; }
		}

		internal override DataObject Evaluate(DataObject ob1, DataObject ob2, IGroupResolver @group, IVariableResolver resolver, IQueryContext context) {
			return ob1.IsEqual(ob2);
		}
	}
}