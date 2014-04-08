using System;

using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Expressions {
	public sealed class SubQueryExpression : Expression {
		public SubQueryExpression(SelectStatement select) {
			SelectStatement = select;
		}

		public SelectStatement SelectStatement { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Query; }
		}
	}
}