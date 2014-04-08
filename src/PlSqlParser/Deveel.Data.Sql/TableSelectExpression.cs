using System;
using System.Collections.Generic;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class TableSelectExpression {
		private Expression whereClause;
		private bool whereSet;

		public TableSelectExpression() {
			Columns = new List<SelectColumn>();
			CompositeFunction = CompositeFunction.None;
			Into = new SelectIntoClause();
			From = new FromClause();
			GroupBy = new List<ByColumn>();
		}

		public FromClause From { get; private set; }

		public SelectIntoClause Into { get; private set; }

		public Expression Where {
			get { return whereClause; }
			set {
				whereClause = value;
				whereSet = true;
			}
		}

		public bool Distinct { get; set; }

		public ICollection<ByColumn> GroupBy { get; private set; }

		public string GroupMax { get; set; }

		public Expression Having { get; set; }

		public bool IsCompositeAll { get; private set; }

		public CompositeFunction CompositeFunction { get; private set; }

		public TableSelectExpression NextComposite { get; private set; }

		public ICollection<SelectColumn> Columns { get; private set; }

		public void ChainComposite(TableSelectExpression expression, CompositeFunction composite, bool isAll) {
			NextComposite = expression;
			CompositeFunction = composite;
			IsCompositeAll = isAll;
		}
	}
}