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
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class QueryExpression : Expression {
		public QueryExpression(TableSelectExpression select) {
			if (select == null)
				throw new ArgumentNullException("select");

			SelectExpression = select;
		}

		public TableSelectExpression SelectExpression { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Query; }
		}

		protected override DataObject OnEvaluate(IExpressionEvaluator evaluator) {
			// Generate the TableExpressionFromSet hierarchy for the expression,
			TableExpressionFromSet fromSet = Planner.GenerateFromSet(SelectExpression, evaluator.Context.QueryContext.Connection);

			// Form the plan
			IQueryPlanNode plan = Planner.FormQueryPlan(evaluator.Context.QueryContext.Connection, SelectExpression, fromSet, new List<ByColumn>());

			return new DataObject(PrimitiveTypes.Query(), plan);
		}
	}
}