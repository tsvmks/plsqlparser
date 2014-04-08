using System;

namespace Deveel.Data.Sql {
	public enum JoinType {
		Inner = 1,
		Left = 2,
		Right = 3,
		Full = 4,
		None = -1,
	}
}