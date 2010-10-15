using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Manages custom and built-in behaviors for grid views and columns.</summary>
	public static class GridManager {
		//The static ctor is executed after all field initializers, so the linked list will exist
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Force deterministic initialization")]
		static GridManager() { SettingsRegistrator.EnsureRegistered(); }

		///<summary>Gets the number of behavior registrations,</summary>
		///<remarks>This property is used y the grid to verify design-time
		///registrations.  See <see cref="SmartGrid.RegistrationCount"/>.</remarks>
		internal static int RegistrationCount { get { return behaviors.Count; } }

		#region Grid Behaviors
		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>> behaviors = new LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>>();

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

		#region Column Controllers
		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<KeyValuePair<FieldPredicate, ColumnController>> columnControllers = new LinkedList<KeyValuePair<FieldPredicate, ColumnController>>();

		///<summary>Registers a column controller for a column in a Singularity schema.</summary>
		///<param name="column">The column that should use the controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the column).</param>
		public static void RegisterColumn(Column column, ColumnController controller) {
			if (column == null) throw new ArgumentNullException("column");
			RegisterColumn((ds, name) => name == column.Name && TableSchema.GetSchema(ds) == column.Schema, controller);
		}
		///<summary>Registers a column controller for fields that match a delegate.</summary>
		///<param name="selector">A delegate that indicates which datasource/column-name pairs should use this controller.</param>
		///<param name="controller">A ColumnController instance, or null to use no controller (suppressing any existing registrations matching the column).</param>
		public static void RegisterColumn(FieldPredicate selector, ColumnController controller) {
			if (selector == null) throw new ArgumentNullException("selector");
			//controller can be null.

			//By inserting each registration at the beginning of the list, I
			//override any previous registrations that match the same field.
			columnControllers.AddFirst(new KeyValuePair<FieldPredicate, ColumnController>(selector, controller));
		}

		///<summary>Gets the ColumnController instance for a field in a datasource.</summary>
		public static ColumnController GetController(object dataSource, string fieldName) {
			return columnControllers.FirstOrDefault(kvp => kvp.Key(dataSource, fieldName)).Value;
		}
		#endregion
	}
	///<summary>A method that determines whether a field in a datasource matches a condition.</summary>
	public delegate bool FieldPredicate(object dataSource, string columnName);
}
