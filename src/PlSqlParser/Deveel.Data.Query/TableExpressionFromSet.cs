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
using System.Collections.Generic;

using Deveel.Data.DbSystem;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql;

namespace Deveel.Data.Query {
	public sealed class TableExpressionFromSet {
		/// The list of table resources in this set. (IFromTableSource).
		/// </summary>
		private readonly List<IFromTableSource> tableResources;

		/// <summary>
		/// The list of function expression resources.
		/// </summary>
		/// <example>
		/// For example, one table expression may expose a function as 
		/// <c>SELECT (a + b) AS c, ....</c> in which case we have a 
		/// virtual assignment of c = (a + b) in this set.
		/// </example>
		private readonly List<object> functionResources;

		/// <summary>
		/// The list of Variable references in this set that are exposed 
		/// to the outside, including function aliases.
		/// </summary>
		/// <example>
		/// For example, <c>SELECT a, b, c, (a + 1) d FROM ABCTable</c> 
		/// would be exposing variables 'a', 'b', 'c' and 'd'.
		/// </example>
		private readonly List<ObjectName> exposedVariables;

		/// <summary>
		/// Set to true if this should do case insensitive resolutions.
		/// </summary>
		private bool caseInsensitive;

		public TableExpressionFromSet(bool caseInsensitive) {
			tableResources = new List<IFromTableSource>();
			functionResources = new List<object>();
			exposedVariables = new List<ObjectName>();
			// Is the database case insensitive?
			this.caseInsensitive = caseInsensitive;
		}

		/// <summary>
		/// Gets or sets the parent expression for the current one.
		/// </summary>
		/// <remarks>
		/// Thi can be setted or returns <b>null</b> if the expression
		/// has no parent.
		/// </remarks>
		public TableExpressionFromSet Parent { get; set; }

		/// <summary>
		/// Toggle the case sensitivity flag.
		/// </summary>
		/// <param name="status"></param>
		public void SetCaseInsensitive(bool status) {
			caseInsensitive = status;
		}

		internal bool StringCompare(string str1, string str2) {
			return String.Compare(str1, str2, caseInsensitive) == 0;
		}

		/// <summary>
		/// Adds a table resource to the set.
		/// </summary>
		/// <param name="tableResource"></param>
		public void AddTable(IFromTableSource tableResource) {
			tableResources.Add(tableResource);
		}

		/// <summary>
		/// Adds a function resource to the set.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="expression"></param>
		/// <remarks>
		/// Note that is possible for there to be references in the 
		/// <paramref name="expression"/> that do not reference resources 
		/// in this set (eg. a correlated reference).
		/// </remarks>
		public void AddFunctionRef(string name, Expression expression) {
			functionResources.Add(name);
			functionResources.Add(expression);
		}

		/// <summary>
		/// Adds a variable in this from set that is exposed to the outside.
		/// </summary>
		/// <param name="v"></param>
		/// <remarks>
		/// This list should contain all references from the <c>SELECT ...</c>
		/// part of the query (eg. <c>SELECT a, b, (a + 1) d</c> exposes 
		/// variables a, b and d).
		/// </remarks>
		public void ExposeVariable(ObjectName v) {
			exposedVariables.Add(v);
		}

		/// <summary>
		/// Exposes all the columns from the given <see cref="IFromTableSource"/>.
		/// </summary>
		/// <param name="table"></param>
		public void ExposeAllColumnsFromSource(IFromTableSource table) {
			ObjectName[] v = table.AllColumns;
			for (int p = 0; p < v.Length; ++p) {
				ExposeVariable(v[p]);
			}
		}

		/// <summary>
		/// Exposes all the columns in all the child tables.
		/// </summary>
		public void ExposeAllColumns() {
			for (int i = 0; i < SetCount; ++i) {
				ExposeAllColumnsFromSource(GetTable(i));
			}
		}

		/// <summary>
		/// Exposes all the columns from the given table name.
		/// </summary>
		/// <param name="tn"></param>
		public void ExposeAllColumnsFromSource(ObjectName tn) {
			IFromTableSource table = FindTable(tn.Parent != null ? tn.Parent.Name : null, tn.Name);
			if (table == null)
				throw new ApplicationException("Table name found: " + tn);

			ExposeAllColumnsFromSource(table);
		}

		public ObjectName[] GenerateResolvedVariableList() {
			int sz = exposedVariables.Count;
			ObjectName[] list = new ObjectName[sz];
			for (int i = 0; i < sz; ++i) {
				list[i] = exposedVariables[i].Clone();
			}
			return list;
		}

		internal IFromTableSource FindTable(string schema, string name) {
			for (int p = 0; p < SetCount; ++p) {
				IFromTableSource table = GetTable(p);
				if (table.MatchesReference(null, schema, name)) {
					return table;
				}
			}
			return null;
		}

		internal int SetCount {
			get { return tableResources.Count; }
		}

		/// <summary>
		/// Returns the IFromTableSource object at the given index position in this set.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		internal IFromTableSource GetTable(int i) {
			return tableResources[i];
		}


		/// <summary>
		/// Dereferences a fully qualified reference that is within the 
		/// set.
		/// </summary>
		/// <param name="v"></param>
		/// <example>
		/// For example, <c>SELECT ( a + b ) AS z</c> given <i>z</i> would 
		/// return the expression <c>(a + b)</c>.
		/// </example>
		/// <returns>
		/// Returns the expression part of the assignment or <b>null</b>
		/// if unable to dereference assignment because it does not
		/// exist.
		/// </returns>
		internal Expression DereferenceAssignment(ObjectName v) {
			ObjectName tname = v.Parent;
			string varName = v.Name;

			// We are guarenteed not to match with a function if the table name part
			// of a Variable is present.
			if (tname != null)
				return null;

			// Search for the function with this name
			Expression lastFound = null;
			int matchesFound = 0;
			for (int i = 0; i < functionResources.Count; i += 2) {
				string funName = (string) functionResources[i];
				if (StringCompare(funName, varName)) {
					if (matchesFound > 0)
						throw new ApplicationException("Ambiguous reference '" + v + "'");

					lastFound = (Expression) functionResources[i + 1];
					++matchesFound;
				}
			}

			return lastFound;
		}

		private ObjectName ResolveAssignmentReference(ObjectName v) {
			ObjectName tname = v.Parent;
			string varName = v.Name;

			// We are guarenteed not to match with a function if the table name part
			// of a Variable is present.
			if (tname != null)
				return null;

			// Search for the function with this name
			ObjectName lastFound = null;
			int matchesFound = 0;
			for (int i = 0; i < functionResources.Count; i += 2) {
				string funName = (string) functionResources[i];
				if (StringCompare(funName, varName)) {
					if (matchesFound > 0)
						throw new ApplicationException("Ambiguous reference '" + v + "'");

					lastFound = new ObjectName(funName);
					++matchesFound;
				}
			}

			return lastFound;
		}


		private ObjectName ResolveTableColumnReference(ObjectName v) {
			ObjectName tname = v.Parent;
			string schemaName = null;
			string tableName = null;
			string columnName = v.Name;
			if (tname != null) {
				schemaName = tname.Parent != null ? tname.Parent.Name : null;
				tableName = tname.Name;
			}

			// Find matches in our list of tables sources,
			ObjectName matchedVar = null;

			foreach (IFromTableSource table in tableResources) {
				int rcc = table.ResolveColumnCount(null, schemaName, tableName, columnName);
				if (rcc == 0) {
					// do nothing if no matches
				} else if (rcc == 1 && matchedVar == null) {
					// If 1 match and matched_var = null
					matchedVar = table.ResolveColumn(null, schemaName, tableName, columnName);
				} else {
					throw new ApplicationException("Ambiguous reference '" + v + "'");
				}
			}

			return matchedVar;
		}

		internal ObjectName ResolveReference(ObjectName v) {
			// Try and resolve against alias names first,
			List<ObjectName> list = new List<ObjectName>();

			ObjectName functionVar = ResolveAssignmentReference(v);
			if (functionVar != null) {
				list.Add(functionVar);
			}

			ObjectName tcVar = ResolveTableColumnReference(v);
			if (tcVar != null) {
				list.Add(tcVar);
			}

			// Return the variable if we found one unambiguously.
			int listSize = list.Count;
			if (listSize == 0)
				return null;
			if (listSize == 1)
				return list[0];

			throw new ApplicationException("Ambiguous reference '" + v + "'");
		}

		private CorrelatedVariable GlobalResolveReference(int level, ObjectName v) {
			ObjectName nv = ResolveReference(v);
			if (nv == null && Parent != null)
				// If we need to descend to the parent, increment the level.
				return Parent.GlobalResolveReference(level + 1, v);
			if (nv != null)
				return new CorrelatedVariable(nv, level);
			return null;
		}

		private object QualifyVariable(ObjectName v_in) {
			ObjectName v = ResolveReference(v_in);
			if (v == null) {
				// If not found, try and resolve in parent set (correlated)
				if (Parent != null) {
					CorrelatedVariable cv = Parent.GlobalResolveReference(1, v_in);
					if (cv == null) {
						throw new ApplicationException("Reference '" + v_in + "' not found.");
					}
					return cv;
				}

				//TODO: check this...
				throw new ApplicationException("Reference '" + v_in + "' not found.");
			}
			return v;
		}

		internal IExpressionPreparer ExpressionQualifier {
			get { return new ExpressionPreparerImpl(this); }
		}

		private class ExpressionPreparerImpl : IExpressionPreparer {
			public ExpressionPreparerImpl(TableExpressionFromSet fromSet) {
				this.fromSet = fromSet;
			}

			private readonly TableExpressionFromSet fromSet;

			public bool CanPrepare(Expression expression) {
				return expression is VariableExpression;
			}

			public Expression Prepare(Expression expression) {
				var varExp = (VariableExpression) expression;
				var result = fromSet.QualifyVariable(varExp.VariableName);
				if (result is CorrelatedVariable)
					return new CorrelatedVariableExpression((CorrelatedVariable) result);
				if (result is ObjectName)
					return Expression.Variable((ObjectName) result);
				throw new NotSupportedException();
			}
		}

		internal static TableExpressionFromSet GenerateFromSet(TableSelectExpression selectExpression, ITableSpaceContext db) {
			// Get the 'from_clause' from the table expression
			FromClause fromClause = selectExpression.From;

			// Create a TableExpressionFromSet for this table expression
			var fromSet = new TableExpressionFromSet(db.IsInCaseInsensitive);

			// Add all tables from the 'fromClause'
			foreach (FromTable fromTable in fromClause.AllTables) {
				string uniqueKey = fromTable.UniqueKey;
				ObjectName alias = fromTable.Alias;

				// If this is a sub-command table,
				if (fromTable.IsSubQueryTable) {
					// eg. FROM ( SELECT id FROM Part )
					TableSelectExpression subQuery = fromTable.SubSelect;
					TableExpressionFromSet subQueryFromSet = GenerateFromSet(subQuery, db);

					// The aliased name of the table
					ObjectName aliasTableName = null;
					if (alias != null)
						aliasTableName = alias.Clone();

					var source = new FromTableSubQuerySource(db.IsInCaseInsensitive, uniqueKey, subQuery, subQueryFromSet, aliasTableName);
					// Add to list of subquery tables to add to command,
					fromSet.AddTable(source);
				} else {
					// Else must be a standard command table,
					ObjectName name = fromTable.Name;

					// Resolve to full table name
					ObjectName tableName = db.ResolveTableName(name);

					if (!db.TableExists(tableName))
						throw new ApplicationException("Table '" + tableName + "' was not found.");

					ObjectName givenName = null;
					if (alias != null)
						givenName = alias.Clone();

					// Get the ITableQueryInfo object for this table name (aliased).
					ITableQueryInfo tableQueryInfo = db.GetTableQueryInfo(tableName, givenName);
					var source = new FromTableDirectSource(db.IsInCaseInsensitive, tableQueryInfo, uniqueKey, givenName, tableName);

					fromSet.AddTable(source);
				}
			}  // foreach

			// Set up functions, aliases and exposed variables for this from set,

			// For each column being selected
			foreach (SelectColumn col in selectExpression.Columns) {
				// Is this a glob?  (eg. Part.* )
				if (col.IsGlob) {
					// Find the columns globbed and add to the 'selectedColumns' result.
					if (col.IsAll) {
						fromSet.ExposeAllColumns();
					} else {
						// Otherwise the glob must be of the form '[table name].*'
						string tname = col.GlobPrefix;
						ObjectName tn = ObjectName.Parse(tname);
						fromSet.ExposeAllColumnsFromSource(tn);
					}
				} else {
					// Otherwise must be a standard column reference.  Note that at this
					// time we aren't sure if a column expression is correlated and is
					// referencing an outer source.  This means we can't verify if the
					// column expression is valid or not at this point.

					// If this column is aliased, add it as a function reference to the
					// TableExpressionFromSet.
					var varExpression = (VariableExpression)col.Expression;

					ObjectName alias = col.Alias;
					ObjectName v =  varExpression.VariableName;
					bool aliasMatchV = (v != null && alias != null && fromSet.StringCompare(v.Name, alias.Name));
					if (alias != null && !aliasMatchV) {
						fromSet.AddFunctionRef(alias.Name, col.Expression);
						fromSet.ExposeVariable(alias.Clone());
					} else if (v != null) {
						ObjectName resolved = fromSet.ResolveReference(v);
						fromSet.ExposeVariable(resolved ?? v);
					} else {
						string funName = col.Expression.ToString();
						fromSet.AddFunctionRef(funName, col.Expression);
						fromSet.ExposeVariable(ObjectName.Parse(funName));
					}
				}

			}  // for each column selected

			return fromSet;
		}
	}
}