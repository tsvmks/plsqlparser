﻿// 
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
using System.Collections.ObjectModel;
using System.Linq;

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	public static class AggregateExplorer {
		public static bool HasAggregateFunction(this Expression expression, IQueryContext context) {
			var explorer = new Explorer(context);
			return explorer.Explore(expression);
		}

		#region Explorer

		class Explorer : ExpressionVisitor {
			private bool aggregateFound;
			private readonly IQueryContext queryContext;

			public Explorer(IQueryContext queryContext) {
				this.queryContext = queryContext;
			}

			public bool Explore(Expression expression) {
				Visit(expression);
				return aggregateFound;
			}

			protected override Expression VisitMethodCall(FunctionCallExpression expression) {
				if (expression.IsAggregate(queryContext))
					aggregateFound = true;

				return base.VisitMethodCall(expression);
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				var value = expression.Value;
				if (value.DataType is ArrayType) {
					var exps = (IEnumerable<Expression>) value.Value;
					VisitExpressionList(new ReadOnlyCollection<Expression>(exps.ToList()));
				}

				return base.VisitConstant(expression);
			}
		}

		#endregion
	}
}