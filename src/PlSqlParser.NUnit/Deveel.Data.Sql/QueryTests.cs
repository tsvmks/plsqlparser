using System;

using Deveel.Data.Sql;
using Deveel.Data.Types;

using NUnit.Framework;

namespace Deveel.Data.DbSystem {
	[TestFixture]
	public class QueryTests {
		private TestDatabase database;

		[Test]
		public void SimpleSelect() {
			var connection = CreateConnection();

			var query = new SqlQuery("SELECT * FROM person WHERE first_name = 'Antonello';");
			var result = SqlExecutor.Execute(connection, query);

			Assert.IsNotNull(result);
		}

		[SetUp]
		public void SetUp() {
			database = new TestDatabase();

			var personTableInfo = new DataTableInfo("APP", "person");
			personTableInfo.AddColumn("first_name", PrimitiveTypes.String());
			personTableInfo.AddColumn("last_name", PrimitiveTypes.String());
			personTableInfo.AddColumn("age", PrimitiveTypes.Numeric());

			var table = database.CreateTable(personTableInfo);

			long rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, new DataObject(PrimitiveTypes.String(), new StringObject("Antonello")));
			table.SetValue(1, rowIndex, new DataObject(PrimitiveTypes.String(), new StringObject("Provenzano")));
			table.SetValue(2, rowIndex, new DataObject(PrimitiveTypes.Numeric(), Number.FromInt32(32)));
		}

		private IDatabaseConnection CreateConnection() {
			return database.CreateConnection("APP", null, null);
		}
	}
}