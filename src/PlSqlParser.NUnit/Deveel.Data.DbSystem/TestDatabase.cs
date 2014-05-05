using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Deveel.Data.Index;
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
			tables[tableInfo.TableName] = table;
			return table;
		}

		#region DatabaseConnection

		class DatabaseConnection : IDatabaseConnection {
			public DatabaseConnection(TestDatabase database, string currentSchema) {
				Database = database;
				CurrentSchema = currentSchema;
				Transaction = new FakeTransaction(this);
			}

			public bool TableExists(ObjectName name) {
				return Database.tables.ContainsKey(name);
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
					for (int i = 0; i < tableInfo.ColumnCount; i++) {
						var columnInfo = tableInfo[i];
						newTableInfo.AddColumn(columnInfo.Name, columnInfo.DataType, !columnInfo.IsNotNull);
					}
					
					newTableInfo.IsReadOnly = true;
					tableInfo = newTableInfo;
				}

				return new TableQueryInfo(this, tableInfo, tableName, givenName);

			}

			private DataTableInfo GetTableInfo(ObjectName tableName) {
				ITable table;
				if (!Database.tables.TryGetValue(tableName, out table))
					return null;

				return table.TableInfo;
			}

			public string CurrentSchema { get; private set; }

			IDatabase IDatabaseConnection.Database {
				get { return Database; }
			}

			public TestDatabase Database { get; private set; }

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
			private DatabaseConnection connection;

			public FakeTransaction(DatabaseConnection connection) {
				this.connection = connection;
			}

			public ObjectName[] GetTables() {
				return connection.Database.tables.Keys.ToArray();
			}

			public bool TableExists(ObjectName tableName) {
				return connection.Database.tables.ContainsKey(tableName);
			}

			public bool RealTableExists(ObjectName tableName) {
				return TableExists(tableName);
			}

			public ObjectName ResolveToTableName(string currentSchema, string name, bool caseInsensitive) {
				return new ObjectName(new ObjectName(currentSchema), name);
			}

			public ObjectName TryResolveCase(ObjectName tableName) {
				throw new NotImplementedException();
			}

			public DataTableInfo GetTableInfo(ObjectName tableName) {
				ITable table;
				if (connection.Database.tables.TryGetValue(tableName, out table))
					return table.TableInfo;

				return null;
			}

			public ITable GetTable(ObjectName tableName) {
				ITable table;
				if (connection.Database.tables.TryGetValue(tableName, out table))
					return new DataTable(connection, table);

				return null;
			}

			public string GetTableType(ObjectName tableName) {
				return "SYSTEM";
			}
		}

		#endregion
	}
}
