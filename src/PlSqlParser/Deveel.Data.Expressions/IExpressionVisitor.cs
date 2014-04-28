using System;

namespace Deveel.Data.Expressions {
	public interface IExpressionVisitor {
		Expression Visit(Expression expression);
	}
}