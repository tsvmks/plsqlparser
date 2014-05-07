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
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Expressions {
	[Serializable]
	public sealed class LikeExpression : BinaryExpression {
		public LikeExpression(Expression left, Expression right) 
			: this(left, right, null) {
		}

		public LikeExpression(Expression left, Expression right, Expression escape) 
			: base(left, right) {
			Escape = escape;
		}

		public Expression Escape { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Like; }
		}

		protected override DataObject EvaluateBinary(DataObject ob1, DataObject ob2, IEvaluateContext context) {
			if (ob1.IsNull)
				return ob1;
			if (ob2.IsNull)
				return ob2;

			char cEscape = '\\';
			if (Escape is ConstantExpression) {
				// TODO: some more checks...
				var escapeValue = ((ConstantExpression) Escape).Value;
				cEscape = escapeValue.ToString()[0];
			}

			string val = ob1.CastTo(PrimitiveTypes.String()).ToStringValue();
			string pattern = ob2.CastTo(PrimitiveTypes.String()).ToStringValue();

			return DataObject.Boolean(PatternSearch.FullPatternMatch(pattern, val, cEscape));
		}
	}
}