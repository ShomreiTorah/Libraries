using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Manages custom and built-in behaviors for grid views and columns.</summary>
	public static class GridManager {
		///<summary>Gets the number of behavior registrations,</summary>
		///<remarks>This property is used y the grid to verify design-time
		///registrations.  See <see cref="SmartGrid.RegistrationCount"/>.</remarks>
		internal static int RegistrationCount { get { return behaviors.Count; } }

		//Since all I need is insertion and in-order traversal, a linked list is best.
		static readonly LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>> behaviors = new LinkedList<KeyValuePair<Func<object, bool>, IGridBehavior>>();

		///<summary>Registers an IGridBehavior for a Singularity schema.</summary>
		///<param name="schema">The schema to apply the behavior to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior(TableSchema schema, IGridBehavior behavior) {
			if (schema == null) throw new ArgumentNullException("schema");
			RegisterBehavior<Table>(t => t.Schema == schema, behavior);
		}

		///<summary>Registers an IGridBehavior for matching typed datasources.</summary>
		///<typeparam name="TDataSource">The type of datasource that the behavior can be applied to.</typeparam>
		///<param name="selector">A delegate that determines which datasources the behavior should be applied to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior<TDataSource>(Func<TDataSource, bool> selector, IGridBehavior behavior) where TDataSource : class {
			if (selector == null) throw new ArgumentNullException("selector");
			RegisterBehavior(o => { var t = o as TDataSource; return t != null && selector(t); }, behavior);
		}

		///<summary>Registers an IGridBehavior for matching datasources.</summary>
		///<param name="selector">A delegate that determines which datasources the behavior should be applied to.</param>
		///<param name="behavior">The IGridBehavior instance.</param>
		public static void RegisterBehavior(Func<object, bool> selector, IGridBehavior behavior) {
			if (selector == null) throw new ArgumentNullException("selector");
			if (behavior == null) throw new ArgumentNullException("behavior");

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
	}
}
