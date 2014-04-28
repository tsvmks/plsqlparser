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

using Deveel.Data.Query;
using Deveel.Data.Types;

namespace Deveel.Data.Expressions {
	static class TableNameExplorer {
		public static IList<ObjectName> DiscoverTableNames(Expression expression, IList<ObjectName> tableNames) {
			var visitor = new TableNameVisitor(tableNames);
			Expression.Visit(expression, visitor);
			return visitor.TableNames;
		}

		#region TableNameVisitor

		class TableNameVisitor : ExpressionVisitor {
			public IList<ObjectName> TableNames { get; private set; }

			public TableNameVisitor(IList<ObjectName> tableNames) {
				TableNames = tableNames;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				if (expression.Value.DataType is QueryType) {
					TableNames = ((IQueryPlanNode)expression.Value.Value).DiscoverTableNames(TableNames);
				}

				return expression;
			}
		}

		#endregion
	}
}