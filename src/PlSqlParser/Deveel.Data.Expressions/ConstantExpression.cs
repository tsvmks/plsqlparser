using System;
using System.Text;

namespace Deveel.Data.Expressions {
	[Serializable]
	public sealed class ConstantExpression : Expression {
		public DataObject Value { get; private set; }

		public ConstantExpression(DataObject value) {
			Value = value;
		}

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Constant; }
		}

		protected override void DumpToString(StringBuilder sb) {
			sb.Append(Value.Value);
		}
	}
}