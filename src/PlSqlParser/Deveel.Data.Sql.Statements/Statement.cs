using System;
using System.Diagnostics;
using System.Text;

namespace Deveel.Data.Sql.Statements {
	[Serializable]
	[DebuggerDisplay("{ToString(), nq}")]
	public abstract class Statement {
		protected Statement() {
		}

		protected virtual void DumpTo(StringBuilder builder) {
		}

		public override string ToString() {
			var builder = new StringBuilder();
			DumpTo(builder);
			return builder.ToString();
		}
	}
}