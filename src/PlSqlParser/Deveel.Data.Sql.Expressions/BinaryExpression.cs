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
using System.Linq;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Sql.Expressions {
	public abstract class BinaryExpression : Expression {
		public Expression Left { get; private set; }

		public Expression Right { get; private set; }

		protected BinaryExpression(Expression left, Expression right) {
			Left = left;
			Right = right;
		}

		internal virtual Operator Operator {
			get { return ExpressionType.AsOperator(); }
		}

		protected abstract DataObject EvaluateBinary(DataObject ob1, DataObject ob2, IEvaluateContext context);

		protected override void WriteTo(ISqlWriter writer) {
			writer.Write(Left);
			writer.Write(" {0} ", Operator.AsString());
			writer.Write(Right);
		}

		protected override DataObject OnEvaluate(IExpressionEvaluator evaluator) {
			var sortedEval = new[] {
				new SortedEvalInfo(0, Left),
				new SortedEvalInfo(1, Right)
			}
				.OrderByDescending(x => x.Precedence)
				.ToArray();

			foreach (var evalInfo in sortedEval) {
				evalInfo.Result = evaluator.Evaluate(evalInfo.Expression);
			}

			var results = sortedEval
				.OrderBy(x => x.Offset)
				.Select(x => x.Result)
				.ToArray();

			return EvaluateBinary(results[0], results[1], evaluator.Context);
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
	}
}