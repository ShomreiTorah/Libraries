using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Data.UI {
	///<summary>Automatically saves a <see cref="DataSyncContext"/> after the data is changed.</summary>
	public class AutoSaver : IDisposable {
		///<summary>Gets the sync context to save changes to.</summary>
		public DataSyncContext SyncContext => App.SyncContext;
		///<summary>Gets the application to save changes with.</summary>
		public AppFramework App { get; }

		readonly Timer timer = new Timer { Interval = 30000 };
		bool isListening;

		///<summary>Creates an AutoSaver that saves for the specified application.</summary>
		public AutoSaver(AppFramework app) {
			App = app;
			timer.Tick += Timer_Tick;
		}

		private void Timer_Tick(object sender, EventArgs e) {
			timer.Stop();
			AppFramework.Current.SaveDatabase();
		}


		///<summary>Indicates whether the AutoSaver is currently listening for change events to save.</summary>
		public bool IsListening {
			get { return isListening; }

			set {
				if (value)
					Start();
				else
					Stop();
			}
		}

		///<summary>Starts watching for changes.</summary>
		public void Start() {
			if (isListening) return;
			SyncContext.DataContext.Tables.TableAdded += Tables_TableAdded;
			foreach (var table in SyncContext.DataContext.Tables) {
				table.RowAdded += Table_Changed;
				table.ValueChanged += Table_Changed;
				table.RowRemoved += Table_Changed;
			}
			isListening = true;
		}

		///<summary>Stops watching for changes.</summary>
		public void Stop() {
			if (!isListening) return;
			timer.Stop();
			SyncContext.DataContext.Tables.TableAdded -= Tables_TableAdded;
			foreach (var table in SyncContext.DataContext.Tables) {
				table.RowAdded -= Table_Changed;
				table.ValueChanged -= Table_Changed;
				table.RowRemoved -= Table_Changed;
			}
			isListening = false;
		}

		private void Tables_TableAdded(object sender, TableEventArgs e) {
			e.Table.RowAdded += Table_Changed;
			e.Table.ValueChanged += Table_Changed;
			e.Table.RowRemoved += Table_Changed;
		}

		private void Table_Changed(object sender, EventArgs e) {
			timer.Stop();
			timer.Start();
		}

		///<summary>Releases all resources used by the AutoSaver.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the AutoSaver and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				timer.Dispose();
			}
		}
	}
}
