using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>Forwards table events to RowCollectionBinders through a weak reference.</summary>
	class WeakEventForwarder {
		readonly IRowEventProvider source;
		readonly WeakReference<IRowEventClient> clientRef;

		public WeakEventForwarder(IRowEventProvider source, IRowEventClient client) {
			if (source == null) throw new ArgumentNullException("source");
			if (client == null) throw new ArgumentNullException("client");

			this.source = source;
			this.clientRef = new WeakReference<IRowEventClient>(client);

			if (source.SourceTable != null) //RowListBinders might not have tables
				source.SourceTable.LoadCompleted += SourceTable_LoadCompleted;
			source.Schema.SchemaChanged += Schema_SchemaChanged;

			source.RowAdded += source_RowAdded;
			source.ValueChanged += source_ValueChanged;
			source.RowRemoved += source_RowRemoved;
		}

		void SourceTable_LoadCompleted(object sender, EventArgs e) {
			RunOnTarget(client => client.OnLoadCompleted());
		}

		void Schema_SchemaChanged(object sender, EventArgs e) {
			RunOnTarget(client => client.OnSchemaChanged());
		}

		void source_RowAdded(object sender, RowListEventArgs e) {
			RunOnTarget(client => client.OnRowAdded(e));
		}
		void source_ValueChanged(object sender, ValueChangedEventArgs e) {
			RunOnTarget(client => client.OnValueChanged(e));
		}
		void source_RowRemoved(object sender, RowListEventArgs e) {
			RunOnTarget(client => client.OnRowRemoved(e));
		}

		///<summary>Runs an action if the client is still alive.  If not, cleans up event registrations.</summary>
		///<returns>False if the client has been collected.</returns>
		bool RunOnTarget(Action<IRowEventClient> action) {
			IRowEventClient client;
			if (clientRef.TryGetTarget(out client)) {
				action(client);
				return true;
			}

			if (source.SourceTable != null) //RowListBinders might not have tables
				source.SourceTable.LoadCompleted -= SourceTable_LoadCompleted;
			source.Schema.SchemaChanged -= Schema_SchemaChanged;

			source.RowAdded -= source_RowAdded;
			source.ValueChanged -= source_ValueChanged;
			source.RowRemoved -= source_RowRemoved;
			return false;
		}
	}
	///<summary>Receives table events from a WeakEventForwarder.</summary>
	interface IRowEventClient {
		void OnSchemaChanged();
		void OnLoadCompleted();

		void OnRowAdded(RowListEventArgs args);
		void OnValueChanged(ValueChangedEventArgs args);
		void OnRowRemoved(RowListEventArgs args);
	}
}
