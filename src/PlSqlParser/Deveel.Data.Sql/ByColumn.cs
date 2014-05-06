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
using System.Text;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql {
	[Serializable]
	public sealed class ByColumn : IPreparable {
		public ByColumn(Expression exp, bool ascending) {
			Expression = exp;
			Ascending = ascending;
		}

		public ByColumn(Expression exp)
			: this(exp, true) {
		}

		public Expression Expression { get; private set; }

		public bool Ascending { get; private set; }

		internal void DumpSqlTo(StringBuilder builder) {
			Expression.DumpTo(builder);

			builder.Append(" ");
			builder.Append(Ascending ? "ASC" : "DESC");
		}

		public ByColumn Prepare(IExpressionPreparer preparer) {
			var exp = Expression;
			if (exp != null)
				exp = exp.Prepare(preparer);
			return new ByColumn(exp, Ascending);
		}

		object IPreparable.Prepare(IExpressionPreparer preparer) {
			return Prepare(preparer);
		}
	}
}