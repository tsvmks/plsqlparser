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

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query {
	/// <summary>
	/// The node for performing a simple select operation on a table.
	/// </summary>
	/// <remarks>
	/// The simple select requires a LHS variable, an operator, and an expression 
	/// representing the RHS.
	/// </remarks>
	[Serializable]
	public class SimpleSelectNode : SingleQueryPlanNode {
		/// <summary>
		/// The LHS variable.
		/// </summary>
		private ObjectName leftVar;

		/// <summary>
		/// The operator to select under (=, &lt;&gt;, &gt;, &lt;, &gt;=, &lt;=).
		/// </summary>
		private Operator op;

		/// <summary>
		/// The RHS expression.
		/// </summary>
		private Expression rightExpression;

		public SimpleSelectNode(IQueryPlanNode child, ObjectName leftVar, Operator op, Expression rightExpression)
			: base(child) {
			this.leftVar = leftVar;
			this.op = op;
			this.rightExpression = rightExpression;
		}

		public override ITable Evaluate(IQueryContext context) {
			// Solve the child branch result
			ITable table = Child.Evaluate(context);

			// The select operation.
			return table.SimpleSelect(context, leftVar, op, rightExpression);
		}

		public override IList<ObjectName> DiscoverTableNames(IList<ObjectName> list) {
			return rightExpression.DiscoverTableNames(base.DiscoverTableNames(list));
		}

		public override IList<CorrelatedVariable> DiscoverCorrelatedVariables(int level, IList<CorrelatedVariable> list) {
			return rightExpression.DiscoverCorrelatedVariables(ref level, base.DiscoverCorrelatedVariables(level, list));
		}

		public override string Name {
			get { return "SIMPLE: " + leftVar + op + rightExpression; }
		}
	}
}