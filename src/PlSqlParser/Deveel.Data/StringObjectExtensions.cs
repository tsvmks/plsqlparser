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

namespace Deveel.Data {
	public static class StringObjectExtensions {
		public static string ToString(this IStringObject obj) {
			if (obj == null)
				return null;

			var sb = new StringBuilder(obj.Length);
			using (var reader = obj.GetInput()) {
				var buffer = new char[254];
				int readCount;
				while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
					sb.Append(buffer, 0, readCount);
				}
			}

			return sb.ToString();
		}

		public static IStringObject Concat(this IStringObject obj, IStringObject other) {
			if (obj == null && other == null)
				return null;

			if (obj == null)
				return other;
			if (other == null)
				return obj;

			var length = obj.Length + other.Length;

			// TODO: Support bigger strings ( <= Int64.MaxValue )
			if (length > Int32.MaxValue)
				throw new InvalidOperationException(
					String.Format("The concatenation of the two strings would exceed the maximum size supported ( {0} + {1} > {2} )",
						obj.Length,
						other.Length,
						Int32.MaxValue));

			var sb = new StringBuilder(length);
			using (var reader = obj.GetInput()) {
				var buffer = new char[254];
				int readCount;
				while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
					sb.Append(buffer, 0, readCount);
				}
			}

			using (var reader = other.GetInput()) {
				var buffer = new char[254];
				int readCount;
				while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
					sb.Append(buffer, 0, readCount);
				}
			}

			return new StringObject(sb.ToString());
		}
	}
}