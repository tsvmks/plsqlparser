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

using Deveel.Data.Types;

namespace Deveel.Data.Expressions {
	public static class ConstantInspector {
		public static bool IsConstant(this Expression expression) {
			var visitor = new Visitor();
			return visitor.Inspect(expression);
		}

		class Visitor : ExpressionVisitor {
			private bool isConstant = true;

			public bool Inspect(Expression expression) {
				Visit(expression);
				return isConstant;
			}

			protected override Expression VisitVariable(VariableExpression expression) {
				isConstant = false;
				return base.VisitVariable(expression);
			}

			protected override Expression VisitSubQuery(SubQueryExpression expression) {
				isConstant = false;
				return base.VisitSubQuery(expression);
			}

			protected override Expression VisitConstant(ConstantExpression expression) {
				var value = expression.Value;
				if (!value.IsNull) {
					if (value.DataType is QueryType) {
						isConstant = false;
					} else if (value.DataType is ArrayType) {
						var array = (IEnumerable<Expression>) value.Value;
						if (array.Any(exp => !exp.IsConstant()))
							isConstant = false;
					}
				}

				return base.VisitConstant(expression);
			}
		}
	}
}