using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Manages custom and built-in behaviors for grid views and columns.</summary>
	public static class GridManager {
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

			behaviors.AddLast(new KeyValuePair<Func<object, bool>, IGridBehavior>(selector, behavior));
		}

		///<summary>Gets the grid behaviors that should be applied to the given datasource.</summary>
		public static IEnumerable<IGridBehavior> GetBehaviors(object dataSource) {
			if (dataSource == null) throw new ArgumentNullException("dataSource");

			return behaviors.Where(kvp => kvp.Key(dataSource)).Select(kvp => kvp.Value);
		}
	}
}
