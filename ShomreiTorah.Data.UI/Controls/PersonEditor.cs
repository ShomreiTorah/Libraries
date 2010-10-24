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

		bool hasCustomFullName;
		private void SingleName_Changing(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e) {
			hasCustomFullName = !String.IsNullOrWhiteSpace(FullNameTextEdit.Text) && FullNameTextEdit.Text != GenerateFullName();
		}
		string GenerateFullName() {
			string title;
			string first;

			if (!String.IsNullOrWhiteSpace(HisNameTextEdit.Text)) {
				if (!String.IsNullOrWhiteSpace(HerNameTextEdit.Text))
					title = "Mr. & Mrs.";
				else
					title = "Mr.";
				first = HisNameTextEdit.Text;
			} else if (!String.IsNullOrWhiteSpace(HerNameTextEdit.Text)) {
				title = "Mrs.";
				first = HisNameTextEdit.Text;
			} else if (!String.IsNullOrWhiteSpace(LastNameTextEdit.Text))
				return LastNameTextEdit.Text + " Family";
			else
				return null;

			return (title + " " + first + " " + LastNameTextEdit.Text).Trim();
		}
		void UpdateFullName(TextEdit changedEdit) {
			if (!hasCustomFullName) {
				FullNameTextEdit.Text = GenerateFullName();
				return;
			}
			var oldName = (string)changedEdit.OldEditValue;
			if (!String.IsNullOrWhiteSpace(oldName))
				FullNameTextEdit.Text = FullNameTextEdit.Text.Replace(oldName, changedEdit.Text);
		}

		private void HisNameTextEdit_EditValueChanged(object sender, EventArgs e) {
			UpdateFullName(HisNameTextEdit);
		}

		private void HerNameTextEdit_EditValueChanged(object sender, EventArgs e) {
			UpdateFullName(HerNameTextEdit);
		}
		private void LastNameTextEdit_EditValueChanged(object sender, EventArgs e) {
			UpdateFullName(LastNameTextEdit);
		}
	}
	class DataBinderContext : DataContext {
		public DataBinderContext() {
			DisplaySettings.SettingsRegistrator.EnsureRegistered();

			Tables.AddTable(Person.CreateTable());
		}
	}
	class ContextBinder : BindableDataContextBase<DataBinderContext> {
		protected override DataBinderContext FindDataContext() {
			return new DataBinderContext();
		}
	}
}
