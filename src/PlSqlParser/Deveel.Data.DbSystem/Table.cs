// 
//  Copyright 2010-2014 Deveel
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Deveel.Data.Index;
using Deveel.Data.Types;

using SysMath = System.Math;

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// This is a definition for a table in the database.
	/// </summary>
	/// <remarks>
	/// It stores the name of the table, and the fields (columns) in the 
	/// table.  A table represents either a 'core' <see cref="DataTable"/>
	/// that directly maps to the information stored in the database, or a 
	/// temporary table generated on the fly.
	/// <para>
	/// It is an abstract class, because it does not implement the methods to 
	/// add, remove or access row data in the table.
	/// </para>
	/// </remarks>
	public abstract class Table : ITable {

		/// <summary>
		/// Returns the <see cref="Database"/> object that this table is derived from.
		/// </summary>
		public abstract IDatabase Database { get; }

		/// <summary>
		/// Returns the number of columns in the table.
		/// </summary>
		public abstract int ColumnCount { get; }

		/// <summary>
		/// Returns the number of rows stored in the table.
		/// </summary>
		public abstract long RowCount { get; }

		private ObjectName ResolveColumnName(string columnName) {
			return new ObjectName(TableInfo.TableName, columnName);
		}

		private ObjectName[] ResolveColumnNames(string[] columnNames) {
			if (columnNames == null)
				return new ObjectName[0];

			ObjectName[] variableNames = new ObjectName[columnNames.Length];
			for (int i = 0; i < columnNames.Length; i++) {
				variableNames[i] = ResolveColumnName(columnNames[i]);
			}

			return variableNames;
		}

		public DataType GetTTypeForColumn(int column) {
			return TableInfo[column].DataType;
		}

		public DataType GetTTypeForColumn(ObjectName v) {
			return GetTTypeForColumn(FindFieldName(v));
		}

		public abstract int FindFieldName(ObjectName v);


		public abstract ObjectName GetResolvedVariable(int column);

		internal abstract SelectableScheme GetSelectableSchemeFor(int column, int originalColumn, Table table);

		internal abstract void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor);

		internal abstract RawTableInformation ResolveToRawTable(RawTableInformation info);

		public abstract DataObject GetValue(int column, long row);

		public abstract IEnumerator<long> GetRowEnumerator();

		public abstract DataTableInfo TableInfo { get; }


		public abstract void LockRoot(int lockKey);

		public abstract void UnlockRoot(int lockKey);

		public abstract bool HasRootsLocked { get; }

		// ---------- Implemented from ITableDataSource ----------

		public SelectableScheme GetScheme(int column) {
			return GetSelectableSchemeFor(column, column, this);
		}

		public SelectableScheme GetColumnScheme(ObjectName columnName) {
			return GetScheme(FastFindFieldName(columnName));
		}

		public SelectableScheme GetColumnScheme(string columnName) {
			return GetColumnScheme(ResolveColumnName(columnName));
		}

		// ---------- Convenience methods ----------

		/// <summary>
		/// Returns the <see cref="DataColumnInfo"/> object for the 
		/// given column index.
		/// </summary>
		/// <param name="columnOffset"></param>
		/// <returns></returns>
		public DataColumnInfo GetColumnInfo(int columnOffset) {
			return TableInfo[columnOffset];
		}


		/** ======================= Table Operations ========================= */


		// Stores col name -> col index lookups
		private Dictionary<ObjectName, int> colNameLookup;
		private readonly object colLookupLock = new object();

		/// <summary>
		/// Provides faster way to find a column index given a column name.
		/// </summary>
		/// <param name="col">Name of the column to get the index for.</param>
		/// <returns>
		/// Returns the index of the column for the given name, or -1
		/// if not found.
		/// </returns>
		public int FastFindFieldName(ObjectName col) {
			lock (colLookupLock) {
				if (colNameLookup == null)
					colNameLookup = new Dictionary<ObjectName, int>(30);

				int index;
				if (!colNameLookup.TryGetValue(col, out index)) {
					index = FindFieldName(col);
					colNameLookup[col] = index;
				}

				return index;
			}
		}

		private int[] FastFindFieldNames(params ObjectName[] columnNames) {
			if (columnNames == null)
				return new int[0];

			int[] colIndex = new int[columnNames.Length];
			for (int i = 0; i < columnNames.Length; i++) {
				colIndex[i] = FastFindFieldName(columnNames[i]);
			}

			return colIndex;
		}

		/// <summary>
		/// Returns a TableVariableResolver object for this table.
		/// </summary>
		/// <returns></returns>
		internal TableVariableResolver GetVariableResolver() {
			return new TableVariableResolver(this);
		}


		// ---------- Inner classes ----------

		/// <summary>
		/// An implementation of <see cref="IVariableResolver"/> that we can use 
		/// to resolve column names in this table to cells for a specific row.
		/// </summary>
		internal class TableVariableResolver : IVariableResolver {
			public TableVariableResolver(Table table) {
				this.table = table;
			}

			private readonly Table table;
			private int rowIndex = -1;

			private int FindColumnName(ObjectName variable) {
				int colIndex = table.FastFindFieldName(variable);
				if (colIndex == -1) {
					throw new ApplicationException("Can't find column: " + variable);
				}
				return colIndex;
			}

			// --- Implemented ---

			public int SetId {
				get { return rowIndex; }
				set { rowIndex = value; }
			}

			public DataObject Resolve(ObjectName variable) {
				return table.GetValue(FindColumnName(variable), rowIndex);
			}

			public DataType ReturnType(ObjectName variable) {
				return table.GetTTypeForColumn(variable);
			}

		}

		public IEnumerator<DataRow> GetEnumerator() {
			return new SimpleDataRowEnumerator(this, GetRowEnumerator());
		}

		/// <inheritdoc/>
		public override String ToString() {
			String name = "VT" + GetHashCode();
			if (this is DataTableBase) {
				name = ((DataTableBase)this).TableName.ToString();
			}
			return name;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Prints a graph of the table hierarchy to the stream.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="indent"></param>
		public virtual void PrintGraph(TextWriter output, int indent) {
			for (int i = 0; i < indent; ++i) {
				output.Write(' ');
			}
			output.WriteLine("T[" + GetType() + "]");
		}
	}
}