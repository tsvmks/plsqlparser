using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Deveel.Data.Query;
using Deveel.Data.Transactions;

namespace Deveel.Data.DbSystem {
	public class TestDatabase : IDatabase {
		private readonly Dictionary<ObjectName, ITable> tables;

		public TestDatabase() {
			tables = new Dictionary<ObjectName, ITable>();
			CellCache = new FakeCellCache();
		}

		public ICellCache CellCache { get; private set; }

		public ITable SingleRowTable { get; private set; }

		public IDatabaseConnection CreateConnection(string schema, string userName, string password) {
			return new DatabaseConnection(this, schema);
		}

		public TestTable CreateTable(DataTableInfo tableInfo) {
			var table = new TestTable(tableInfo);
			tables[tableInfo.Name] = table;
			return table;
		}

		#region TestTable

		#endregion

		#region DatabaseConnection

		class DatabaseConnection : IDatabaseConnection {
			private readonly TestDatabase database;

			public DatabaseConnection(TestDatabase database, string currentSchema) {
				this.database = database;
				CurrentSchema = currentSchema;
				Transaction = new FakeTransaction(database);
			}

			public bool TableExists(ObjectName name) {
				return database.tables.ContainsKey(name);
			}

			public bool IsInCaseInsensitive {
				get { return true; }
			}

			public ObjectName ResolveTableName(ObjectName name) {
				if (name.Parent == null)
					name = new ObjectName(new ObjectName(CurrentSchema), name.Name);

				return name;
			}

			public ITableQueryInfo GetTableQueryInfo(ObjectName tableName, ObjectName givenName) {
				DataTableInfo tableInfo = GetTableInfo(tableName);
				// If the table is aliased, set a new DataTableInfo with the given name
				if (givenName != null) {
					var newTableInfo = new DataTableInfo(givenName.Clone());
					foreach (var columnInfo in tableInfo) {
						newTableInfo.AddColumn(columnInfo.Name, columnInfo.DataType, columnInfo.IsNullable);
					}
					
					newTableInfo.IsReadOnly = true;
					tableInfo = newTableInfo;
				}

				return new TableQueryInfo(this, tableInfo, tableName, givenName);

			}

			private DataTableInfo GetTableInfo(ObjectName tableName) {
				ITable table;
				if (!database.tables.TryGetValue(tableName, out table))
					return null;

				return table.TableInfo;
			}

			public string CurrentSchema { get; private set; }

			public IDatabase Database {
				get { return database; }
			}

			public ITransaction Transaction { get; private set; }

			public IQueryPlanNode CreateObjectFetchQueryPlan(ObjectName tableName, ObjectName aliasedAs) {
				return new FetchTableNode(tableName, aliasedAs);
			}

			private class TableQueryInfo : ITableQueryInfo {
				private readonly IDatabaseConnection conn;
				private readonly DataTableInfo tableInfo;
				private readonly ObjectName tableName;
				private readonly ObjectName aliasedAs;

				public TableQueryInfo(IDatabaseConnection conn, DataTableInfo tableInfo, ObjectName tableName, ObjectName aliasedAs) {
					this.conn = conn;
					this.tableInfo = tableInfo;
					this.aliasedAs = aliasedAs;
					this.tableName = tableName;
				}

				public DataTableInfo TableInfo {
					get { return tableInfo; }
				}

				public IQueryPlanNode QueryPlanNode {
					get { return conn.CreateObjectFetchQueryPlan(tableName, aliasedAs); }
				}
			}
		}

		#endregion

		#region FakeCellCache

		class FakeCellCache : ICellCache {
			public void Set(int id, long row, int column, DataObject value) {
			}

			public DataObject Get(int id, long row, int column) {
				return null;
			}
		}

		#endregion

		#region FakeTransaction

		class FakeTransaction : ITransaction {
			private readonly TestDatabase database;

			public FakeTransaction(TestDatabase database) {
				this.database = database;
			}

			public ObjectName[] GetTables() {
				return database.tables.Keys.ToArray();
			}

			public bool TableExists(ObjectName tableName) {
				return database.tables.ContainsKey(tableName);
			}

			public bool RealTableExists(ObjectName tableName) {
				throw new NotImplementedException();
			}

			public ObjectName ResolveToTableName(string currentSchema, string name, bool caseInsensitive) {
				return new ObjectName(new ObjectName(currentSchema), name);
			}

			public ObjectName TryResolveCase(ObjectName tableName) {
				throw new NotImplementedException();
			}

			public DataTableInfo GetTableInfo(ObjectName tableName) {
				ITable table;
				if (database.tables.TryGetValue(tableName, out table))
					return table.TableInfo;

				return null;
			}

			public ITable GetTable(ObjectName tableName) {
				ITable table;
				if (database.tables.TryGetValue(tableName, out table))
					return table;

				return null;
			}

			public string GetTableType(ObjectName tableName) {
				return "SYSTEM";
			}
		}

		#endregion
	}
}
