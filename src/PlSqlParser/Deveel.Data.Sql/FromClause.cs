using System;
using System.Collections.Generic;
using System.Text;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class FromClause {
		internal FromClause() {
			JoinSet = new JoiningSet();
		}

		private List<FromTable> fromTableList = new List<FromTable>();
		private List<ObjectName> allTableNames = new List<ObjectName>();
		private int tableKey;

		private String CreateNewKey() {
			++tableKey;
			return tableKey.ToString();
		}


		private void AddFromTable(ObjectName tableName, FromTable table) {
			if (tableName != null) {
				if (allTableNames.Contains(tableName))
					throw new ApplicationException("Duplicate table name in FROM clause: " + tableName);

				allTableNames.Add(tableName);
			}

			// Create a new unique key for this table
			string key = CreateNewKey();
			table.UniqueKey = key;
			// Add the table key to the join set
			JoinSet.AddTable(key);
			// Add to the alias def map
			fromTableList.Add(table);
		}

		public void AddTable(ObjectName tableName) {
			AddFromTable(tableName, new FromTable(tableName));
		}

		public void AddTable(ObjectName tableName, ObjectName tableAlias) {
			AddFromTable(tableAlias, new FromTable(tableName, tableAlias));
		}

		public void AddTableDeclaration(ObjectName tableName, TableSelectExpression select, ObjectName tableAlias) {
			// This is an inner select in the FROM clause
			if (tableName == null && select != null) {
				if (tableAlias == null) {
					AddFromTable(null, new FromTable(select));
				} else {
					AddFromTable(tableAlias, new FromTable(select, tableAlias));
				}
			}
				// This is a standard table reference in the FROM clause
			else if (tableName != null && select == null) {
				if (tableAlias == null) {
					AddTable(tableName);
				} else {
					AddTable(tableName, tableAlias);
				}
			}
				// Error
			else {
				throw new ApplicationException("Unvalid declaration parameters.");
			}
		}

		public void AddJoin(JoinType type) {
			JoinSet.AddJoin(type);
		}

		public void AddPreviousJoin(JoinType type, Expression onExpression) {
			JoinSet.AddPreviousJoin(type, onExpression);
		}

		public void AddJoin(JoinType type, Expression onExpression) {
			JoinSet.AddJoin(type, onExpression);
		}

		public JoiningSet JoinSet { get; private set; }

		public JoinType GetJoinType(int n) {
			return JoinSet.GetJoinType(n);
		}

		public Expression GetOnExpression(int n) {
			return JoinSet.GetOnExpression(n);
		}

		public ICollection<FromTable> AllTables {
			get { return fromTableList.AsReadOnly(); }
		}

		internal void DumpSqlTo(StringBuilder builder) {
			builder.Append("FROM ");

		}

		public FromClause Prepare(IExpressionPreparer preparer) {
			var clause = new FromClause();

			clause.JoinSet = JoinSet.Prepare(preparer);

			// Prepare the StatementTree sub-queries in the from tables
			foreach (FromTable table in fromTableList) {
				clause.fromTableList.Add(table.Prepare(preparer));
			}

			return clause;
		}
	}
}