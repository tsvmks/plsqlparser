using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class SelectColumn {
		public SelectColumn(Expression expression, string alias) {
			Alias = alias;
			Expression = expression;
		}

		public SelectColumn(Expression expression)
			: this(expression, null) {
		}

		public Expression Expression { get; private set; }

		public string Alias { get; private set; }
	}
}