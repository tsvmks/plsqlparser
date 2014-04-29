using System;
using System.Diagnostics;
using System.Text;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql.Statements {
	[Serializable]
	[DebuggerDisplay("{ToString(), nq}")]
	public abstract class Statement : IPreparable {
		protected Statement() {
		}

		protected virtual void DumpTo(StringBuilder builder) {
		}

		public virtual Statement Prepare(IExpressionPreparer preparer) {
			return this;
		}

		object IPreparable.Prepare(IExpressionPreparer preparer) {
			return Prepare(preparer);
		}

		public override string ToString() {
			var builder = new StringBuilder();
			DumpTo(builder);
			return builder.ToString();
		}
	}
}