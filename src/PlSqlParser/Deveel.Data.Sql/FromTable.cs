using System;

namespace Deveel.Data.Sql {
	public sealed class FromTable {
		public FromTable(string tableName, string tableAlias) {
			Name = tableName;
			Alias = tableAlias;
			SubSelect = null;
			IsSubQueryTable = false;
		}

		public FromTable(string tableName)
			: this(tableName, null) {
		}

		public FromTable(TableSelectExpression select, string tableAlias) {
			SubSelect = select;
			Name = tableAlias;
			Alias = tableAlias;
			IsSubQueryTable = true;
		}

		public FromTable(TableSelectExpression select) {
			SubSelect = select;
			Name = null;
			Alias = null;
			IsSubQueryTable = true;
		}

		public string Name { get; private set; }

		public string Alias { get; private set; }

		internal string UniqueKey { get; set; }

		public bool IsSubQueryTable { get; private set; }

		public TableSelectExpression SubSelect { get; private set; }
	}
}