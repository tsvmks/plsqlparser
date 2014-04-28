using System;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Expressions {
	public sealed class SubQueryExpression : Expression {
		public SubQueryExpression(TableSelectExpression select) {
			SelectExpression = select;
		}

		public TableSelectExpression SelectExpression { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Query; }
		}
	}
}