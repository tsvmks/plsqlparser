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
	public sealed class SubsetExpression : UnaryExpression {
		public SubsetExpression(Expression child)
			: base(child) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Subset; }
		}

		protected override DataObject EvaluateUnary(DataObject obj, IEvaluateContext context) {
			return Operand.Evaluate(context.GroupResolver, context.VariableResolver, context.QueryContext);
		}

		protected override void WriteTo(ISqlWriter writer) {
			writer.Write("(");
			writer.Write(Operand);
			writer.Write(")");
		}
	}
}