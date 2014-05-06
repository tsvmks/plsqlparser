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
using System.Text;

using Deveel.Data.DbSystem;
using Deveel.Data.Query;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class AnyExpression : BinaryExpression {
		public AnyExpression(Expression left, ExpressionType subType, Expression right) 
			: base(left, right) {
			AssertSubTypeIsRelational(subType);
			SubType = subType;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Any; }
		}

		public ExpressionType SubType { get; private set; }

		internal override Operator Operator {
			get {
				if (SubType == ExpressionType.Equal)
					return Operator.AnyEqual;
				if (SubType == ExpressionType.NotEqual)
					return Operator.AnyNotEqual;
				if (SubType == ExpressionType.Greater)
					return Operator.AnyGreater;
				if (SubType == ExpressionType.GreaterOrEqual)
					return Operator.AnyGreaterOrEqual;
				if (SubType == ExpressionType.Smaller)
					return Operator.AnySmaller;
				if (SubType == ExpressionType.SmallerOrEqual)
					return Operator.AnySmallerOrEqual;

				throw new InvalidOperationException("Subtype not supported");
			}
		}

		private static void AssertSubTypeIsRelational(ExpressionType type) {
			if (type != ExpressionType.Equal &&
				type != ExpressionType.NotEqual &&
				type != ExpressionType.Greater &&
				type != ExpressionType.GreaterOrEqual &&
				type != ExpressionType.Smaller &&
				type != ExpressionType.SmallerOrEqual)
				throw new ArgumentException(String.Format("The expression sub-type {0} is not valid for ANY expression.", type));
		}

		private static bool IsTrue(DataObject b) {
			return (!b.IsNull &&
					b.DataType is BooleanType &&
					b.Value.Equals(true));
		}

		internal override DataObject Evaluate(DataObject ob1, DataObject ob2, IGroupResolver @group, IVariableResolver resolver, IQueryContext context) {
			var op = Operator;

			if (ob2.DataType is QueryType) {
				// The sub-query plan
				IQueryPlanNode plan = (IQueryPlanNode)ob2.Value;
				// Discover the correlated variables for this plan.
				IList<CorrelatedVariable> list = plan.DiscoverCorrelatedVariables(1, new List<CorrelatedVariable>());

				if (list.Count > 0) {
					// Set the correlated variables from the IVariableResolver
					foreach (CorrelatedVariable variable in list) {
						variable.SetFromResolver(resolver);
					}
					// Clear the cache in the context
					context.ClearCache();
				}

				// Evaluate the plan,
				ITable t = plan.Evaluate(context);

				// The ANY operation
				Operator revPlainOp = op.Plain().Reverse();
				return DataObject.Boolean(t.ColumnMatchesValue(0, revPlainOp, ob1));
			}

			if (ob2.DataType is ArrayType) {
				Operator plainOp = op.Plain();
				var expList = (IEnumerable<Expression>) ob2.Value;
				// Assume there are no matches
				DataObject retVal = DataObject.BooleanFalse;
				foreach (Expression exp in expList) {
					DataObject expItem = exp.Evaluate(group, resolver, context);
					// If null value, return null if there isn't otherwise a match found.
					if (expItem.IsNull) {
						retVal = DataObject.BooleanNull;
					} else {
						var opExp = Expression.Operator(ob1, plainOp, expItem);
						if (IsTrue(opExp.Evaluate())) {
							// If there is a match, the ANY set test is true
							return DataObject.BooleanTrue;
						}
					}
				}
				// No matches, so return either false or NULL.  If there are no matches
				// and no nulls, return false.  If there are no matches and there are
				// nulls present, return null.
				return retVal;
			}

			throw new ApplicationException("Unknown RHS of ANY.");
		}
	}
}