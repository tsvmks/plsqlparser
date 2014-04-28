using System;

namespace Deveel.Data.Expressions {
	public enum ExpressionType {
		Constant,

		// Operators
		Add,
		Concat,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Exponent,

		Equal,
		NotEqual,
		Greater,
		GreaterOrEqual,
		Smaller,
		SmallerOrEqual,

		Like,

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
		Bind,
		Assign,
	}
}