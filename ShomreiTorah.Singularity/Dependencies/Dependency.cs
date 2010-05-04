using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A base class for a dependency of a calculated column.</summary>
	public abstract class Dependency {
		///<summary>Creates a new Dependency.</summary>
		protected Dependency() { }

		///<summary>Indicates whether this dependency uses a table's DataContext.</summary>
		///<remarks>If this property is true, the Register method will only be called when 
		///for tables in a DataContext.</remarks>
		public bool RequiresDataContext { get; protected set; }

		///<summary>Registers event handlers for this dependency to track changes for a table.</summary>
		public abstract void Register(Table table);
		///<summary>Unregisters event handlers registered in Register.</summary>
		public abstract void Unregister(Table table);

		///<summary>Informs the dependency's client that a dependent value has changed.</summary>
		protected void OnRowInvalidated(Row row) { OnRowInvalidated(new RowEventArgs(row)); }

		///<summary>Occurs when a row's dependencies have changed.</summary>
		public event EventHandler<RowEventArgs> RowInvalidated;
		///<summary>Raises the RowInvalidated event.</summary>
		///<param name="e">A RowEventArgs object that provides the event data.</param>
		internal protected virtual void OnRowInvalidated(RowEventArgs e) {
			if (RowInvalidated != null)
				RowInvalidated(this, e);
		}

	}

	///<summary>A dependency that aggregates other dependencies.</summary>
	public sealed class AggregateDependency : Dependency {
		///<summary>Creates a new AggregateDependency.</summary>
		public AggregateDependency(IEnumerable<Dependency> dependencies) {
			Dependencies = new ReadOnlyCollection<Dependency>(dependencies.ToArray());
			RequiresDataContext = Dependencies.Any(d => d.RequiresDataContext);
		}

		///<summary>Gets the dependencies aggregated by this dependency.</summary>
		public ReadOnlyCollection<Dependency> Dependencies { get; private set; }

		///<summary>Registers event handlers for this dependency to track changes for a table.</summary>
		public override void Register(Table table) {
			foreach (var d in Dependencies)
				d.Register(table);
		}

		///<summary>Unregisters event handlers registered in Register.</summary>
		public override void Unregister(Table table) {
			foreach (var d in Dependencies)
				d.Unregister(table);
		}
	}
}
