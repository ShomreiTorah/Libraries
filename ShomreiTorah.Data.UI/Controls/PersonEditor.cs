using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;
using ShomreiTorah.Singularity.DataBinding;

namespace ShomreiTorah.Data.UI.Controls {
	public partial class PersonEditor : XtraUserControl {
		public PersonEditor() {
			InitializeComponent();
		}
	}
	class DataBinderContext : DataContext {
		public DataBinderContext() {
			DisplaySettings.SettingsRegistrator.EnsureRegistered();

			Tables.AddTable(Person.CreateTable());

			//var syncer = new DataSyncContext(this, new SqlServerSqlProvider(DB.Default));
			//syncer.Tables.AddPrimaryMappings();
			//syncer.ReadData();
		}
	}
	class ContextBinder : BindableDataContextBase<DataBinderContext> {
		protected override DataBinderContext FindDataContext() {
			return new DataBinderContext();
		}
	}
}
