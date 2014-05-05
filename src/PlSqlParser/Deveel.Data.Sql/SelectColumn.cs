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

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql {
	public sealed class SelectColumn {
		public SelectColumn(Expression expression, ObjectName alias) {
			Alias = alias;
			Expression = expression;
		}

		public SelectColumn(Expression expression)
			: this(expression, null) {
		}

		public Expression Expression { get; private set; }

		public bool IsGlob {
			get {
				var exp = Expression as ConstantExpression;
				if (exp == null)
					return false;

				var s = exp.Value.ToStringValue();
				return s == "*" || s.EndsWith(".*");
			}
		}

		public bool IsAll {
			get {
				var exp = Expression as ConstantExpression;
				if (exp == null)
					return false;

				var s = exp.Value.ToStringValue();
				return s == "*";
			}
		}

		public string GlobPrefix {
			get {
				var exp = Expression as ConstantExpression;
				if (exp == null)
					return null;

				var s = exp.Value.ToStringValue();
				var index = s.LastIndexOf(".*", StringComparison.InvariantCultureIgnoreCase);
				if (index == -1)
					return null;

				return s.Substring(0, index);
			}
		}

		public ObjectName Alias { get; private set; }

		internal ObjectName InternalName { get; set; }

		internal void DumpSqlTo(StringBuilder builder) {
			Expression.DumpTo(builder);

			if (Alias != null) {
				builder.Append(" AS ");
				builder.Append(Alias);
			}
		}

		public SelectColumn Prepare(IExpressionPreparer preparer) {
			return new SelectColumn(Expression.Prepare(preparer), Alias);
		}
	}
}