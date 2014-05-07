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

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class ConditionalExpression : Expression {
		public ConditionalExpression(Expression test, Expression ifTrue) 
			: this(test, ifTrue, null) {
		}

		public ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse) {
			IfFalse = ifFalse;
			IfTrue = ifTrue;
			Test = test;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Conditional; }
		}

		public Expression Test { get; private set; }

		public Expression IfTrue { get; private set; }

		public Expression IfFalse { get; set; }

		protected override void WriteTo(ISqlWriter writer) {
			writer.Write("CASE ");
			writer.Write(IfTrue);
			writer.Write(" WHEN ");
			writer.Write(Test);

			if (IfFalse != null) {
				writer.Write(" THEN ");
				writer.Write(IfFalse);
			}

			writer.Write(" END");
		}

		protected override DataObject OnEvaluate(IExpressionEvaluator evaluator) {
			var result = evaluator.Evaluate(Test).ToBoolean();
			if (result == true)
				return evaluator.Evaluate(IfTrue);
			if (IfFalse != null)
				return evaluator.Evaluate(IfFalse);

			throw new InvalidOperationException();
		}
	}
}