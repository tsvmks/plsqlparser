using System;

using Deveel.Data.Types;

using NUnit.Framework;

namespace Deveel.Data.Sql {
	[TestFixture]
	public sealed class DataTypeTests {
		[Test]
		public void PrimitiveInteger() {
			var dataType = DataType.Parse("INTEGER(22)");
			Assert.IsNotNull(dataType);
			Assert.AreEqual(SqlType.Integer, dataType.SqlType);
			Assert.AreEqual("NUMERIC", dataType.Name);
			Assert.IsTrue(dataType.IsPrimitive);

			Assert.IsInstanceOf<NumericType>(dataType);
			var numDataType = (NumericType) dataType;
			Assert.AreEqual(22, numDataType.Size);
			Assert.AreEqual("NUMERIC(22)", numDataType.ToString());

			dataType = DataType.Parse("INTEGER(64, 2)");
			Assert.AreEqual(SqlType.Integer, dataType.SqlType);
			Assert.AreEqual("NUMERIC", dataType.Name);
			Assert.IsTrue(dataType.IsPrimitive);

			Assert.IsInstanceOf<NumericType>(dataType);
			numDataType = (NumericType)dataType;
			Assert.AreEqual(64, numDataType.Size);
			Assert.AreEqual(2, numDataType.Scale);
			Assert.AreEqual("NUMERIC(64,2)", numDataType.ToString());
		}

		[Test]
		public void RowType() {
			var dataType = DataType.Parse("Person%ROWTYPE");
			Assert.IsNotNull(dataType);
			Assert.AreEqual(SqlType.RowType, dataType.SqlType);
			Assert.AreEqual("Person%ROWTYPE", dataType.Name);
			Assert.IsFalse(dataType.IsPrimitive);
			Assert.IsInstanceOf<RowType>(dataType);

			var rowType = (RowType) dataType;
			Assert.AreEqual("Person", rowType.TableName.ToString());
		}

		[Test]
		public void ColumnType() {
			var dataType = DataType.Parse("Person.Name%TYPE");
			Assert.IsNotNull(dataType);
			Assert.AreEqual(SqlType.ColumnType, dataType.SqlType);
			Assert.AreEqual("Person.Name%TYPE", dataType.Name);
			Assert.IsFalse(dataType.IsPrimitive);
			Assert.IsInstanceOf<ColumnType>(dataType);

			var rowType = (ColumnType)dataType;
			Assert.AreEqual("Person.Name", rowType.ColumnName.ToString());
		}
	}
}