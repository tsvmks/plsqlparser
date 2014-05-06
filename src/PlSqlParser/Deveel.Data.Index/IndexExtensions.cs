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
using System.Linq;

namespace Deveel.Data.Index {
	public static class IndexExtensions {
		public static IEnumerable<T> AsEnumerable<T>(this IIndex<T> index) {
			List<T> list = new List<T>(index.Count);
			IIndexEnumerator<T> i = index.GetEnumerator();
			// NOTE: We are guarenteed the size of the 'list' array matches the size
			//   of input list.
			while (i.MoveNext()) {
				list.Add(i.Current);
			}

			return list;
		}
	}
}