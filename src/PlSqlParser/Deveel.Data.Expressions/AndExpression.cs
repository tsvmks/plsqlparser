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

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	[Serializable]
	public sealed class AndExpression : BinaryExpression {
		public AndExpression(Expression first, Expression second) 
			: base(first, second) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.And; }
		}

		internal override DataObject Evaluate(DataObject ob1, DataObject ob2, IGroupResolver @group, IVariableResolver resolver, IQueryContext context) {
			bool? b1 = ob1.ToBoolean();
			bool? b2 = ob2.ToBoolean();

			// If either ob1 or ob2 are null
			if (!b1.HasValue)
				return b2.HasValue && b2.Equals(false) ? DataObject.BooleanFalse : DataObject.BooleanNull;
			if (!b2.HasValue)
				return b1.Equals(false) ? DataObject.BooleanFalse : DataObject.BooleanNull;

			// If both true.
			return DataObject.Boolean(b1.Equals(true) && b2.Equals(true));
		}
	}
}