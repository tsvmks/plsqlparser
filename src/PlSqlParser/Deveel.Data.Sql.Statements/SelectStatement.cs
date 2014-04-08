using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Deveel.Data.Sql.Statements {
	public sealed class SelectStatement : Statement {
		public SelectStatement() {
			OrderBy = new Collection<ByColumn>();
		}

		public TableSelectExpression SelectExpression { get; set; }

		public ICollection<ByColumn> OrderBy { get; private set; }
	}
}