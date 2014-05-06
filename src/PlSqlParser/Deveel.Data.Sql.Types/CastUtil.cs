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
using System.Text;

namespace Deveel.Data.Sql.Types {
	static class CastUtil {
		public static string PaddedString(String str, int size) {
			if (size == -1) {
				return str;
			}
			int dif = size - str.Length;
			if (dif > 0) {
				var buf = new StringBuilder(str);
				for (int n = 0; n < dif; ++n) {
					buf.Append(' ');
				}
				return buf.ToString();
			}
			if (dif < 0) {
				return str.Substring(0, size);
			}
			return str;
		}
	}
}