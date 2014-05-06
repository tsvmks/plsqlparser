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
using System.Linq;

using Deveel.Data.DbSystem;
using Deveel.Data.Query;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	public static class ExpressionEvaluator {
		public static DataObject Evaluate(this Expression expression, IQueryContext context) {
			return Evaluate(expression, null, context);
		}

		public static DataObject Evaluate(this Expression expression, IVariableResolver resolver, IQueryContext context) {
			return Evaluate(expression, null, resolver, context);
		}

		public static DataObject Evaluate(this Expression expression, IGroupResolver group, IVariableResolver resolver, IQueryContext context) {
			var visitor = new Evaluator();
			return visitor.EvaluateExpression(expression, group, resolver, context);
		}

		public static DataObject Evaluate(this Expression expression) {
			return expression.Evaluate(null, null, null);
		}

		#region Evaluator

		class Evaluator : ExpressionVisitor {
			private Stack<object> evalStack;
			private readonly List<object> elements;

			private IGroupResolver group;
			private IVariableResolver resolver;
			private IQueryContext context;

			private delegate DataObject BinaryExpressionEvaluate(
				DataObject a,
				DataObject b,
				IGroupResolver group,
				IVariableResolver resolver,
				IQueryContext context);

			private delegate DataObject UnaryExpressionEvaluate(
				DataObject obj,
				IGroupResolver group,
				IVariableResolver resolver,
				IQueryContext context);

			public Evaluator() {
				elements = new List<object>();
			}

			private object ElementToObject(int index) {
				object ob = elements[index];
				if (ob is DataObject ||
				    ob is BinaryExpressionEvaluate ||
				    ob is UnaryExpressionEvaluate) {
					return ob;
				}

				if (ob is ObjectName)
					return resolver.Resolve((ObjectName) ob);
				if (ob is CorrelatedVariable)
					return ((CorrelatedVariable) ob).EvalResult;
/*
TODO:
if (ob is RoutineInvoke) {
IFunction fun = (IFunction)((RoutineInvoke)ob).GetFunction(context);
return fun.Execute(((RoutineInvoke)ob), group, resolver, context);
}
*/
				if (ob is TableSelectExpression) {
					TableSelectExpression selectExpression = (TableSelectExpression)ob;

					// Generate the TableExpressionFromSet hierarchy for the expression,
					TableExpressionFromSet fromSet = Planner.GenerateFromSet(selectExpression, context.Connection);

					// Form the plan
					IQueryPlanNode plan = Planner.FormQueryPlan(context.Connection, selectExpression, fromSet, new List<ByColumn>());

					return new DataObject(PrimitiveTypes.Query(), plan);
				}

				if (ob == null)
					throw new NullReferenceException("Null element in expression");

				throw new ApplicationException("Unknown element type: " + ob.GetType());
			}

			private DataObject DoEvaluate() {
				// Optimization - trivial case of 'a' or 'ab*' postfix are tested for
				//   here.
				int elementCount = elements.Count;
				if (elementCount == 1)
					return (DataObject)ElementToObject(0);
				if (elementCount == 2) {
					//TODO: experimenting unaries...
					var obj = (DataObject) ElementToObject(0);
					var op = (UnaryExpressionEvaluate) elements[1];
					return op(obj, group, resolver, context);
				}
				if (elementCount == 3) {
					var o1 = (DataObject)ElementToObject(0);
					var o2 = (DataObject)ElementToObject(1);
					var op = (BinaryExpressionEvaluate)elements[2];
					return op(o1, o2, group, resolver, context);
				}

				if (evalStack == null)
					evalStack = new Stack<object>();

				for (int n = 0; n < elementCount; ++n) {
					object val = ElementToObject(n);
					if (val is BinaryExpressionEvaluate) {
						var op = (BinaryExpressionEvaluate)val;

						var v2 = (DataObject)evalStack.Pop();
						var v1 = (DataObject)evalStack.Pop();

						evalStack.Push(op(v1, v2, group, resolver, context));
					} else {
						evalStack.Push(val);
					}
				}
				// We should end with a single value on the stack.
				return (DataObject)evalStack.Pop();
			}

			protected override Expression VisitBinary(BinaryExpression expression) {
				var sortedEval = new[] {
					new SortedEvalInfo(0, expression.First),
					new SortedEvalInfo(1, expression.Second)
				}
					.OrderByDescending(x => x.Precedence)
					.ToArray();

				foreach (var evalInfo in sortedEval) {
					evalInfo.Result = evalInfo.Expression.Evaluate(group, resolver, context);
				}

				var results = sortedEval
					.OrderBy(x => x.Offset)
					.Select(x => x.Result)
					.ToArray();

				elements.Add(results[0]);
				elements.Add(results[1]);
				elements.Add(new BinaryExpressionEvaluate(expression.Evaluate));
				return expression;
			}

			private class SortedEvalInfo {
				public SortedEvalInfo(int offset, Expression expression) {
					Offset = offset;
					Expression = expression;
				}

				public int Offset { get; private set; }
				public DataObject Result { get; set; }
				public Expression Expression { get; private set; }

				public int Precedence {
					get { return Expression.Precedence; }
				}
			}

			protected override Expression VisitUnary(UnaryExpression expression) {
				elements.Add(expression.Operand.Evaluate(group, resolver, context));
				elements.Add(new UnaryExpressionEvaluate(expression.Evaluate));
				return expression;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				elements.Add(expression.Value);
				return expression;
			}

			protected override Expression VisitVariable(VariableExpression expression) {
				elements.Add(expression.VariableName);
				return expression;
			}

			protected override Expression VisitSubQuery(SubQueryExpression expression) {
				elements.Add(expression.SelectExpression);
				return base.VisitSubQuery(expression);
			}

			protected override Expression VisitCorrelatedVariable(CorrelatedVariableExpression expression) {
				elements.Add(expression.CorrelatedVariable);
				return base.VisitCorrelatedVariable(expression);
			}

			protected override Expression VisitMethodCall(FunctionCallExpression expression) {
				return expression;
			}

			public DataObject EvaluateExpression(Expression expression, IGroupResolver group, IVariableResolver resolver, IQueryContext context) {
				this.group = group;
				this.resolver = resolver;
				this.context = context;

				// Collect all elements to evaluate
				Visit(expression);

				// Finally evaluate
				return DoEvaluate();
			}
		}

		#endregion
	}
}