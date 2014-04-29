using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	[Serializable]
	public sealed class FromTable : IPreparable {
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

		private FromTable() {
		}

		public ObjectName Name { get; private set; }

		public ObjectName Alias { get; private set; }

		internal string UniqueKey { get; set; }

		public bool IsSubQueryTable { get; private set; }

		public TableSelectExpression SubSelect { get; private set; }

		public FromTable Prepare(IExpressionPreparer preparer) {
			var fromTable = new FromTable {
				Name = Name, 
				Alias = 
				Alias, 
				UniqueKey = UniqueKey, 
				IsSubQueryTable = IsSubQueryTable
			};
			if (SubSelect != null)
				fromTable.SubSelect = SubSelect.Prepare(preparer);

			return fromTable;
		}

		object IPreparable.Prepare(IExpressionPreparer preparer) {
			return Prepare(preparer);
		}
	}
}