using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.DataBinding;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>A composite editor for a Person row.</summary>
	[Description("A composite editor for a Person row.")]
	public partial class PersonEditor : XtraUserControl {
		//When setting properties, you must call UpdateValue
		//Otherwise, the underlying row will not be updated.


		///<summary>Creates a new PersonEditor control.</summary>
		public PersonEditor() {
			InitializeComponent();
		}

		///<summary>Gets or sets the person bound to the control.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//[Description("Gets or sets the person bound to the control.")]
		//[Category("Data")]
		//[DefaultValue(null)]
		//[Bindable(true)]
		public Person Person {
			get { return (Person)bindingSource.Current; }
			set {
				bindingSource.DataMember = null;
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
						ZipTextEdit.UpdateValue("07055");
						break;
					case "Clifton":
						if (!ZipTextEdit.Text.StartsWith("0701"))
							ZipTextEdit.UpdateValue("0701");
						break;
					case "Rutherford":
						ZipTextEdit.UpdateValue("07070");
						break;
				}
			}
		}
		[SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)", Justification = "Numeric")]
		private void ZipTextEdit_EditValueChanged(object sender, EventArgs e) {
			if (ZipTextEdit.Text == "07055") {
				CityTextEdit.UpdateValue("Passaic");
				StateComboBoxEdit.UpdateValue("NJ");
			} else if (ZipTextEdit.Text == "07070") {
				CityTextEdit.UpdateValue("Rutherford");
				StateComboBoxEdit.UpdateValue("NJ");
			} else if (ZipTextEdit.Text.StartsWith("0701")	//Clifton's ZIP codes are between 07011 & 07015 (Google them)
					&& (ZipTextEdit.Text.Length == 4 || (ZipTextEdit.Text[4] >= '1' && ZipTextEdit.Text[4] <= '6'))) {
				CityTextEdit.UpdateValue("Clifton");
				StateComboBoxEdit.UpdateValue("NJ");
			}
		}
		#endregion

		#region FullName
		///<summary>Generates a default full name from the his-, her-, and last-name textboxes.</summary>
		///<param name="valueGetter">A function to get the value of a textbox.  This can be used to get a previous FullName.</param>
		string GenerateFullName(Func<TextEdit, String> valueGetter) {
			Func<TextEdit, bool> isEmpty = t => String.IsNullOrWhiteSpace(valueGetter(t));

			string title;
			string first;

			if (!isEmpty(HisNameTextEdit)) {
				if (!isEmpty(HerNameTextEdit))
					title = "Mr. & Mrs.";
				else
					title = "Mr.";
				first = valueGetter(HisNameTextEdit);
			} else if (!isEmpty(HerNameTextEdit)) {
				title = "Mrs.";
				first = valueGetter(HerNameTextEdit);
			} else if (!isEmpty(LastNameTextEdit))
				return valueGetter(LastNameTextEdit) + " Family";
			else
				return null;

			return (title + " " + first + " " + valueGetter(LastNameTextEdit)).Trim();
		}
		readonly Dictionary<TextEdit, string> oldTexts = new Dictionary<TextEdit, string>();
		void UpdateFullName(TextEdit changedEdit) {
			if (oldTexts.Count == 0) {	//This happens in an EndInit call
				UpdateOldTexts();
				return;
			}

			var newFullName = GenerateFullName(e => e.Text);
			var oldFullName = GenerateFullName(e => oldTexts[e]);
			UpdateOldTexts();

			if (String.IsNullOrWhiteSpace(FullNameTextEdit.Text)
			 || FullNameTextEdit.Text == oldFullName) {
				FullNameTextEdit.UpdateValue(newFullName);
				return;
			}

			var oldName = changedEdit.OldEditValue as string;	//Can be DBNull
			if (!String.IsNullOrWhiteSpace(oldName))
				FullNameTextEdit.UpdateValue(FullNameTextEdit.Text.Replace(oldName, changedEdit.Text));
		}


		private void SingleNameEdit_KeyUp(object sender, KeyEventArgs e) {
			UpdateFullName((TextEdit)sender);
		}
		private void SingleNameEdit_EditValueChanged(object sender, EventArgs e) {
			UpdateFullName((TextEdit)sender);
			UpdateOldTexts();
		}
		void UpdateOldTexts() {
			oldTexts[HisNameTextEdit] = HisNameTextEdit.Text;
			oldTexts[HerNameTextEdit] = HerNameTextEdit.Text;
			oldTexts[LastNameTextEdit] = LastNameTextEdit.Text;
		}
		#endregion
	}
	///<summary>A component that binds to a dummy DataContext for use in designers in libraries.</summary>
	class DesignerBinder : BindableDataContextBase {
		protected override DataContext FindDataContext() {
			//DesignerBinders should not be used in real programs.
			//However, they will still be used by library controls
			//InitializeComponents, so they must work in programs.
			//If the program has calculated columns that reference
			//other tables, we won't be able to easily create this
			//DataContext.  Therefore, I use the AppFramework when
			//available.
			if (AppFramework.Current != null)	
				return AppFramework.Current.DataContext;

			var context = new DataContext();
			context.Tables.AddTable(Person.CreateTable());
			return context;
		}
	}
}
