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

using Deveel.Data.DbSystem;

namespace Deveel.Data.Query {
	public interface ITableQueryInfo {
		/// <summary>
		/// Returns an immutable <see cref="TableInfo"/> that describes 
		/// the columns in this table source, and the name of the table.
		/// </summary>
		DataTableInfo TableInfo { get; }

		/// <summary>
		/// Returns a <see cref="IQueryPlanNode"/> that can be put into a plan 
		/// tree and can be evaluated to find the result of the table.
		/// </summary>
		/// <remarks>
		/// This property should always return a new object representing 
		/// the query plan.
		/// </remarks>
		IQueryPlanNode QueryPlanNode { get; }
	}
}