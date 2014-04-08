using System;
using System.Collections.Generic;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class FromClause {
		internal FromClause() {
		}

		private JoiningSet joinSet = new JoiningSet();
		private List<FromTable> fromTableList = new List<FromTable>();
		private List<string> allTableNames = new List<string>();
		private int tableKey;

		private String CreateNewKey() {
			++tableKey;
			return tableKey.ToString();
		}


		private void AddFromTable(string tableName, FromTable table) {
			if (tableName != null) {
				if (allTableNames.Contains(tableName))
					throw new ApplicationException("Duplicate table name in FROM clause: " + tableName);

				allTableNames.Add(tableName);
			}

			// Create a new unique key for this table
			string key = CreateNewKey();
			table.UniqueKey = key;
			// Add the table key to the join set
			joinSet.AddTable(key);
			// Add to the alias def map
			fromTableList.Add(table);
		}

		public void AddTable(string tableName) {
			AddFromTable(tableName, new FromTable(tableName));
		}

		public void AddTable(string tableName, string tableAlias) {
			AddFromTable(tableAlias, new FromTable(tableName, tableAlias));
		}

		public void AddTableDeclaration(string tableName, TableSelectExpression select, string tableAlias) {
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
			joinSet.AddJoin(type);
		}

		public void AddPreviousJoin(JoinType type, Expression onExpression) {
			joinSet.AddPreviousJoin(type, onExpression);
		}

		public void AddJoin(JoinType type, Expression onExpression) {
			joinSet.AddJoin(type, onExpression);
		}

		public JoiningSet JoinSet {
			get { return joinSet; }
		}

		public JoinType GetJoinType(int n) {
			return JoinSet.GetJoinType(n);
		}

		public Expression GetOnExpression(int n) {
			return JoinSet.GetOnExpression(n);
		}

		public ICollection<FromTable> AllTables {
			get { return fromTableList.AsReadOnly(); }
		}
	}
}