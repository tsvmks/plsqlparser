using System;

namespace Deveel.Data.Expressions {
	public enum ExpressionType {
		Constant,

		// Operators
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Exponent,

		Equals,
		NotEquals,
		Greater,
		GreaterOrEqual,
		Smaller,
		SmallerOrEqual,

		Any,
		All,

		// Logical
		And,
		Or,

		Not,
		Negate,
		Is,

		Conditional,
		Call,
		Cast,
		Variable,

		Subset,
		Query,
	}
}