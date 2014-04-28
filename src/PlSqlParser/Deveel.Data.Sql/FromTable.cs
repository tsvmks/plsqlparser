using System;

namespace Deveel.Data.Sql {
	public sealed class FromTable {
		public FromTable(ObjectName tableName, ObjectName tableAlias) {
			Name = tableName;
			Alias = tableAlias;
			SubSelect = null;
			IsSubQueryTable = false;
		}

		public FromTable(ObjectName tableName)
			: this(tableName, null) {
		}

		public FromTable(TableSelectExpression select, ObjectName tableAlias) {
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

		public ObjectName Name { get; private set; }

		public ObjectName Alias { get; private set; }

		internal string UniqueKey { get; set; }

		public bool IsSubQueryTable { get; private set; }

		public TableSelectExpression SubSelect { get; private set; }
	}
}