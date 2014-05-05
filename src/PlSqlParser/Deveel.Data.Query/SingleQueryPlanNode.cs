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
	[Serializable]
	public abstract class SingleQueryPlanNode : IQueryPlanNode {
		protected SingleQueryPlanNode(IQueryPlanNode child) {
			Child = child;
		}

		protected IQueryPlanNode Child { get; private set; }

		public virtual string Name {
			get { return GetType().Name; }
		}

		public abstract ITable Evaluate(IQueryContext context);

		public virtual IList<ObjectName> DiscoverTableNames(IList<ObjectName> list) {
			return Child.DiscoverTableNames(list);
		}

		public virtual IList<CorrelatedVariable> DiscoverCorrelatedVariables(int level, IList<CorrelatedVariable> list) {
			return Child.DiscoverCorrelatedVariables(level, list);
		}

		public void Explain(StringBuilder output) {
			throw new NotImplementedException();
		}
	}
}