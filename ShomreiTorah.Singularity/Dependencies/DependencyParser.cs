using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections;
using System.Reflection;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.Dependencies {
	sealed class DependencyParser : ExpressionVisitor {
		public static Dependency GetDependencies(IDependencyClient client, TableSchema schema, LambdaExpression expression) {
			if (client == null) throw new ArgumentNullException("client");
			if (schema == null) throw new ArgumentNullException("schema");
			if (expression == null) throw new ArgumentNullException("expression");

			var info = new RowInfo(schema);
			new DependencyParser(info, expression.Parameters.Single()).Visit(expression);
			var setup = info.CreateDependencySetup(client);

			//If the expression depends on columns in the same schema,
			//it's simplest to return a single SameRowDependancy with 
			//child dependencies, if any.  If it only depends on child
			//or parent rows, returning a SameRowDependancy will cause
			//extra handlers to be added, so I return the actual child
			//dependencies instead.
			if (info.DependantColumns.Any())
				return new SameRowDependancy(setup);
			else
				return new AggregateDependency(client, setup.NestedDependencies.Select(f => f(client)).ToArray());
		}
		private DependencyParser(RowInfo info, ParameterExpression parameter) {
			rowParameters.Add(parameter, info);
		}

		protected override Expression VisitMemberAccess(MemberExpression m) {
			if (m.Expression.Type == typeof(Row))
				VisitRowProperty(GetName(m), m);

			return base.VisitMemberAccess(m);
		}
		protected override Expression VisitMethodCall(MethodCallExpression m) {
			if (m.Object.Type == typeof(Row))
				VisitRowProperty(GetName(m), m);
			else if (IsLinqMethod(m.Method)) {
				var childRow = GetRow(m.Arguments[0]);
				if (typeof(Row).IsAssignableFrom(m.Type)
				 || (typeof(IEnumerable).IsAssignableFrom(m.Type)
				  && m.Type.IsGenericType
				  && !m.Type.ContainsGenericParameters
				  && typeof(Row).IsAssignableFrom(m.Type.GetGenericArguments()[0]))
				) {
					if (m.Method.Name == "Select")
						throw new InvalidOperationException("Select calls cannot return rows");
					rowParameters.Add(m, childRow);
				}

				foreach (var argument in m.Arguments.OfType<LambdaExpression>()
													.SelectMany(l => l.Parameters)
													.Where(p => typeof(Row).IsAssignableFrom(p.Type))) {
					rowParameters.Add(argument, childRow);
				}
			}

			return base.VisitMethodCall(m);
		}
		static bool IsLinqMethod(MethodInfo method) {
			if (!method.IsStatic) return false;
			var parameters = method.GetParameters();
			if (parameters.Length < 2) return false;

			if (parameters.Count(p => typeof(IEnumerable).IsAssignableFrom(p.ParameterType)) != 1)
				return false;
			if (!typeof(Delegate).IsAssignableFrom(parameters[1].ParameterType))
				return false;

			return true;
		}

		void VisitRowProperty(string name, Expression expr) {
			var rowInfo = GetRow(expr);

			var column = rowInfo.Schema.Columns[name];
			if (column != null) {
				rowInfo.DependantColumns.Add(rowInfo.Schema.Columns[name]);
				return;
			}

			var relation = rowInfo.Schema.ChildRelations[name];
			if (relation != null) {
				if (!rowInfo.ChildRows.ContainsKey(relation))
					rowInfo.ChildRows.Add(relation, new RowInfo(relation.ParentSchema));
				return;
			}

			throw new InvalidOperationException("Unknown row property: " + expr);
		}

		readonly Dictionary<Expression, RowInfo> rowParameters = new Dictionary<Expression, RowInfo>();

		///<summary>Finds the RowInfo that represents the schema accessed by a parameter, LINQ call, or parent row expression.</summary>
		RowInfo GetRow(Expression expr) {
			//expr might be a call to a LINQ method that returns an
			//IEnumerable<Row>; if so, it will be in the dictionary
			RowInfo retVal;
			if (rowParameters.TryGetValue(expr, out retVal))
				return retVal;

			if (!typeof(Row).IsAssignableFrom(expr.Type))
				throw new ArgumentException("Expression must be a row", "expr");

			var childRow = GetRow(GetParent(expr));

			var key = (ForeignKeyColumn)childRow.Schema.Columns[GetName(expr)];
			if (childRow.ParentRows.TryGetValue(key, out retVal))
				return retVal;
			retVal = new RowInfo(key.ForeignSchema);
			childRow.ParentRows.Add(key, retVal);
			return retVal;
		}

		///<summary>Gets the expression for the object that a field or method is invoked on.</summary>
		static Expression GetParent(Expression expr) {
			var member = expr as MemberExpression;
			if (member != null)
				return member.Expression;

			var methodCall = expr as MethodCallExpression;
			if (methodCall != null)
				return methodCall.Object;

			//Other possibilities:
			//InvocationExpression, UnaryExpression, TypeBinaryExpression

			throw new ArgumentException("Expression does not have a parent", "expr");
		}

		///<summary>Gets the column or relation name that an expressions resolves to.</summary>
		static string GetName(Expression expr) {
			//Strongly-typed property.  (Returns Row, IChildRowCollection, or scalar value)
			var member = expr as MemberExpression;
			if (member != null)
				return member.Member.Name;

			//Untyped indexer (get_Item), typed field (Field<T>), child collection (ChildRows)
			//All of these methods take a string, Column, or ChildRelation.
			var methodCall = expr as MethodCallExpression;
			if (methodCall != null && methodCall.Arguments.Count == 1) {
				var parameter = methodCall.Arguments[0] as ConstantExpression;
				if (parameter != null) {
					var no = parameter.Value as INamedObject;
					if (no != null)
						return no.Name;
					return parameter.Value.ToString();
				}
			}
			throw new InvalidOperationException("Unsupported named accessor: " + expr);
		}

		sealed class RowInfo {
			public RowInfo(TableSchema schema) {
				Schema = schema;
				DependantColumns = new SchemaColumnCollection(schema);
				ParentRows = new Dictionary<ForeignKeyColumn, RowInfo>();
				ChildRows = new Dictionary<ChildRelation, RowInfo>();
			}

			public TableSchema Schema { get; private set; }

			public Collection<Column> DependantColumns { get; private set; }
			public Dictionary<ForeignKeyColumn, RowInfo> ParentRows { get; private set; }
			public Dictionary<ChildRelation, RowInfo> ChildRows { get; private set; }

			public RowDependencySetup CreateDependencySetup(IDependencyClient client) {
				var setup = new RowDependencySetup(Schema, client);
				setup.DependantColumns.AddRange<Column, Column>(DependantColumns);

				setup.NestedDependencies.AddRange(ParentRows.Select(kvp => new Func<IDependencyClient, Dependency>(
					c => new ParentRowDependency(kvp.Value.CreateDependencySetup(c), kvp.Key)
				)));

				setup.NestedDependencies.AddRange(ChildRows.Select(kvp => new Func<IDependencyClient, Dependency>(
					c => new ChildRowDependency(kvp.Value.CreateDependencySetup(c), kvp.Key)
				)));

				return setup;
			}
		}
	}
	class SchemaColumnCollection : Collection<Column> {
		public SchemaColumnCollection(TableSchema schema) { Schema = schema; }
		public TableSchema Schema { get; private set; }
		protected override void InsertItem(int index, Column item) {
			if (item == null) throw new ArgumentNullException("item");
			if (item.Schema != Schema) throw new ArgumentException("Column must belong to parent schema", "item");
			base.InsertItem(index, item);
		}
		protected override void SetItem(int index, Column item) {
			if (item == null) throw new ArgumentNullException("item");
			if (item.Schema != Schema) throw new ArgumentException("Column must belong to parent schema", "item");

			base.SetItem(index, item);
		}
	}
}
