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
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public abstract class BinaryExpression : Expression {
		public Expression First { get; private set; }

		public Expression Second { get; private set; }

		protected BinaryExpression(Expression first, Expression second) {
			First = first;
			Second = second;
		}

		internal virtual Operator Operator {
			get { return ExpressionType.AsOperator(); }
		}

		internal abstract DataObject Evaluate(DataObject ob1,
			DataObject ob2,
			IGroupResolver group,
			IVariableResolver resolver,
			IQueryContext context);
	}
}