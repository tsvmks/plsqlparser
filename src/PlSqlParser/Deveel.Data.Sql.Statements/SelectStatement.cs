using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Deveel.Data.Sql.Statements {
	public sealed class SelectStatement : Statement {
		public SelectStatement() {
			OrderBy = new Collection<ByColumn>();
		}

		public TableSelectExpression SelectExpression { get; set; }

		public ICollection<ByColumn> OrderBy { get; private set; }

		protected override void DumpTo(StringBuilder builder) {
			SelectExpression.DumpSqlTo(builder);

			if (OrderBy.Count > 0) {
				builder.Append("ORDER BY ");

				var orderByCount = OrderBy.Count;
				var i = -1;
				foreach (var column in OrderBy) {
					column.DumpSqlTo(builder);

					if (++i < orderByCount - 1)
						builder.Append(", ");				
				}
			}

			base.DumpTo(builder);
		}
	}
}