// 
//  Copyright 2014  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
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