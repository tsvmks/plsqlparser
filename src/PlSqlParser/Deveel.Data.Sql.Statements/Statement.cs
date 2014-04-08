using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Statements {
	[Serializable]
	public abstract class Statement {
		private readonly IDictionary<string, object> elements;

		protected Statement() {
			elements = new Dictionary<string, object>();
		}

		public void SetValue(string key, object value) {
			elements[key] = value;
		}

		public object GetValue(string key) {
			object value;
			if (!elements.TryGetValue(key, out value))
				return null;

			return value;
		}
	}
}