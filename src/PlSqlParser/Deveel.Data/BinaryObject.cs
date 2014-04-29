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
using System.IO;

namespace Deveel.Data {
	[Serializable]
	public sealed class BinaryObject : IBinaryObject {
		private readonly byte[] buffer;

		public BinaryObject(byte[] buffer, int offset, int length) {
			this.buffer = new byte[length];
			Array.Copy(buffer, offset, this.buffer, 0, length);
		}

		public BinaryObject(byte[] buffer)
			: this(buffer, 0, buffer.Length) {
		}

		long IBinaryObject.Length {
			get { return (long) Length; }
		}

		public int Length {
			get { return buffer.Length; }
		}

		public Stream GetInputStream() {
			return new MemoryStream(buffer, false);
		}
	}
}