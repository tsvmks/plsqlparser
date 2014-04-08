using System;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Expressions {
	public sealed class EvaluationContext {
		public EvaluationContext(IQueryContext context) {
			Context = context;
		}

		public IQueryContext Context { get; private set; }
	}
}