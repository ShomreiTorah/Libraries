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
	[Description("A control that allows the user to select a person from the master directory.")]
	public class PersonSelector : ItemSelector {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static PersonSelector() { RepositoryItemPersonSelector.Register(); }

		///<summary>Gets or sets the person selected in the lookup.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Person SelectedPerson {
			get { return base.EditValue as Person; }
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
		///<summary>Prompts the user to create a new person.</summary>
		protected internal virtual Person PromptNew() { return AppFramework.Current.PromptPerson(); }

		///<summary>Gets settings specific to the PersonSelector.</summary>
		[Category("Properties")]
		[Description("Gets an object containing properties, methods and events specific to the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new RepositoryItemPersonSelector Properties { get { return (RepositoryItemPersonSelector)base.Properties; } }

		///<summary>Occurs before a person is selecting using the UI.</summary>
		[Description("Occurs before a person is selecting using the UI.")]
		[Category("Data")]
		public event EventHandler<PersonSelectingEventArgs> PersonSelecting {
			add { Properties.PersonSelecting += value; }
			remove { Properties.PersonSelecting -= value; }
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
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new PersonSelector OwnerEdit { get { return (PersonSelector)base.OwnerEdit; } }
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
		//Set by EditorRepository.PersonLookup
		public new Image SelectionIcon {
			get { return base.SelectionIcon; }
			set { base.SelectionIcon = value; }
		}
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
		///<summary>Raises the ButtonClick event.</summary>
		protected override void RaiseButtonClick(ButtonPressedEventArgs e) {
			base.RaiseButtonClick(e);
			if (e.Button.Index == 1) {
				var person = OwnerEdit.PromptNew();
				if (person != null && !RaisePersonSelecting(person, PersonSelectionReason.Created))
					return;
				OwnerEdit.EditValue = person;
			}
		}
		///<summary>Raises the PersonSelecting event.</summary>
		///<returns>False if the event was cancelled.</returns>
		bool RaisePersonSelecting(Person person, PersonSelectionReason method) {
			var args = new PersonSelectingEventArgs(person, method);
			OnPersonSelecting(args);
			return !args.Cancel;
		}

		///<summary>Raises the ItemSelecting event.</summary>
		protected override void OnItemSelecting(ItemSelectingEventArgs e) {
			base.OnItemSelecting(e);
			var args = new PersonSelectingEventArgs((Person)e.NewItem, PersonSelectionReason.ResultClick) { Cancel = e.Cancel };
			OnPersonSelecting(args);
			e.Cancel = args.Cancel;
		}


		///<summary>Occurs before a person is selecting using the UI.</summary>
		public event EventHandler<PersonSelectingEventArgs> PersonSelecting;
		///<summary>Raises the PersonSelecting event.</summary>
		///<param name="e">A PersonSelectingEventArgs object that provides the event data.</param>
		internal protected virtual void OnPersonSelecting(PersonSelectingEventArgs e) {
			if (PersonSelecting != null)
				PersonSelecting(this, e);
		}
	}
	///<summary>Provides information for the PersonSelecting event.</summary>
	public class PersonSelectingEventArgs : CancelEventArgs {
		///<summary>Creates a PersonSelectingEventArgs instance.</summary>
		public PersonSelectingEventArgs(Person person, PersonSelectionReason method) {
			if (person == null) throw new ArgumentNullException("person");
			Person = person;
			Method = method;
		}

		///<summary>Gets the person that was selected.</summary>
		public Person Person { get; private set; }
		///<summary>Gets the way in which the person was selected.</summary>
		public PersonSelectionReason Method { get; private set; }
	}
	///<summary>Represents the reason that a person was selected.</summary>
	public enum PersonSelectionReason {
		///<summary>The person was clicked in the results grid.</summary>
		ResultClick,
		///<summary>The person was created using the New person dialog.</summary>
		Created
	}
}
