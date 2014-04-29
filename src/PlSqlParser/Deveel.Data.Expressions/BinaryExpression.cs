using System;
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public abstract class BinaryExpression : Expression {
		public Expression First { get; private set; }

		public Expression Second { get; private set; }

		protected BinaryExpression(Expression first, Expression second) {
			First = first;
			Second = second;
		}

		internal abstract DataObject Evaluate(DataObject ob1,
			DataObject ob2,
			IGroupResolver group,
			IVariableResolver resolver,
			IQueryContext context);
	}
}