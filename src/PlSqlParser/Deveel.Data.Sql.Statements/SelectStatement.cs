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
using System.Collections.ObjectModel;
using System.Text;

using Deveel.Data.DbSystem;
using Deveel.Data.Expressions;
using Deveel.Data.Query;

namespace Deveel.Data.Sql.Statements {
	public sealed class SelectStatement : Statement {
		private SelectIntoClause intoClause;

		public SelectStatement() {
			OrderBy = new Collection<ByColumn>();
		}

		public TableSelectExpression SelectExpression { get; set; }

		public IList<ByColumn> OrderBy { get; private set; }

		public IQueryPlanNode Plan { get; private set; }

		protected override Statement Prepare(IQueryContext context) {
			var selectStatement = new SelectStatement {
				SelectExpression = SelectExpression,
				OrderBy = OrderBy
			};

			// check to see if the construct is the special one for
			// selecting the latest IDENTITY value from a table
			if (IsIdentitySelect(selectStatement.SelectExpression)) {
				selectStatement.SelectExpression.Columns.RemoveAt(0);
				FromTable fromTable = ((IList<FromTable>)selectStatement.SelectExpression.From.AllTables)[0];
				var idExp = Expression.FunctionCall("identity", new FunctionArgument(Expression.Constant(DataObject.String(fromTable.Name.ToString()))));
				selectStatement.SelectExpression.Columns.Add(new SelectColumn(idExp));
			}

			if (selectStatement.SelectExpression.Into != null)
				selectStatement.intoClause = selectStatement.SelectExpression.Into;

			// Generate the TableExpressionFromSet hierarchy for the expression,
			TableExpressionFromSet fromSet = Planner.GenerateFromSet(selectStatement.SelectExpression, context.Connection);

			// Form the plan
			selectStatement.Plan = Planner.FormQueryPlan(context.Connection, selectStatement.SelectExpression, fromSet, selectStatement.OrderBy);
			return selectStatement;
		}

		private bool IsIdentitySelect(TableSelectExpression expression) {
			if (expression.Columns.Count != 1)
				return false;
			if (expression.From == null)
				return false;
			if (expression.From.AllTables.Count != 1)
				return false;

			SelectColumn column = expression.Columns[0];
			if (column.Alias == null)
				return false;
			if (column.Alias.Name != "IDENTITY")
				return false;

			return true;

		}

		public override ITable Evaluate(IQueryContext context) {
			// Check the permissions for this user to select from the tables in the
			// given plan.
			CheckUserSelectPermissions(context, Plan);

			ITable t = Plan.Evaluate(context);

			/*
			TODO:
			if (intoClause != null &&
			    (intoClause.HasElements || intoClause.HasTableName))
				t = intoClause.SelectInto(context, t);
			*/

			return t;
		}

		private void CheckUserSelectPermissions(IQueryContext context, IQueryPlanNode plan) {
			// Discover the list of TableName objects this command touches,
			IList<ObjectName> touchedTables = plan.DiscoverTableNames(new List<ObjectName>());
			IDatabase dbase = context.Connection.Database;

			/*
			TODO:
			// Check that the user is allowed to select from these tables.
			foreach (ObjectName table in touchedTables) {
				if (!dbase.CanUserSelectFromTableObject(context, table, null))
					throw new UserAccessException("User not permitted to select from table: " + table);
			}
			*/
		}

		protected override void DumpTo(StringBuilder builder) {
			SelectExpression.DumpSqlTo(builder);

			if (OrderBy.Count > 0) {
				builder.Append("ORDER BY ");

				var orderByCount = OrderBy.Count;
				var i = -1;
				foreach (var column in OrderBy) {
					column.DumpSqlTo(builder);

					if (++i < orderByCount - 1)
						builder.Append(", ");				
				}
			}

			base.DumpTo(builder);
		}
	}
}