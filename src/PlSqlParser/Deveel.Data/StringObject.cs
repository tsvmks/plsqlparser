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
	public sealed class StringObject : IStringObject {
		private readonly string s;

		public StringObject(string s) {
			this.s = s;
		}

		public StringObject(char[] chars, int offset, int count)
			: this(new string(chars, offset, count)) {
		}

		public StringObject(char[] chars)
			: this(chars, 0, chars.Length) {
		}

		public int Length {
			get { return s.Length; }
		}

		public TextReader GetInput() {
			return new StringReader(s);
		}

		public override bool Equals(object obj) {
			if (obj is string)
				obj = new StringObject((string)obj);

			var other = obj as StringObject;
			return Equals(other);
		}

		public bool Equals(StringObject obj) {
			return s.Equals(obj.s);
		}

		public override int GetHashCode() {
			return s.GetHashCode();
		}
	}
}