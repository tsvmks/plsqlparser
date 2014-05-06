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
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(1, result[0].RowCount);
		}

		[Test]
		public void InnerJoinSelect() {
			var connection = CreateConnection();
			var query = new SqlQuery("SELECT a.*, b.city, b.country FROM person a INNER JOIN lives b ON a.id = b.person_id WHERE a.first_name = 'Antonello';");

			var result = SqlExecutor.Execute(connection, query);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(1, result[0].RowCount);
		}

		[SetUp]
		public void SetUp() {
			database = new TestDatabase();

			var tableInfo = new DataTableInfo("APP", "person");
			tableInfo.AddColumn("id", PrimitiveTypes.Numeric());
			tableInfo.AddColumn("first_name", PrimitiveTypes.String());
			tableInfo.AddColumn("last_name", PrimitiveTypes.String());
			tableInfo.AddColumn("age", PrimitiveTypes.Numeric());

			var table = database.CreateTable(tableInfo);

			long rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(0));
			table.SetValue(1, rowIndex, new DataObject(PrimitiveTypes.String(), new StringObject("Antonello")));
			table.SetValue(2, rowIndex, new DataObject(PrimitiveTypes.String(), new StringObject("Provenzano")));
			table.SetValue(3, rowIndex, new DataObject(PrimitiveTypes.Numeric(), Number.FromInt32(32)));

			tableInfo = new DataTableInfo("APP", "lives");
			tableInfo.AddColumn("person_id", PrimitiveTypes.Numeric());
			tableInfo.AddColumn("city", PrimitiveTypes.String());
			tableInfo.AddColumn("country", PrimitiveTypes.String());

			table = database.CreateTable(tableInfo);
			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(0));
			table.SetValue(1, rowIndex, DataObject.String("Oslo"));
			table.SetValue(2, rowIndex, DataObject.String("Norway"));
		}

		private IDatabaseConnection CreateConnection() {
			return database.CreateConnection("APP", null, null);
		}
	}
}