using System;

namespace Deveel.Data.Expressions {
	public interface IExpressionVisitor {
		void Visit(Expression expression);
	}
}