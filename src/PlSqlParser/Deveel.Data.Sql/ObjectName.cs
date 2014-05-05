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
using System.Text;

namespace Deveel.Data {
	[Serializable]
	[DebuggerDisplay("{FullName}")]
	public sealed class ObjectName : IEquatable<ObjectName>, IComparable<ObjectName>, ICloneable {
		public const string GlobName = "*";

		public ObjectName(string name) 
			: this(null, name) {
		}

		public ObjectName(ObjectName parent, string name) {
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			
			Name = name;
			Parent = parent;
		}

		public ObjectName Parent { get; private set; }

		public string Name { get; private set; }

		public string FullName {
			get { return ToString(); }
		}

		public bool IsGlob {
			get { return Name.Equals(GlobName); }
		}

		public static ObjectName Parse(string s) {
			if (String.IsNullOrEmpty(s))
				throw new ArgumentNullException("s");

			var sp = s.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
			if (sp.Length == 0)
				throw new FormatException("At least one part of the name must be provided");

			if (sp.Length == 1)
				return new ObjectName(sp[0]);

			ObjectName finalName = null;
			for (int i = sp.Length - 1; i >= 0; i--) {
				if (finalName == null) {
					finalName = new ObjectName(sp[i]);
				} else {
					finalName = new ObjectName(finalName, sp[i]);
				}
			}

			return finalName;
		}

		public static ObjectName ResolveSchema(string schemaName, string name) {
			var sb = new StringBuilder();
			if (!String.IsNullOrEmpty(schemaName))
				sb.Append(schemaName).Append('.');
			sb.Append(name);

			return Parse(sb.ToString());
		}

		public ObjectName Child(string name) {
			return new ObjectName(this, name);
		}

		public ObjectName Child(ObjectName childName) {
			var baseName = this;
			ObjectName parent = childName.Parent;
			while (parent != null) {
				baseName = baseName.Child(parent.Name);
				parent = parent.Parent;
			}

			baseName = baseName.Child(childName.Name);
			return baseName;
		}

		public int CompareTo(ObjectName other) {
			if (other == null)
				return -1;

			int v = 0;
			if (Parent != null)
				v = Parent.CompareTo(other.Parent);

			if (v == 0)
				v = Name.CompareTo(other.Name);

			return v;
		}

		public override string ToString() {
			var sb = new StringBuilder();
			if (Parent != null) {
				sb.Append(Parent);
				sb.Append('.');
			}

			sb.Append(Name);
			return sb.ToString();
		}

		public override bool Equals(object obj) {
			var other = obj as ObjectName;
			if (other == null)
				return false;

			return Equals(other);
		}

		public bool Equals(ObjectName other) {
			return Equals(other, true);
		}

		public bool Equals(ObjectName other, bool ignoreCase) {
			if (Parent != null && other.Parent == null)
				return false;
			if (Parent == null && other.Parent != null)
				return false;

			if (Parent != null && !Parent.Equals(other.Parent, ignoreCase))
				return false;

			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return String.Equals(Name, other.Name, comparison);
		}

		public override int GetHashCode() {
			var code = Name.GetHashCode() ^ 5623;
			if (Parent != null)
				code ^= Parent.GetHashCode();

			return code;
		}

		public ObjectName Clone() {
			return new ObjectName(Parent, Name);
		}

		object ICloneable.Clone() {
			return Clone();
		}
	}
}