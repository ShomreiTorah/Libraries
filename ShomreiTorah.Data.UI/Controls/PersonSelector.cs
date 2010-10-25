using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Registrator;
using ShomreiTorah.Data.UI.Properties;
using ShomreiTorah.WinForms;
using ShomreiTorah.WinForms.Controls.Lookup;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>A control that allows the user to select a person from the master directory.</summary>
	public class PersonSelector : ItemSelector {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static PersonSelector() { RepositoryItemPersonSelector.Register(); }

		///<summary>Gets or sets the person selected in the lookup.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Person SelectedPerson {
			get { return (Person)base.EditValue; }
			set { base.EditValue = value; }
		}

		///<summary>Gets the editor's type name.</summary>
		public override string EditorTypeName { get { return "PersonSelector"; } }

		protected override void OnEditValueChanged() {
			base.OnEditValueChanged();
			if (SelectedPerson == null)
				SuperTip = null;
			else
				SuperTip = SelectedPerson.GetSuperTip();
		}
	}
	[UserRepositoryItem("Register")]
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
			NullValuePrompt = DefaultNullValuePrompt;
		}
		///<summary>Notifies the editor that the initialization has been completed.</summary>
		public override void EndInit() {
			base.EndInit();
			//By applying settings here, any previous settings in 
			//the designer will be overwritten.  This allows me to
			//change the settings in Data.UI and have the changes 
			//applied to existing designers.
			CreateDefaultButton();
			DisplaySettings.EditorRepository.PersonLookup.Apply(this);
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

		//If these properties are designer-serialized, all defaults will be duplicated.

		///<summary>Gets or sets the column to display in the editor when a value is selected.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ResultColumn ResultDisplayColumn {
			get { return base.ResultDisplayColumn; }
			set { base.ResultDisplayColumn = value; }
		}
		///<summary>Gets columns that display additional information about the selected item.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ItemSelectorColumnCollection AdditionalResultColumns { get { return base.AdditionalResultColumns; } }
		///<summary>Gets the columns displayed in the results grid.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ItemSelectorColumnCollection Columns { get { return base.Columns; } }
		#endregion

		public override void CreateDefaultButton() {
			Buttons.Clear();
			base.CreateDefaultButton();
			Buttons[0].SuperTip = Utilities.CreateSuperTip(body: "Click to select a person");
			Buttons.Add(new EditorButton(ButtonPredefines.Glyph) {
				SuperTip = Utilities.CreateSuperTip("New Person...", "Adds a new person to the master directory"),
				Image = Resources.Plus13,
				Width = 90,
				ImageLocation = ImageLocation.MiddleLeft,
				Caption = "New person...",
				IsDefaultButton = true
			});
		}
		protected override void RaiseButtonClick(ButtonPressedEventArgs e) {
			base.RaiseButtonClick(e);
			if (e.Button.Index == 1) {
				OwnerEdit.EditValue = AppFramework.Current.PromptPerson();
			}
		}
	}
}
