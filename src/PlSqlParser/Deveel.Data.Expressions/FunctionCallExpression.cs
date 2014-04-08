using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Deveel.Data.Expressions {
	public sealed class FunctionCallExpression : Expression {
		public FunctionCallExpression(string functionName, IEnumerable<Expression> arguments) 
			: this(null, functionName, arguments) {
		}

		public FunctionCallExpression(Expression obj, string functionName, IEnumerable<Expression> arguments) {
			Arguments = arguments;
			FunctionName = functionName;
			Object = obj;
		}

		public Expression Object { get; private set; }

		public string FunctionName { get; private set; }

		public override ExpressionType ExpressionType {
			get { return ExpressionType.Call; }
		}

		public IEnumerable<Expression> Arguments { get; private set; }
	}
}