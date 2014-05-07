﻿// 
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

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class AllExpression : BinaryExpression {
		public AllExpression(Expression left, Expression right) 
			: base(left, right) {
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.All; }
		}

		protected override DataObject EvaluateBinary(DataObject ob1, DataObject ob2, IEvaluateContext context) {
			throw new NotImplementedException();
		}
	}
}