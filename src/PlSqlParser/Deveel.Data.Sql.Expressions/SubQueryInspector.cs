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

using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	public static class SubQueryInspector {
		public static bool HasSubQuery(this Expression expression) {
			var visitor = new Visitor();
			return visitor.Inspect(expression);
		}

		class Visitor : ExpressionVisitor {
			private bool hasSubQuery = false;

			public bool Inspect(Expression expression) {
				Visit(expression);
				return hasSubQuery;
			}

			protected override Expression VisitSubQuery(SubQueryExpression expression) {
				hasSubQuery = true;
				return base.VisitSubQuery(expression);
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				var value = expression.Value;
				if (value.DataType is QueryType)
					hasSubQuery = true;

				return base.VisitConstant(expression);
			}
		}
	}
}