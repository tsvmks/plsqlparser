using System;

using Deveel.Data.Expressions;

namespace Deveel.Data.Sql.Statements {
	[Serializable]
	public sealed class AssignmentStatement : Statement {
		public AssignmentStatement(Expression member, Expression assignExpression) {
			if (member == null)
				throw new ArgumentNullException("member");

			AssignExpression = assignExpression;
			Member = member;
		}

		public Expression Member { get; private set; }

		public Expression AssignExpression { get; private set; }
	}
}