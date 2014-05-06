// 
//  Copyright 2010  Deveel
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

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query {
	/// <summary>
	/// A branch node for equi-joining two tables together given two 
	/// sets of columns.
	/// </summary>
	/// <remarks>
	/// This is a seperate node from a general join operation to allow
	/// for optimizations with multi-column indexes.
	/// <para>
	/// An equi-join is the most common type of join.
	/// </para>
	/// <para>
	/// At query runtime, this decides the best best way to perform the 
	/// join, either by
	/// </para>
	/// </remarks>
	[Serializable]
	public class EquiJoinNode : BranchQueryPlanNode {
		/// <summary>
		/// The columns in the left table.
		/// </summary>
		private readonly ObjectName[] leftColumns;

		/// <summary>
		/// The columns in the right table.
		/// </summary>
		private readonly ObjectName[] rightColumns;

		public EquiJoinNode(IQueryPlanNode left, IQueryPlanNode right, ObjectName[] leftColumns, ObjectName[] rightColumns)
			: base(left, right) {
			this.leftColumns = leftColumns;
			this.rightColumns = rightColumns;
		}

		public override ITable Evaluate(IQueryContext context) {
			// Solve the left branch result
			ITable leftResult = Left.Evaluate(context);
			// Solve the right branch result
			ITable rightResult = Right.Evaluate(context);

			// TODO: This needs to migrate to a better implementation that
			//   exploits multi-column indexes if one is defined that can be used.

			ObjectName firstLeft = leftColumns[0];
			ObjectName firstRight = rightColumns[0];

			ITable result = leftResult.SimpleJoin(context, rightResult, firstLeft, Operator.Equal, Expression.Variable(firstRight));

			int sz = leftColumns.Length;
			// If there are columns left to equi-join, we resolve the rest with a
			// single exhaustive select of the form,
			//   ( table1.col2 = table2.col2 AND table1.col3 = table2.col3 AND ... )
			if (sz > 1) {
				// Form the expression
				Expression restExpression = null;
				for (int i = 1; i < sz; ++i) {
					ObjectName leftVar = leftColumns[i];
					ObjectName rightVar = rightColumns[i];
					var equalExp = Expression.Equal(Expression.Variable(leftVar), Expression.Variable(rightVar));
					if (restExpression == null) {
						restExpression = equalExp;
					} else {
						restExpression = Expression.And(restExpression, equalExp);
					}
				}

				result = result.ExhaustiveSelect(context, restExpression);
			}

			return result;
		}
	}
}