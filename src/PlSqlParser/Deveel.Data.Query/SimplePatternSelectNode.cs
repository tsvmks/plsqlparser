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
	/// The node for evaluating a simple pattern search on a table which
	/// includes a single left hand variable or constant, a pattern type 
	/// (<i>LIKE</i>, <i>NOT LIKE</i> or <i>REGEXP</i>), and a right hand 
	/// constant (eg. <c>T__y</c>).
	/// </summary>
	/// <remarks>
	/// If the expression is not in the form described above then this 
	/// node will not operate correctly.
	/// </remarks>
	[Serializable]
	public class SimplePatternSelectNode : SingleQueryPlanNode {
		/// <summary>
		/// The search expression.
		/// </summary>
		private readonly Expression expression;

		public SimplePatternSelectNode(IQueryPlanNode child, Expression expression)
			: base(child) {
			this.expression = expression;
		}

		public override ITable Evaluate(IQueryContext context) {
			// Evaluate the child
			ITable t = Child.Evaluate(context);

			var binary = (BinaryExpression) expression;

			// Perform the pattern search expression on the table.			
			ObjectName lhsVar = binary.First.AsVariable();
			if (lhsVar != null) {
				// LHS is a simple variable so do a simple select
				Operator op = binary.Operator;
				return t.SimpleSelect(context, lhsVar, op, binary.Second);
			}

			// LHS must be a constant so we can just evaluate the expression
			// and see if we get true, false, null, etc.
			DataObject v = expression.Evaluate(context);

			// If it evaluates to NULL or FALSE then return an empty set
			if (v.IsNull || v.Value.Equals(false))
				return t.EmptySelect();

			return t;
		}

		public override IList<ObjectName> DiscoverTableNames(IList<ObjectName> list) {
			return expression.DiscoverTableNames(base.DiscoverTableNames(list));
		}

		public override IList<CorrelatedVariable> DiscoverCorrelatedVariables(int level, IList<CorrelatedVariable> list) {
			return expression.DiscoverCorrelatedVariables(ref level,
					 base.DiscoverCorrelatedVariables(level, list));
		}

		public override string Name {
			get { return "PATTERN: " + expression; }
		}
	}
}