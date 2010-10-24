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
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>A composite editor for a Person row.</summary>
	[Description("A composite editor for a Person row.")]
	[DefaultBindingProperty("Person")]
	public partial class PersonEditor : XtraUserControl {
		///<summary>Creates a new PersonEditor control.</summary>
		public PersonEditor() {
			InitializeComponent();
			bindingSource.DataSource = null;
			bindingSource.DataMember = null;
		}

		///<summary>Gets or sets the person bound to the control.</summary>
		[Description("Gets or sets the person bound to the control.")]
		[Category("Data")]
		[DefaultValue(null)]
		[Bindable(true)]
		public Person Person {
			get { return (Person)bindingSource.Current; }
			set {
				if (value == null)
					bindingSource.DataSource = null;
				else
					bindingSource.DataSource = new RowListBinder(value.Table, new Row[] { value });
			}
		}

		#region ZIP Codes
		[SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)", Justification = "Numeric")]
		private void CityTextEdit_EditValueChanged(object sender, EventArgs e) {
			if (StateComboBoxEdit.Text == "NJ") {
				switch (CityTextEdit.Text) {
					case "Passaic":
						ZipTextEdit.Text = "07055";
						break;
					case "Clifton":
						if (!ZipTextEdit.Text.StartsWith("0701"))
							ZipTextEdit.Text = "0701";
						break;
					case "Rutherford":
						ZipTextEdit.Text = "07070";
						break;
				}
			}
		}
		[SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)", Justification = "Numeric")]
		private void ZipTextEdit_EditValueChanged(object sender, EventArgs e) {
			if (ZipTextEdit.Text == "07055") {
				CityTextEdit.Text = "Passaic";
				StateComboBoxEdit.Text = "NJ";
			} else if (ZipTextEdit.Text == "07070") {
				CityTextEdit.Text = "Rutherford";
				StateComboBoxEdit.Text = "NJ";
			} else if (ZipTextEdit.Text.StartsWith("0701")	//Clifton's ZIP codes are between 07011 & 07015 (Google them)
					&& ZipTextEdit.Text.Length == 4 || (ZipTextEdit.Text[4] >= '1' && ZipTextEdit.Text[4] <= '6')) {
				CityTextEdit.Text = "Clifton";
				StateComboBoxEdit.Text = "NJ";
			}
		}
		#endregion

		#region FullName
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
		#endregion

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
