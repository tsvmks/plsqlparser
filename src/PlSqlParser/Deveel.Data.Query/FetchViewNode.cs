// 
//  Copyright 2014  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Query {
	/// <summary>
	/// The node that fetches a view from the current session.
	/// </summary>
	/// <remarks>
	/// This is a tree node that has no children, however the child can 
	/// be created by calling <see cref="CreateViewChildNode"/>. This node 
	/// can be removed from a plan tree by calling <see cref="CreateViewChildNode"/>
	/// and substituting this node with the returned child. 
	/// For a planner that normalizes and optimizes plan trees, this is 
	/// a useful feature.
	/// </remarks>
	[Serializable]
	public class FetchViewNode : IQueryPlanNode {
		/// <summary>
		/// The name of the view to fetch.
		/// </summary>
		private readonly ObjectName tableName;

		/// <summary>
		/// The name to alias the table as.
		/// </summary>
		private readonly ObjectName aliasName;

		public FetchViewNode(ObjectName tableName, ObjectName aliasName) {
			this.tableName = tableName;
			this.aliasName = aliasName;
		}

		/// <summary>
		/// Looks up the query plan in the given context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>
		/// Returns the <see cref="IQueryPlanNode"/> that resolves to the view.
		/// </returns>
		public virtual IQueryPlanNode CreateViewChildNode(IQueryContext context) {
			DatabaseQueryContext db = (DatabaseQueryContext)context;
			return db.CreateViewQueryPlanNode(tableName);
		}

		/// <inheritdoc/>
		public virtual IList<ObjectName> DiscoverTableNames(IList<ObjectName> list) {
			if (!list.Contains(tableName))
				list.Add(tableName);
			return list;
		}

		/// <inheritdoc/>
		public virtual ITable Evaluate(IQueryContext context) {
			// Create the view child node
			IQueryPlanNode node = CreateViewChildNode(context);
			// Evaluate the plan
			ITable t = node.Evaluate(context);

			return aliasName != null ? new ReferenceTable((Table) t, aliasName) : t;
		}

		/// <inheritdoc/>
		public virtual IList<CorrelatedVariable> DiscoverCorrelatedVariables(int level, IList<CorrelatedVariable> list) {
			return list;
		}

		public void Explain(StringBuilder output) {
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public virtual string Name {
			get { return "VIEW: " + tableName + " AS " + aliasName; }
		}
	}
}