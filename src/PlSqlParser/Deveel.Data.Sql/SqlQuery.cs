// 
//  Copyright 2010  Deveel
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
using System.Text;

namespace Deveel.Data.Sql {
	public sealed class SqlQuery : ICloneable {
		private String text;
		private bool prepared;

		private object[] parameters;
		private string[] parameters_names;
		private int parameters_index;
		private int parameter_count;


		private SqlQuery() {
		}

		public SqlQuery(string text) {
			this.text = text;
			parameters = new Object[8];
			parameters_names = new string[8];
			parameters_index = 0;
			parameter_count = 0;
			prepared = false;
		}

		private void GrowParametersList(int new_size) {
			// Make new list
			Object[] new_list = new Object[new_size];
			// Copy everything to new list
			Array.Copy(parameters, 0, new_list, 0, parameters.Length);
			// Set the new list.
			parameters = new_list;
		}

		private static object TranslateObjectType(object ob) {
			//TODO: return ObjectTranslator.Translate(ob);
			return ob;
		}

		public void AddVariable(object value) {
			value = TranslateObjectType(value);

			parameters[parameters_index] = value;
			++parameters_index;
			++parameter_count;
			if (parameters_index >= parameters.Length)
				GrowParametersList(parameters_index + 8);
		}

		public void SetVariable(int i, Object ob) {
			ob = TranslateObjectType(ob);
			if (i >= parameters.Length) {
				GrowParametersList(i + 8);
			}
			parameters[i] = ob;
			parameters_index = i + 1;
			parameter_count = System.Math.Max(parameters_index, parameter_count);
		}

		public void Clear() {
			parameters_index = 0;
			parameter_count = 0;
			for (int i = 0; i < parameters.Length; ++i) {
				parameters[i] = null;
			}
		}

		public string Text {
			get { return text; }
		}

		public object[] Variables {
			get { return parameters; }
		}

		public string[] VariableNames {
			get { return parameters_names; }
		}

		/// <inheritdoc/>
		public override bool Equals(Object ob) {
			SqlQuery q2 = (SqlQuery)ob;
			// NOTE: This could do syntax analysis on the query string to determine
			//   if it's the same or not.
			if (text.Equals(q2.text)) {
				if (parameter_count == q2.parameter_count) {
					for (int i = 0; i < parameter_count; ++i) {
						if (parameters[i] != q2.parameters[i]) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		public object Clone() {
			SqlQuery q = new SqlQuery();
			q.text = text;
			q.parameters = (Object[])parameters.Clone();
			q.parameters_index = parameters_index;
			q.parameter_count = parameter_count;
			q.prepared = prepared;
			return q;
		}

		/// <inheritdoc/>
		public override string ToString() {
			StringBuilder buf = new StringBuilder();
			buf.AppendLine("[ Command: ");
			buf.Append("[ ");
			buf.Append(Text);
			buf.AppendLine(" ]");
			if (parameter_count > 0) {
				buf.AppendLine();
				buf.AppendLine("Params: ");
				buf.Append("[ ");
				for (int i = 0; i < parameter_count; ++i) {
					Object ob = parameters[i];
					if (ob == null) {
						buf.Append("NULL");
					} else {
						buf.Append(parameters[i].ToString());
					}
					buf.Append(", ");
				}
				buf.Append(" ]");
			}
			buf.AppendLine();
			buf.Append("]");
			return buf.ToString();
		}
	}
}