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
	[Flags]
	public enum Operator {
		Unknown = 0,

		// Multiplicative
		Add = 1,
		Concat,			// TODO: Shift all op codes
		Subtract = 2,
		Multiply = 4,
		Divide = 8,
		Modulo = 16,

		// Relational
		Equal = 32,
		NotEqual = 64,
		Like = 128,
		NotLike = 256,
		Is = 512,
		IsNot = 1024,
		Smaller = 2048,
		SmallerOrEqual = 4096,
		Greater = 8192,
		GreaterOrEqual = 16384,

		// Logical
		And = 32768,
		Or = 65536,

		All = 131072,
		Any = 262144,

		AllEqual = All | Equal,
		AllNotEqual = All | NotEqual,
		AllLike = All | Like,
		AllNotLike = All | NotLike,
		AllIs = All | Is,
		AllIsNot = All | IsNot,
		AllSmaller = All | Smaller,
		AllSmallerOrEqual = All | SmallerOrEqual,
		AllGreater = All | Greater,
		AllGreaterOrEqual = All | GreaterOrEqual,
		
		AnyEqual = Any | Equal,
		AnyNotEqual = Any | NotEqual,
		AnyLike = Any | Like,
		AnyNotLike = Any | NotLike,
		AnyIs = Any | Is,
		AnyIsNot = Any | IsNot,
		AnySmaller = Any | Smaller,
		AnySmallerOrEqual = Any | SmallerOrEqual,
		AnyGreater = Any | Greater,
		AnyGreaterOrEqual = Any | GreaterOrEqual,
	}
}
