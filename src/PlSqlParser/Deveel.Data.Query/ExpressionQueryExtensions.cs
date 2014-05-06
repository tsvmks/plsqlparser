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

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Query {
	static class ExpressionQueryExtensions {
		public static IQueryPlanNode AsQueryPlanNode(this Expression expression) {
			if (expression is ConstantExpression) {
				var constant = (ConstantExpression) expression;
				if (constant.Value == null ||
				    constant.Value.IsNull)
					return null;

				if (constant.Value.DataType is QueryType)
					return (IQueryPlanNode) constant.Value.Value;
			}

			return null;
		}

		public static ObjectName AsVariable(this Expression expression) {
			if (expression is ConstantExpression) {
				var constant = (ConstantExpression)expression;
				if (constant.Value == null ||
					constant.Value.IsNull)
					return null;

				return ObjectName.Parse(constant.Value.ToStringValue());
			} else if (expression is VariableExpression) {
				var variable = (VariableExpression) expression;
				return variable.VariableName;
			}

			return null;
		}
	}
}