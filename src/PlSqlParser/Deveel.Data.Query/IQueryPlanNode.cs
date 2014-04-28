using System;
using System.Collections.Generic;
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Query {
	public interface IQueryPlanNode {
		object Evaluate(IQueryContext context);

		IList<ObjectName> DiscoverTableNames(IList<ObjectName> list);

		IList<ObjectName> DiscoverCorrelatedVariables(IList<ObjectName> list);

		void Explain(StringBuilder output);
	}
}