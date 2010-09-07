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
	///<summary>Parses dependencies from expressions.</summary>
	public static class DependencyParser {
		///<summary>Parses a lambda expression into a dependency tree.</summary>
		///<param name="schema">The schema that the expression operates on.</param>
		///<param name="expression">The expression to parse.</param>
		public static Dependency GetDependencyTree(TableSchema schema, LambdaExpression expression) {
			if (schema == null) throw new ArgumentNullException("schema");
			if (expression == null) throw new ArgumentNullException("expression");

			var info = new RowInfo(schema);
			new DependencyVisitor(info, expression.Parameters.Single()).Visit(expression);
			var setup = info.CreateDependencySetup();

			//If the expression depends on columns in the same schema,
			//it's simplest to return a single SameRowDependancy with 
			//child dependencies, if any.  If it only depends on child
			//or parent rows, returning a SameRowDependancy will cause
			//extra handlers to be added, so I return the actual child
			//dependencies instead.
			if (info.DependentColumns.Any())
				return new SameRowDependency(setup);
			else
				return new AggregateDependency(setup.NestedDependencies);
		}

		sealed class DependencyVisitor : ExpressionVisitor {
			public DependencyVisitor(RowInfo info, ParameterExpression parameter) {
				rowExpressions.Add(parameter, info);
			}
			public new void Visit(Expression expr) { base.Visit(expr); }

			protected override Expression VisitMethodCall(MethodCallExpression m) {
				if (m.Object != null && m.Object.Type == typeof(Row)) {	//Check for row methods (the null check handles static members)
					VisitRowProperty(GetName(m), m);
				} else if (IsLinqMethod(m.Method)		//Handle LINQ calls on child rows
						&& TypeContains<Row>(m.Arguments[0].Type)) {

					var childRow = GetRow(m.Arguments[0]);

					foreach (var lambdaParameter in m.Arguments.OfType<LambdaExpression>()
														.SelectMany(l => l.Parameters)
														.Where(p => TypeContains<Row>(p.Type))) {
						rowExpressions.Add(lambdaParameter, childRow);
					}
				}

				return base.VisitMethodCall(m);
			}

			protected override Expression VisitMemberAccess(MemberExpression m) {
				base.VisitMemberAccess(m);					//Handle ChildRows().First().ColumnProperty
				if (m.Expression != null && typeof(Row).IsAssignableFrom(m.Expression.Type))	//Handle static members
					VisitRowProperty(GetName(m), m);

				return m;
			}

			void VisitRowProperty(string name, Expression expr) {
				var rowInfo = GetRow(GetParent(expr));

				var column = rowInfo.Schema.Columns[name];
				if (column != null) {
					if (!rowInfo.DependentColumns.Contains(column))
						rowInfo.DependentColumns.Add(rowInfo.Schema.Columns[name]);
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

			///<summary>Maps parameter expressions to the RowInfo objects for the schema that they access.</summary>
			readonly Dictionary<Expression, RowInfo> rowExpressions = new Dictionary<Expression, RowInfo>();

			///<summary>Finds the RowInfo that represents the schema accessed by a parameter, LINQ call, child rows, or parent row expression.</summary>
			///<remarks>
			/// GetRow can be called on any expression that returns a row or set of rows.
			/// Unless the expression is a ParameterExpression, it does not need to have 
			/// already been walked.  If it is a ParameterExpression, it must be in the 
			/// rowExpressions dictionary.  Parameters to LINQ lambdas are added to the
			/// dictionary by VisitMethodCall before walking the parameters; parameters
			/// to the root expression are added by the constructor.
			///</remarks>
			RowInfo GetRow(Expression expr) {
				if (!TypeContains<Row>(expr.Type))
					throw new ArgumentException("Expression must be a row: " + expr, "expr");
				expr = Unwrap(expr);	//After checking the expression's type, unwrap any casts.

				//Parameters to lambda expressions will be found in this dictionary
				RowInfo retVal;
				if (rowExpressions.TryGetValue(expr, out retVal))
					return retVal;

				var methodCall = expr as MethodCallExpression;
				if (methodCall != null && IsLinqMethod(methodCall.Method)) {
					//A LINQ method returns rows from the same 
					//schema that it was passed.
					if (methodCall.Method.Name == "Select" || methodCall.Method.Name == "SelectMany")
						throw new InvalidOperationException("Select calls cannot return rows");
					return GetRow(methodCall.Arguments[0]);
				}

				var childRow = GetRow(GetParent(expr));
				var name = GetName(expr);

				//Handle parent rows (columns of type Row)
				var key = (ForeignKeyColumn)childRow.Schema.Columns[name];
				if (key != null) {
					if (childRow.ParentRows.TryGetValue(key, out retVal))
						return retVal;
					retVal = new RowInfo(key.ForeignSchema);
					childRow.ParentRows.Add(key, retVal);
					return retVal;
				}

				//Handle child rows (ChildRows calls or typed properties)
				var relation = childRow.Schema.ChildRelations[name];
				if (relation != null) {
					if (childRow.ChildRows.TryGetValue(relation, out retVal))
						return retVal;
					retVal = new RowInfo(relation.ChildSchema);
					childRow.ChildRows.Add(relation, retVal);
					return retVal;
				}

				throw new InvalidOperationException("Unsupported row expression: " + expr);
			}

			static bool IsLinqMethod(MethodInfo method) {
				if (!method.IsStatic) return false;
				var parameters = method.GetParameters();
				if (parameters.Length < 1) return false;

				if (parameters.Count(p => typeof(IEnumerable).IsAssignableFrom(p.ParameterType)) != 1)
					return false;

				return true;
			}
			///<summary>Checks whether a type contains value(s) of a desired type.</summary>
			///<typeparam name="TDesired">The type to check for.</typeparam>
			///<param name="check">The type to check.</param>
			///<returns>True if check inherits TDesired or is a collection or array of types that inherit TDesired.</returns>
			static bool TypeContains<TDesired>(Type check) {
				if (typeof(TDesired).IsAssignableFrom(check))
					return true;

				IEnumerable<Type> interfaces = check.GetInterfaces();

				if (check.IsInterface)	//Handle raw IEnumerable<TDesired> (or derived)
					interfaces = interfaces.Concat(Enumerable.Repeat(check, 1));
				if (interfaces.Any(i =>
							i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
						 && TypeContains<TDesired>(i.GetGenericArguments().Single())
					))
					return true;
				if (check.HasElementType && TypeContains<TDesired>(check.GetElementType()))	//Handle arrays
					return true;
				return false;
			}

			///<summary>Gets the expression for the object that a field or method is invoked on.</summary>
			static Expression GetParent(Expression expr) {
				var member = expr as MemberExpression;
				if (member != null)
					return member.Expression;

				var methodCall = expr as MethodCallExpression;
				if (methodCall != null)
					return methodCall.Object;


				throw new ArgumentException("Expression does not have a parent: " + expr, "expr");
			}
			///<summary>Unwraps wrapper expressions like type casts and returns the inner expression that was wrapped.</summary>
			static Expression Unwrap(Expression expr) {
				var unary = expr as UnaryExpression;
				if (unary != null)
					return Unwrap(unary.Operand);
				//I might need to add TypeBinaryExpression
				return expr;
			}

			///<summary>Gets the column or relation name that an expressions resolves to.</summary>
			static string GetName(Expression expr) {
				//Strongly-typed property.  (Returns Row, IChildRowCollection, or scalar value)
				var member = expr as MemberExpression;
				if (member != null)
					return member.Member.Name;

				//Untyped indexer (get_Item), typed field (Field<T>), child collection (ChildRows)
				//All of these methods take a string, Column, or ChildRelation.

				//If the expression is generated by the compiler, the only supported 
				//parameter type is string, as the others would become field accesses
				//on the closure type.
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
		}
		sealed class RowInfo {
			public RowInfo(TableSchema schema) {
				Schema = schema;
				DependentColumns = new SchemaColumnCollection(schema);
				ParentRows = new Dictionary<ForeignKeyColumn, RowInfo>();
				ChildRows = new Dictionary<ChildRelation, RowInfo>();
			}

			public TableSchema Schema { get; private set; }

			public Collection<Column> DependentColumns { get; private set; }
			public Dictionary<ForeignKeyColumn, RowInfo> ParentRows { get; private set; }
			public Dictionary<ChildRelation, RowInfo> ChildRows { get; private set; }

			public RowDependencySetup CreateDependencySetup() {
				var setup = new RowDependencySetup(Schema);
				setup.DependentColumns.AddRange<Column, Column>(DependentColumns);

				setup.NestedDependencies.AddRange(ParentRows.Select(kvp =>
					new ParentRowDependency(kvp.Value.CreateDependencySetup(), kvp.Key)
				));

				setup.NestedDependencies.AddRange(ChildRows.Select(kvp =>
					new ChildRowDependency(kvp.Value.CreateDependencySetup(), kvp.Key)
				));

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
