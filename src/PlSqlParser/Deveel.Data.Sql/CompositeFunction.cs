using System;

namespace Deveel.Data.Sql {
	public enum CompositeFunction {
		Union = 1,
		Intersect = 2,
		Except = 3,
		None = -1
	}
}