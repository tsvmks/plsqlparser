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
	public enum ExpressionType {
		Constant,

		// Operators

		// Multiplicative
		Add,
		Concat,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Exponent,

		// Relational
		Equal,
		NotEqual,
		Greater,
		GreaterOrEqual,
		Smaller,
		SmallerOrEqual,

		Is,
		Like,

		// Sub-sets
		Any,
		All,

		// Logical
		And,
		Or,

		// Unary 
		Not,
		Negative,
		Positive,

		Conditional,
		Call,

		// Variables
		Variable,
		CorrelatedVariable,
		VariableRef,

		Subset,
		Query,
		Assign,
		Column
	}
}