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

		[Test]
		public void SimpleJoinSelect() {
			var connection = CreateConnection();
			var query = new SqlQuery("SELECT a.*, b.city, b.country FROM person a, lives b WHERE a.first_name = 'Antonello' AND a.id = b.person_id;");

			var result = SqlExecutor.Execute(connection, query);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(1, result[0].RowCount);
		}

		[Test]
		public void GroupBySelect() {
			var connection = CreateConnection();
			var query = new SqlQuery("SELECT a.*, COUNT(b.id) AS device_count FROM person a LEFT JOIN devices b ON a.id = b.person_id GROUP BY a.first_name HAVING a.first_name = 'Antonello';");

			var result = SqlExecutor.Execute(connection, query);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(1, result[0].RowCount);
			Assert.IsNotNull(result[0].GetValue(result[0].TableInfo.ColumnCount - 1, 0));
			Assert.AreEqual(4, result[0].GetValue(result[0].TableInfo.ColumnCount - 1, 0).ToNumber().ToInt32());
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

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(1));
			table.SetValue(1, rowIndex, DataObject.String("Mart"));
			table.SetValue(2, rowIndex, DataObject.String("Roosmaa"));
			table.SetValue(3, rowIndex, DataObject.Number(28));

			tableInfo = new DataTableInfo("APP", "lives");
			tableInfo.AddColumn("person_id", PrimitiveTypes.Numeric());
			tableInfo.AddColumn("city", PrimitiveTypes.String());
			tableInfo.AddColumn("country", PrimitiveTypes.String());
			table = database.CreateTable(tableInfo);

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(0));
			table.SetValue(1, rowIndex, DataObject.String("Oslo"));
			table.SetValue(2, rowIndex, DataObject.String("Norway"));

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(1));
			table.SetValue(1, rowIndex, DataObject.String("Tallinn"));
			table.SetValue(2, rowIndex, DataObject.String("Estonia"));

			tableInfo = new DataTableInfo("APP", "devices");
			tableInfo.AddColumn("id", PrimitiveTypes.Numeric());
			tableInfo.AddColumn("person_id", PrimitiveTypes.Numeric());
			tableInfo.AddColumn("device_name", PrimitiveTypes.String());
			tableInfo.AddColumn("os", PrimitiveTypes.String());
			tableInfo.AddColumn("date", PrimitiveTypes.Date());
			table = database.CreateTable(tableInfo);

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(0));
			table.SetValue(1, rowIndex, DataObject.Number(0));
			table.SetValue(2, rowIndex, DataObject.String("Work Notebook"));
			table.SetValue(3, rowIndex, DataObject.String("Windows 8.1"));
			table.SetValue(4, rowIndex, DataObject.Now());

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(1));
			table.SetValue(1, rowIndex, DataObject.Number(0));
			table.SetValue(2, rowIndex, DataObject.String("Tablet"));
			table.SetValue(3, rowIndex, DataObject.String("Android 4.4"));
			table.SetValue(4, rowIndex, DataObject.Now());

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(2));
			table.SetValue(1, rowIndex, DataObject.Number(0));
			table.SetValue(2, rowIndex, DataObject.String("Other Notebook"));
			table.SetValue(3, rowIndex, DataObject.String("Ubuntu Linux"));
			table.SetValue(4, rowIndex, DataObject.Now());

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(3));
			table.SetValue(1, rowIndex, DataObject.Number(1));
			table.SetValue(2, rowIndex, DataObject.String("Mac Work Notebook"));
			table.SetValue(3, rowIndex, DataObject.String("Mac OS X"));
			table.SetValue(4, rowIndex, DataObject.Now());

			rowIndex = table.NewRow();
			table.SetValue(0, rowIndex, DataObject.Number(4));
			table.SetValue(1, rowIndex, DataObject.Number(1));
			table.SetValue(2, rowIndex, DataObject.String("Tablet"));
			table.SetValue(3, rowIndex, DataObject.String("Android 4.2"));
			table.SetValue(4, rowIndex, DataObject.Now());
		}

		private IDatabaseConnection CreateConnection() {
			return database.CreateConnection("APP", null, null);
		}
	}
}