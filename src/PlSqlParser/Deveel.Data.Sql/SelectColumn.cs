using System;
using System.Text;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class SelectColumn {
		public SelectColumn(Expression expression, ObjectName alias) {
			Alias = alias;
			Expression = expression;
		}

		public SelectColumn(Expression expression)
			: this(expression, null) {
		}

		public Expression Expression { get; private set; }

		public ObjectName Alias { get; private set; }

		internal void DumpSqlTo(StringBuilder builder) {
			Expression.DumpTo(builder);

			if (Alias != null) {
				builder.Append(" AS ");
				builder.Append(Alias);
			}
		}
	}
}