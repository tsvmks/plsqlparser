using System;
using System.Text;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class ByColumn {
		public ByColumn(Expression exp, bool ascending) {
			Expression = exp;
			Ascending = ascending;
		}

		public ByColumn(Expression exp)
			: this(exp, true) {
		}

		public Expression Expression { get; private set; }

		public bool Ascending { get; private set; }

		internal void DumpSqlTo(StringBuilder builder) {
			Expression.DumpTo(builder);

			builder.Append(" ");
			builder.Append(Ascending ? "ASC" : "DESC");
		}
	}
}