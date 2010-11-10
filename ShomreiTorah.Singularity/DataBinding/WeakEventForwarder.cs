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

			if (source.SourceTable != null)	//RowListBinders might not have tables
				source.SourceTable.LoadCompleted += SourceTable_LoadCompleted;
			source.Schema.SchemaChanged += Schema_SchemaChanged;

			source.RowAdded += source_RowAdded;
			source.ValueChanged += source_ValueChanged;
			source.RowRemoved += source_RowRemoved;
		}

		void SourceTable_LoadCompleted(object sender, EventArgs e) {
			if (CheckTarget())
				clientRef.Target.OnLoadCompleted();
		}

		void Schema_SchemaChanged(object sender, EventArgs e) {
			if (CheckTarget())
				clientRef.Target.OnSchemaChanged();
		}

		void source_RowAdded(object sender, RowListEventArgs e) {
			if (CheckTarget())
				clientRef.Target.OnRowAdded(e);
		}
		void source_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (CheckTarget())
				clientRef.Target.OnValueChanged(e);
		}
		void source_RowRemoved(object sender, RowListEventArgs e) {
			if (CheckTarget())
				clientRef.Target.OnRowRemoved(e);
		}

		///<summary>Ensures that the client is still alive.</summary>
		///<returns>False if the client has been collected.</returns>
		bool CheckTarget() {
			if (clientRef.IsAlive)
				return true;

			if (source.SourceTable != null)	//RowListBinders might not have tables
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
