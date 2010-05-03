using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity.Dependencies {
	///<summary>A base class for a dependency of a calculated column.</summary>
	abstract class Dependency {
		protected Dependency(IDependencyClient client) {
			if (client == null) throw new ArgumentNullException("client");
			Client = client;
		}

		///<summary>Gets the calculated column that depends on this dependency.</summary>
		public IDependencyClient Client { get; private set; }

		///<summary>Indicates whether this dependency uses a table's DataContext.</summary>
		///<remarks>If this property is true, the Register method will only be called when 
		///for tables in a DataContext.</remarks>
		public bool RequiresDataContext { get; protected set; }

		///<summary>Registers event handlers for this dependency to track changes for a Table.</summary>
		public abstract void Register(Table table);
		///<summary>Unregisters event handlers registered in Register.</summary>
		public abstract void Unregister(Table table);

		protected void InvalidateValue(Row row) {
			if (row == null) return;
			Client.DependencyChanged(row);
		}
	}

	///<summary>A dependency that aggregates other dependencies.</summary>
	sealed class AggregateDependency : Dependency {
		public AggregateDependency(IDependencyClient client, IEnumerable<Dependency> dependencies)
			: base(client) {
			Dependencies = new ReadOnlyCollection<Dependency>(dependencies.ToArray());
			RequiresDataContext = Dependencies.Any(d => d.RequiresDataContext);
		}

		///<summary>Gets the dependencies aggregated by this dependency.</summary>
		public ReadOnlyCollection<Dependency> Dependencies { get; private set; }

		public override void Register(Table table) {
			foreach (var d in Dependencies)
				d.Register(table);
		}

		public override void Unregister(Table table) {
			foreach (var d in Dependencies)
				d.Unregister(table);
		}
	}


	///<summary>An object that reacts to changes in dependencies.</summary>
	interface IDependencyClient {
		void DependencyChanged(Row row);
	}
}
