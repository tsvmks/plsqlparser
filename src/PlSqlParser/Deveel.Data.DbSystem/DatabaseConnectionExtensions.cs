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

namespace Deveel.Data.DbSystem {
	public static class DatabaseConnectionExtensions {
		public static ITable GetTable(this IDatabaseConnection connection, ObjectName name) {
			// TODO: name = connection.SubstituteReservedTableName(name);

			try {
				// Special handling of NEW and OLD table, we cache the DataTable in the
				// OldNewTableState object,
				/* TODO:
				if (name.Equals(SystemSchema.OldTriggerTable))
					return connection.OldNewState.OldDataTable ??
						   (connection.OldNewState.OldDataTable = new DataTable(connection, connection.Transaction.GetTable(name)));
				if (name.Equals(SystemSchema.NewTriggerTable))
					return connection.OldNewState.NewDataTable ??
						   (connection.OldNewState.NewDataTable = new DataTable(connection, connection.Transaction.GetTable(name)));
				*/

				// Ask the transaction for the table
				ITable table = connection.Transaction.GetTable(name);

				// if not found in the transaction return null
				if (table == null)
					return null;

				/*
				TODO:
				// Is this table in the tables_cache?
				ITable dtable = connection.GetCachedTable(name);
				if (dtable == null) {
					// No, so wrap it around a Datatable and WriteByte it in the cache
					dtable = new DataTable(connection, table);
					connection.CacheTable(name, dtable);
				}
				
				// Return the DataTable
				return dtable;
				*/

				return table;
			} catch (Exception e) {
				// TODO: connection.Transaction.Context.SystemContext.Logger.Error(connection, e);
				throw new ApplicationException("Database Exception: " + e.Message, e);
			}

		}
	}
}