using System;
using System.Collections.Generic;
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Query {
	public interface IQueryPlanNode {
		object Evaluate(IQueryContext context);

		IList<string> DiscoverTableNames(IList<string> list);

		IList<string> DiscoverCorrelatedVariables(IList<string> list);

		void Explain(StringBuilder output);
	}
}