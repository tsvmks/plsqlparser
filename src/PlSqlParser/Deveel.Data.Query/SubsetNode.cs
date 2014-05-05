using System;
using System.Text;

using Deveel.Data.DbSystem;

namespace Deveel.Data.Query {
	/// <summary>
	/// The node for finding a subset and renaming the columns of the 
	/// results in the child node.
	/// </summary>
	[Serializable]
	public class SubsetNode : SingleQueryPlanNode {
		/// <summary>
		/// The original columns in the child that we are to make the subset of.
		/// </summary>
		private readonly ObjectName[] originalColumns;

		/// <summary>
		/// New names to assign the columns.
		/// </summary>
		private readonly ObjectName[] newColumnNames;


		public SubsetNode(IQueryPlanNode child, ObjectName[] originalColumns, ObjectName[] newColumnNames)
			: base(child) {
			this.originalColumns = originalColumns;
			this.newColumnNames = newColumnNames;

		}

		public override ITable Evaluate(IQueryContext context) {
			Table t = (Table) Child.Evaluate(context);

			int sz = originalColumns.Length;
			int[] colMap = new int[sz];

			for (int i = 0; i < sz; ++i) {
				int mapped = t.FindFieldName(originalColumns[i]);
				if (mapped == -1)
					throw new InvalidOperationException(String.Format("Column {0} was not found in table {1} when subsetting.", originalColumns[i], t.TableInfo.Name));

				colMap[i] = mapped;
			}

			var subsetTable = new SubsetColumnTable((Table)t);
			subsetTable.SetColumnMap(colMap, newColumnNames);
			return subsetTable;
		}

		// ---------- Set methods ----------

		/// <summary>
		/// Sets the given table name of the resultant table.
		/// </summary>
		/// <param name="name"></param>
		/// <remarks>
		/// This is intended if we want to create a sub-query that has an 
		/// aliased table name.
		/// </remarks>
		public void SetGivenName(ObjectName name) {
			if (name != null) {
				int sz = newColumnNames.Length;
				for (int i = 0; i < sz; ++i) {
					newColumnNames[i] = new ObjectName(name, newColumnNames[i].Name);
				}
			}
		}

		// ---------- Get methods ----------

		/// <summary>
		/// Returns the list of original columns that represent the mappings from
		/// the columns in this subset.
		/// </summary>
		public ObjectName[] OriginalColumns {
			get { return originalColumns; }
		}

		/// <summary>
		/// Returns the list of new column names that represent the new 
		/// columns in this subset.
		/// </summary>
		public ObjectName[] NewColumnNames {
			get { return newColumnNames; }
		}

		public override string Name {
			get {
				StringBuilder sb = new StringBuilder();
				sb.Append("SUBSET: ");
				for (int i = 0; i < newColumnNames.Length; ++i) {
					sb.Append(newColumnNames[i]);
					sb.Append("->");
					sb.Append(originalColumns[i]);
					sb.Append(", ");
				}
				return sb.ToString();
			}
		}
	}
}