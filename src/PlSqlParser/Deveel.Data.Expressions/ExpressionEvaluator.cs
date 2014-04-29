using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.DbSystem;
using Deveel.Data.Query;
using Deveel.Data.Sql;
using Deveel.Data.Types;

namespace Deveel.Data.Expressions {
	public static class ExpressionEvaluator {
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

				if (ob is VariableBind)
					return resolver.Resolve((VariableBind)ob);
				/*
TODO:
if (ob is CorrelatedVariable)
	return ((CorrelatedVariable)ob).EvalResult;
if (ob is RoutineInvoke) {
	IFunction fun = (IFunction)((RoutineInvoke)ob).GetFunction(context);
	return fun.Execute(((RoutineInvoke)ob), group, resolver, context);
}
*/
				if (ob is TableSelectExpression) {
					// TODO:
					//TableSelectExpression selectExpression = (TableSelectExpression)ob;

					//// Generate the TableExpressionFromSet hierarchy for the expression,
					//TableExpressionFromSet from_set = Planner.GenerateFromSet(selectExpression, context.Connection);

					//// Form the plan
					//IQueryPlanNode plan = Planner.FormQueryPlan(context.Connection, selectExpression, from_set, new List<ByColumn>());

					IQueryPlanNode plan = null;
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

			protected override Expression VisitSubQuery(SubQueryExpression expression) {
				elements.Add(expression.SelectExpression);
				return base.VisitSubQuery(expression);
			}

			public DataObject EvaluateExpression(Expression expression, IGroupResolver group, IVariableResolver resolver, IQueryContext context) {
				this.group = group;
				this.resolver = resolver;
				this.context = context;

				// TODO: Order 

				// Collect all elements to evaluate
				Visit(expression);

				// Finally evaluate
				return DoEvaluate();
			}
		}

		#endregion
	}
}