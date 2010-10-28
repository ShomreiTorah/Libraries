using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Manages custom and built-in behaviors for grid views and columns.</summary>
	public static class GridManager {
		//The static ctor is executed after all field initializers, so the linked list will exist
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Force deterministic initialization")]
		static GridManager() { SettingsRegistrator.EnsureRegistered(); }

		///<summary>Gets the number of behavior registrations,</summary>
		///<remarks>This property is used by the grid to verify design-time
		///registrations.  See <see cref="SmartGrid.RegistrationCount"/>.</remarks>
		internal static int RegistrationCount {
			get { return behaviors.Count + columnControllers.Count + columnSuppressors.Count + EditorRepository.RegistrationCount; }
		}

		#region Grid Behaviors
		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>> behaviors = new LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>>();

		///<summary>Registers an IGridBehavior for one or more Singularity schemas.</summary>
		///<param name="schema"s>The schemas to apply the behavior to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior(IEnumerable<TableSchema> schemas, IGridBehavior behavior) {
			if (schemas == null) throw new ArgumentNullException("schemas");
			RegisterBehavior(t => schemas.Contains(TableSchema.GetSchema(t)), behavior);
		}
		///<summary>Registers an IGridBehavior for a Singularity schema.</summary>
		///<param name="schema">The schema to apply the behavior to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior(TableSchema schema, IGridBehavior behavior) {
			if (schema == null) throw new ArgumentNullException("schema");
			RegisterBehavior(t => TableSchema.GetSchema(t) == schema, behavior);
		}

		///<summary>Registers an IGridBehavior for matching datasources.</summary>
		///<param name="selector">A delegate that determines which datasources the behavior should be applied to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior(Func<object, bool> selector, IGridBehavior behavior) {
			if (selector == null) throw new ArgumentNullException("selector");
			if (behavior == null) throw new ArgumentNullException("behavior");
			UIThread.Verify();

			behaviors.AddFirst(new KeyValuePair<Func<object, bool>, IGridBehavior>(selector, behavior));
		}

		//If two IGridBehaviors of the same type match the same
		//datasource, only the one added last will be used.  To
		//do this, I maintain the behaviors set backwards, then
		//call Distinct with a custom IEqualityComparer before 
		//returning matching behaviors.  The Distinct() method 
		//will return the first of each set of duplicates from 
		//the original.  I cannot maintain the uniqueness when 
		//adding the behaviors because I can't compare selector
		//delegates.  This allows specific programs to override
		//the built-in default schema settings.
		//This means that behaviors will be applied in reverse.
		//I want behaviors to be completely independent, so I'm
		//keeping this behavior.  To change it, call .Reverse()
		//after .Distinct().

		///<summary>Gets the grid behaviors that should be applied to the given datasource.</summary>
		public static IEnumerable<IGridBehavior> GetBehaviors(object dataSource) {
			if (dataSource == null) throw new ArgumentNullException("dataSource");
			UIThread.Verify();

			return behaviors.Where(kvp => kvp.Key(dataSource))
							.Select(kvp => kvp.Value)
							.Distinct(new TypeComparer<IGridBehavior>());
		}
		class TypeComparer<T> : IEqualityComparer<T> where T : class {
			public bool Equals(T x, T y) {
				if (x == y) return true;
				if (x == null || y == null) return false;
				return x.GetType() == y.GetType();
			}

			public int GetHashCode(T obj) {
				if (obj == null) return int.MinValue;
				return obj.GetType().GetHashCode();
			}
		}
		#endregion

		#region Automatic DataSource
		///<summary>Gets the default datasource, which wraps the AppFramework's DataContext.</summary>
		public static object DefaultDataSource { get { return AutoDataSource.Instance; } }

		sealed class AutoDataSource : IListSource, IComponent {
			public static readonly AutoDataSource Instance = new AutoDataSource();
			readonly IListSource inner;
			private AutoDataSource() {
				inner = AppFramework.Current.DataContext;
				Site = new NamerSite(this, AppFramework.Current.GetType() + ".DataContext");
			}

			public bool ContainsListCollection { get { return inner.ContainsListCollection; } }
			public IList GetList() { return inner.GetList(); }

			public override string ToString() { return Site.Name; }

			//I implement IComonent and ISite to get the name in the Properties window.
			public void Dispose() { }
			public event EventHandler Disposed { add { } remove { } }
			public ISite Site { get; set; }
			class NamerSite : ISite {
				public NamerSite(IComponent component, string name) {
					Component = component;
					Name = name;
				}
				public string Name { get; set; }
				public IComponent Component { get; private set; }
				public IContainer Container { get { return null; } }

				public bool DesignMode { get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; } }
				public object GetService(Type serviceType) { return LicenseManager.CurrentContext.GetService(serviceType); }
			}
		}
		#endregion

		#region Column Controllers
		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<KeyValuePair<Func<SmartGridColumn, bool>, ColumnController>> columnControllers = new LinkedList<KeyValuePair<Func<SmartGridColumn, bool>, ColumnController>>();

		///<summary>Registers a column controller for a column in a Singularity schema.</summary>
		///<param name="column">The column that should use the controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the column).</param>
		public static void RegisterColumn(Column column, ColumnController controller) {
			if (column == null) throw new ArgumentNullException("column");
			RegisterColumn(gc => gc.FieldName == column.Name && gc.View.GetSourceSchema() == column.Schema, controller);
		}
		///<summary>Registers a column controller for Singularity columns that match a predicate.</summary>
		///<param name="columnPredicate">The delegate that determines which Singularity columns should use the controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the columns).</param>
		public static void RegisterColumn(Func<Column, bool> columnPredicate, ColumnController controller) {
			if (columnPredicate == null) throw new ArgumentNullException("columnPredicate");

			RegisterColumn(
				gc => {
					var column = gc.GetSchemaColumn();
					if (column == null) return false;
					return columnPredicate(column);
				},
				controller
			);
		}
		///<summary>Registers a column controller for one or more columns in Singularity schemas.</summary>
		///<param name="columns">The columns that should use the controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the columns).</param>
		public static void RegisterColumns(IEnumerable<Column> columns, ColumnController controller) {
			if (columns == null) throw new ArgumentNullException("columns");
			RegisterColumn(
				gc => columns.Any(c => gc.FieldName == c.Name && gc.View.GetSourceSchema() == c.Schema),
				controller
			);
		}
		///<summary>Registers a column controller for fields that match a delegate.</summary>
		///<param name="selector">A delegate that indicates which datasource/field-name pairs should use this controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the fields).</param>
		public static void RegisterColumn(Func<SmartGridColumn, bool> selector, ColumnController controller) {
			if (selector == null) throw new ArgumentNullException("selector");
			//controller can be null.

			//By inserting each registration at the beginning of the list, I
			//override any previous registrations that match the same field.
			columnControllers.AddFirst(new KeyValuePair<Func<SmartGridColumn, bool>, ColumnController>(selector, controller));
		}

		///<summary>Gets the ColumnController instance for a field in a datasource.</summary>
		public static ColumnController GetController(SmartGridColumn gridColumn) {
			return columnControllers.FirstOrDefault(kvp => kvp.Key(gridColumn)).Value;
		}
		#endregion

		#region Column Suppression
		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<Func<SmartGridColumn, bool>> columnSuppressors = new LinkedList<Func<SmartGridColumn, bool>>();

		///<summary>Prevents columns in Singularity schemas from being automatically added to a SmartGridView.</summary>
		public static void SuppressColumns(params Column[] columns) {
			if (columns == null) throw new ArgumentNullException("columns");
			foreach (var column in columns) SuppressColumn(column);
		}
		///<summary>Prevents a column in a Singularity schema from being automatically added to a SmartGridView.</summary>
		public static void SuppressColumn(Column column) {
			if (column == null) throw new ArgumentNullException("column");
			SuppressColumn(c => c.View.GetSourceSchema() == column.Schema && c.FieldName == column.Name);
		}
		///<summary>Prevents columns matching a predicate from being automatically added to a SmartGridView.</summary>
		public static void SuppressColumn(Func<SmartGridColumn, bool> predicate) {
			if (predicate == null) throw new ArgumentNullException("predicate");
			columnSuppressors.AddLast(predicate);
		}

		///<summary>Checks whether a column should be suppressed from automatic population.</summary>
		public static bool IsSuppressed(SmartGridColumn column) {
			if (column == null) throw new ArgumentNullException("column");
			return columnSuppressors.Any(s => s(column));
		}
		#endregion

		#region Schema Extension Methods
		///<summary>Gets the Singularity column bound to  grid column, if any.</summary>
		public static Column GetSchemaColumn(this GridColumn gridColumn) {
			var schema = gridColumn.View.GetSourceSchema();
			if (schema == null)
				return null;
			return schema.Columns[gridColumn.FieldName];
		}
		///<summary>Gets the TableSchema for the table bound to a grid view.</summary>
		///<remarks>This method even supports detail pattern views at design
		///time.  (Even though they don't have a data source)</remarks>
		public static TableSchema GetSourceSchema(this BaseView view) {
			if (view == null) throw new ArgumentNullException("view");

			if (view.DataSource != null)
				return TableSchema.GetSchema(view.DataSource);
			if (view == view.GridControl.MainView)	//And there's no datasource
				return null;

			return view.GetLevelNode().GetSchema();

			//var parentSchema = view.ParentView.GetSourceSchema();
			//if (parentSchema != null)
			//    return parentSchema.ChildRelations[view.LevelName].ChildSchema;

			//return null;
		}
		///<summary>Gets the GridLevelNode object for a view's level in its grid's detail tree.</summary>
		public static GridLevelNode GetLevelNode(this BaseView view) {
			if (view == null) throw new ArgumentNullException("view");

			return view.GridControl.LevelTree.Find(view);
		}

		///<summary>Gets the schema bound to a view's parent view.</summary>
		public static TableSchema GetParentSchema(this BaseView view) {
			if (view.ParentView != null)
				return view.ParentView.GetSourceSchema();

			var parentLevel = view.GetLevelNode().Parent;
			if (parentLevel == null) return null;

			return parentLevel.GetSchema();
		}

		///<summary>Gets the TableSchema bound to a node in a grid's level tree.</summary>
		public static TableSchema GetSchema(this GridLevelNode node) {
			if (node == null) throw new ArgumentNullException("node");

			var childPath = new Stack<string>();
			while (!node.IsRootLevel) {
				childPath.Push(node.RelationName);
				node = node.Parent;
			}

			//Node now refers to the root node.
			var schema = GridGetter(node).MainView.GetSourceSchema();
			while (childPath.Count > 0) {
				var cr = schema.ChildRelations[childPath.Pop()];
				if (cr == null) return null;
				schema = cr.ChildSchema;
			}

			return schema;
		}

		static Func<GridLevelNode, GridControl> CreateGridGetter() {
			var parameter = Expression.Parameter(typeof(GridLevelNode));
			return Expression.Lambda<Func<GridLevelNode, GridControl>>(Expression.Property(parameter, "Grid"), parameter).Compile();
		}
		static readonly Func<GridLevelNode, GridControl> GridGetter = CreateGridGetter();
		#endregion
	}
}
