using System;
using System.Text;

namespace Deveel.Data.Expressions {
	public abstract class BinaryExpression : Expression {
		public Expression First { get; private set; }

		public Expression Second { get; private set; }

		protected BinaryExpression(Expression first, Expression second) {
			First = first;
			Second = second;
		}

		internal virtual Operator Operator {
			get { return null; }
		}
	}
}