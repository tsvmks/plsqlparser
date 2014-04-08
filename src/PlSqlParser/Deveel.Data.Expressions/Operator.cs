using System;

namespace Deveel.Data.Expressions {
	[Flags]
	public enum Operator {
		Add = 1,
		Subtract = 2,
		Multiply = 4,
		Divide = 8,
		Modulo = 16,
		Like = 32,
		Equals = 64,
		Greater = 128,
		Smaller = 256,
		Concat = 512,
		Exponent = 4096,
		Is = 8192,
		Regex = 16384,

		Not = 32768,

		All = 65536,
		Any = 131072,

		And = 262144,
		Or = 524288,

		GreaterEquals = Greater | Equals,
		SmallerEquals = Smaller | Equals,

		NotEquals = Not | Equals,
		NotLike = Not | Like,
		IsNot = Not | Is,
	}
}