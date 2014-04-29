using System;
using System.Collections.Generic;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class SelectIntoClause {
		public SelectIntoClause() {
			elements = new List<object>();
		}

		private string tableName;
		private readonly List<object> elements;

		internal bool HasElements {
			get { return elements.Count > 0; }
		}

		public object this[int index] {
			get { return elements[index]; }
		}

		internal bool HasTableName {
			get { return !String.IsNullOrEmpty(tableName); }
		}

		public string Table {
			get { return tableName; }
		}

		internal void SetTableName(string value) {
			if (!string.IsNullOrEmpty(tableName))
				throw new ArgumentException("Cannot set more than one destination table.");

			tableName = value;
		}

		public void AddElement(object value) {
			if (value == null)
				throw new ArgumentNullException("value");

			if (!(value is string))
				throw new ArgumentException("Unable to set the object as target of an INTO clause.");
			
			elements.Add(value);
		}

		public SelectIntoClause Prepare(IExpressionPreparer preparer) {
			throw new NotImplementedException();
		}
	}
}