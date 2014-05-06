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
using System.Diagnostics;

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Query {
	[Serializable]
	[DebuggerDisplay("{ToString(),nq} = {EvalResult}")]
	public sealed class CorrelatedVariable {
		public CorrelatedVariable(ObjectName variable, int level) {
			Variable = variable;
			Level = level;
		}

		public ObjectName Variable { get; private set; }

		public int Level { get; private set; }

		public void SetFromResolver(IVariableResolver resolver) {
			ObjectName v = Variable;
			EvalResult = resolver.Resolve(v);
		}

		public DataObject EvalResult { get; private set; }

		public DataType ReturnType {
			get { return EvalResult.DataType; }
		}

		public override string ToString() {
			return String.Format("CORRELATED: {0}", Variable);
		}
	}
}