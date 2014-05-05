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
using System.Collections.Generic;

using Deveel.Data.Index;

using SysMath = System.Math;

namespace Deveel.Data.DbSystem {
	public sealed class DataTable : DefaultDataTable {
		/// <summary>
		/// The DatabaseConnection object that is the parent of this DataTable.
		/// </summary>
		private readonly IDatabaseConnection connection;

		/// <summary>
		/// A low level access to the underlying transactional data source.
		/// </summary>
		private readonly ITable dataSource;

#if DEBUG
		/// <summary>
		/// The number of read locks we have on this table.
		/// </summary>
		private int debugReadLockCount;

		/// <summary>
		/// The number of write locks we have on this table (this should 
		/// only ever be 0 or 1).
		/// </summary>
		private int debugWriteLockCount;
#endif

		public DataTable(IDatabaseConnection connection, ITable dataSource)
			: base(connection.Database) {
			this.connection = connection;
			this.dataSource = dataSource;
		}


		/// <inheritdoc/>
		public override long RowCount {
			get {
				return dataSource.RowCount;
			}
		}

		/// <inheritdoc/>
		public override DataTableInfo TableInfo {
			get {
				CheckSafeOperation(); // safe op
				return dataSource.TableInfo;
			}
		}

		/// <summary>
		/// Returns the schema that this table is within.
		/// </summary>
		public string Schema {
			get {
				CheckSafeOperation(); // safe op
				return TableInfo.Schema;
			}
		}

		/// <inheritdoc/>
		public override bool HasRootsLocked {
			get {
				// There is no reason why we would need to know this information at
				// this level.
				// We need to deprecate this properly.
				throw new ApplicationException("hasRootsLocked is deprecated.");
			}
		}

		/// <inheritdoc/>
		public override int ColumnCount {
			get {
				CheckSafeOperation(); // safe op

				return base.ColumnCount;
			}
		}

		private void CheckReadWriteLock() {
		}

		private void CheckSafeOperation() {
			// no operation - nothing to check for...
		}

		/// <inheritdoc/>
		protected override void BlankSelectableSchemes(int type) {
		}

		/// <inheritdoc/>
		protected override SelectableScheme GetRootColumnScheme(int column) {
			return dataSource.GetScheme(column);
		}



		// -------- Methods implemented for DefaultDataTable --------

		/// <inheritdoc/>
		internal override void SetToRowTableDomain(int column, IList<long> rowSet, ITable ancestor) {
			if (ancestor != this && ancestor != dataSource)
				throw new Exception("Method routed to incorrect table ancestor.");
		}

		/// <summary>
		/// Declares the table as a new type.
		/// </summary>
		/// <param name="newName">The name of the declared table.</param>
		/// <returns>
		/// Returns a <see cref="ReferenceTable"/> representing the new 
		/// declaration of the table.
		/// </returns>
		public ReferenceTable DeclareAs(ObjectName newName) {
			return new ReferenceTable(this, newName);
		}

		public DataRow NewRow() {
			CheckSafeOperation();  // safe op
			return new DataRow(this);
		}

		/// <inheritdoc/>
		public override DataObject GetValue(int column, long row) {
			CheckSafeOperation();  // safe op

			return dataSource.GetValue(column, row);
		}

		/// <inheritdoc/>
		public override IEnumerator<long> GetRowEnumerator() {
			return dataSource.GetRowEnumerator();
		}


		/// <inheritdoc/>
		public override void LockRoot(int lockKey) {
			CheckSafeOperation();  // safe op
		}

		/// <inheritdoc/>
		public override void UnlockRoot(int lockKey) {
			CheckSafeOperation();  // safe op
		}


		// ------------ Lock debugging methods ----------

		/// <inheritdoc/>
		public override ObjectName GetResolvedVariable(int column) {
			CheckSafeOperation();  // safe op

			return base.GetResolvedVariable(column);
		}

		/// <inheritdoc/>
		public override int FindFieldName(ObjectName v) {
			CheckSafeOperation();  // safe op

			return base.FindFieldName(v);
		}
	}
}