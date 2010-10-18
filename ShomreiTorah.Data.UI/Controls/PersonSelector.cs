using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.WinForms.Controls.Lookup;
using DevExpress.XtraEditors.Controls;
using DevExpress.Utils;
using ShomreiTorah.Data.UI.Properties;
using System.ComponentModel;
using DevExpress.XtraEditors.Registrator;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
namespace ShomreiTorah.Data.UI.Controls {
	///<summary>A control that allows the user to select a person from the master directory.</summary>
	public class PersonSelector : ItemSelector {
		///<summary>Gets or sets the person selected in the lookup.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Person SelectedPerson {
			get { return (Person)base.EditValue; }
			set { base.EditValue = value; }
		}

		///<summary>Gets the editor's type name.</summary>
		public override string EditorTypeName { get { return "PersonSelector"; } }
	}
	public class RepositoryItemPersonSelector : RepositoryItemItemSelector {
		#region Editor Registration
		static RepositoryItemPersonSelector() { Register(); }
		///<summary>Registers the RepositoryItem.</summary>
		public static new void Register() {
			RegisterDerived(typeof(PersonSelector), typeof(RepositoryItemPersonSelector));
		}
		///<summary>Gets the owning editor's type name.</summary>
		public override string EditorTypeName { get { return "PersonSelector"; } }
		///<summary>Gets the owning ItemSelector.</summary>
		public new ItemSelector OwnerEdit { get { return (PersonSelector)base.OwnerEdit; } }
		#endregion

		const string DefaultNullValuePrompt = "Click here to select a person, or type to search";

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "NullValuePrompt overrides should be trivial")]
		public RepositoryItemPersonSelector() {
			SelectionIcon = Resources.People16;
			NullValuePrompt = DefaultNullValuePrompt;
			//TODO: Columns
		}

		#region Property overrides
		///<summary>Gets or sets the text displayed grayed out when the editor doesn't have focus, and its edit value is not set to a valid value.</summary>
		[Description("Gets or sets the text displayed grayed out when the editor doesn't have focus, and its edit value is not set to a valid value.")]
		[Category("Behavior")]
		[DefaultValue(DefaultNullValuePrompt)]
		[Localizable(true)]
		public override string NullValuePrompt {
			get { return base.NullValuePrompt; }
			set { base.NullValuePrompt = value; }
		}
		///<summary>Gets or sets the icon displayed in the editor when an item is selected.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Image SelectionIcon {
			get { return base.SelectionIcon; }
			set { base.SelectionIcon = value; }
		}

		///<summary>Gets or sets the column to display in the editor when a value is selected.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ResultColumn ResultDisplayColumn {
			get { return base.ResultDisplayColumn; }
			set { base.ResultDisplayColumn = value; }
		}
		#endregion

		public override void CreateDefaultButton() {
			base.CreateDefaultButton();
			var superTip = new SuperToolTip();
			superTip.Items.AddTitle("New Person...");
			superTip.Items.Add("Adds a new person to the master directory");
			Buttons.Add(new EditorButton(ButtonPredefines.Glyph, Resources.Plus13, superTip));
		}

		protected override void RaiseButtonClick(ButtonPressedEventArgs e) {
			base.RaiseButtonClick(e);
			if (e.Button.Index == 1) {
			}
		}
	}
}
