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
using System.Diagnostics;
using System.Text;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	[DebuggerDisplay("{ToString(), nq}")]
	public sealed class TableSelectExpression {
		private Expression whereClause;
		private bool whereSet;

		public TableSelectExpression() {
			Columns = new List<SelectColumn>();
			CompositeFunction = CompositeFunction.None;
			Into = new SelectIntoClause();
			From = new FromClause();
			GroupBy = new List<ByColumn>();
		}

		public FromClause From { get; private set; }

		public SelectIntoClause Into { get; private set; }

		public Expression Where {
			get { return whereClause; }
			set {
				whereClause = value;
				whereSet = true;
			}
		}

		public bool Distinct { get; set; }

		public ICollection<ByColumn> GroupBy { get; private set; }

		public string GroupMax { get; set; }

		public Expression Having { get; set; }

		public bool IsCompositeAll { get; private set; }

		public CompositeFunction CompositeFunction { get; private set; }

		public TableSelectExpression NextComposite { get; private set; }

		public ICollection<SelectColumn> Columns { get; private set; }

		public void ChainComposite(TableSelectExpression expression, CompositeFunction composite, bool isAll) {
			NextComposite = expression;
			CompositeFunction = composite;
			IsCompositeAll = isAll;
		}

		internal void DumpSqlTo(StringBuilder builder) {
			builder.Append("SELECT ");
			if (Into != null) {
				// TODO:
			}

			var colCount = Columns.Count;
			int i = -1;
			foreach (var column in Columns) {
				column.DumpSqlTo(builder);

				if (++i < colCount - 1)
					builder.Append(", ");
			}

			if (colCount > 0)
				builder.Append(" ");

			if (From != null) {
				From.DumpSqlTo(builder);
			}
		}
	}
}