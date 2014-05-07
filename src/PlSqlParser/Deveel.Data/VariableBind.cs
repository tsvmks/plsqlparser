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

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data {
	[Serializable]
	[DebuggerDisplay("{ToString()}")]
	public sealed class VariableBind : ISqlElement {
		public VariableBind(ObjectName variableName) {
			if (variableName == null)
				throw new ArgumentNullException("variableName");

			VariableName = variableName;
		}

		public ObjectName VariableName { get; private set; }

		public override string ToString() {
			return String.Format(":{0}", VariableName);
		}

		object IPreparable.Prepare(IExpressionPreparer preparer) {
			return this;
		}

		void ISqlElement.ToString(ISqlWriter writer) {
			writer.Write(":");
			writer.Write(VariableName);
		}
	}
}