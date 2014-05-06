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

namespace Deveel.Data.Sql {
	public static class SqlWriterExtensions {
		public static void Write(this ISqlWriter writer, string format, params object[] args) {
			writer.Write(String.Format(format, args));
		}

		public static void WriteLine(this ISqlWriter writer, string s) {
			writer.Write(s);
			writer.Write(Environment.NewLine);
		}

		public static void WriteLine(this ISqlWriter writer, string format, params object[] args) {
			writer.WriteLine(String.Format(format, args));
		}

		public static void WriteLine(this ISqlWriter writer) {
			writer.Write(Environment.NewLine);
		}

		public static void Write(this ISqlWriter writer, ISqlElement element) {
			element.ToString(writer);
		}

		public static void Write(this ISqlWriter writer, object obj) {
			var s = obj == null ? String.Empty : obj.ToString();
			writer.Write(s);
		}

		public static void Indent(this ISqlWriter writer, int value) {
			writer.Indentation = writer.Indentation + value;
		}

		public static void Indent(this ISqlWriter writer) {
			writer.Indent(1);
		}

		public static void Deindent(this ISqlWriter writer, int value) {
			if (writer.Indentation - value >= 0)
				writer.Indentation = writer.Indentation - value;
		}

		public static void Deindent(this ISqlWriter writer) {
			writer.Deindent(1);
		}
	}
}