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

			public Evaluator() {
				elements = new List<object>();
			}

			private object ElementToObject(int index) {
				object ob = elements[index];
				if (ob is DataObject || ob is Operator) {
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
				if (elementCount == 3) {
					var o1 = (DataObject)ElementToObject(0);
					var o2 = (DataObject)ElementToObject(1);
					var op = (Operator)elements[2];
					return op.Evaluate(o1, o2, group, resolver, context);
				}

				if (evalStack == null)
					evalStack = new Stack<object>();

				for (int n = 0; n < elementCount; ++n) {
					object val = ElementToObject(n);
					if (val is Operator) {
						Operator op = (Operator)val;

						DataObject v2 = (DataObject)evalStack.Pop();
						DataObject v1 = (DataObject)evalStack.Pop();

						evalStack.Push(op.Evaluate(v1, v2, group, resolver, context));
					} else {
						evalStack.Push(val);
					}
				}
				// We should end with a single value on the stack.
				return (DataObject)evalStack.Pop();
			}

			protected override Expression VisitBinary(BinaryExpression expression) {
				elements.Add(expression.First.Evaluate(group, resolver, context));
				elements.Add(expression.Second.Evaluate(group, resolver, context));
				elements.Add(expression.Operator);
				return expression;
			}

			protected override Expression VisitUnary(UnaryExpression expression) {
				elements.Add(expression.Operand);
				return expression;
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				elements.Add(expression.Value);
				return expression;
			}

			protected override Expression VisitMethodCall(FunctionCallExpression expression) {
				return base.VisitMethodCall(expression);
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