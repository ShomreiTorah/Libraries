using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using DevExpress.Accessibility;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using Microsoft.Win32;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>An editor that allows the user to select an email address to send previews to.</summary>
	[Description("An editor that allows the user to select an email address to send previews to.")]
	[ToolboxBitmap(typeof(BaseEdit), "Bitmaps256.MRUEdit.bmp")]
	public class PreviewAddressEdit : MRUEdit {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static PreviewAddressEdit() { RepositoryItemPreviewAddressEdit.Register(); }
		///<summary>Gets the editor's type name.</summary>
		[Browsable(false)]
		public override string EditorTypeName { get { return "PreviewAddressEdit"; } }

		internal const string registryPath = @"HKEY_CURRENT_USER\Software\Shomrei Torah\";

		///<summary>Gets a PreviewEmail-related value from the registry.</summary>
		///<remarks>These values used to live under the Billing subkey; this method also checks the old location.</remarks>
		internal static object GetValue(string name) {
			return Registry.GetValue(registryPath, name, null)
				?? Registry.GetValue(@"HKEY_CURRENT_USER\Software\Shomrei Torah\Billing\", name, null);
		}

		///<summary>Gets or sets the text displayed in the edit box.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text { get { return base.Text; } set { base.Text = value; } }
		///<summary>Gets or sets the value of the editor.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override object EditValue { get { return base.EditValue; } set { base.EditValue = value; } }

		///<summary>Called when the editor is loaded.</summary>
		protected override void OnLoaded() {
			base.OnLoaded();
			Text = DefaultAddress;
		}

		///<summary>Gets the user's default email address from the registry.</summary>
		public static string DefaultAddress { get { return GetValue("LastPreviewEmail") as string; } }

		///<summary>Gets the email address that the user selected.</summary>
		public MailAddress Address { get; private set; }

		///<summary>Raises the Validating event.</summary>
		protected override void OnValidating(CancelEventArgs e) {
			base.OnValidating(e);
			CheckPreviewAddress();
			e.Cancel = !String.IsNullOrEmpty(Text) && Address == null;	//If we failed to parse a non-empty string, throw an error
			ErrorText = e.Cancel ? "Invalid email address" : null;
		}
		///<summary>Raises the EditValueChanged event.</summary>
		protected override void OnEditValueChanged() {
			base.OnEditValueChanged();
			CheckPreviewAddress();
		}
		void CheckPreviewAddress() {
			if (String.IsNullOrEmpty(Text)) {
				Address = null;
			} else {
				try {
					Address = new MailAddress(Text, "Preview: " + Environment.UserName);
				} catch (FormatException) { Address = null; }
			}
			if (!String.IsNullOrEmpty(Text))
				Registry.SetValue(registryPath, "LastPreviewEmail", Text, RegistryValueKind.String);
		}

		///<summary>Destroys the handle associated with the control.</summary>
		protected override void DestroyHandle() {
			base.DestroyHandle();
		}
	}
	///<summary>Stores settings specific to the PreviewAddressEdit control.</summary>
	[UserRepositoryItem("Register")]
	public class RepositoryItemPreviewAddressEdit : RepositoryItemMRUEdit {
		#region Editor Registration
		static RepositoryItemPreviewAddressEdit() { Register(); }
		///<summary>Registers the RepositoryItem.</summary>
		public static void Register() {
			EditorRegistrationInfo.Default.Editors.Add(
				new EditorClassInfo(
					"PreviewAddressEdit",
					typeof(PreviewAddressEdit),
					typeof(RepositoryItemPreviewAddressEdit),
					typeof(MRUEditViewInfo),
					new ButtonEditPainter(),
					true, EditImageIndexes.MRUEdit,
					typeof(ComboBoxEditAccessible)
				)
			);
		}

		///<summary>Gets the owning editor's type name.</summary>
		[Browsable(false)]
		public override string EditorTypeName { get { return "PreviewAddressEdit"; } }
		///<summary>Gets the owning CheckNumberEdit.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new PreviewAddressEdit OwnerEdit { get { return (PreviewAddressEdit)base.OwnerEdit; } }
		#endregion

		///<summary>Gets the collection of items displayed by the current MRU editor.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new MRUEditItemCollection Items { get { return base.Items; } }

		bool isInitializing = true;		//Don't update the registry from the ctor

		///<summary>Creates a new RepositoryItemPreviewAddressEdit.</summary>
		public RepositoryItemPreviewAddressEdit() {
			var recentEmails = (string[])PreviewAddressEdit.GetValue("RecentPreviewEmails");
			if (recentEmails != null)
				Items.AddRange(recentEmails);
			isInitializing = false;
		}
		///<summary>Raises the AddingMRUItem event.</summary>
		protected override void RaiseAddingMRUItem(AddingMRUItemEventArgs e) {
			if (!(e.Item is MailAddress)) {
				try {
					new MailAddress(e.Item as string).ToString();
				} catch (FormatException) { e.Cancel = true; }
			}
			base.RaiseAddingMRUItem(e);
		}
		///<summary>Called when the Items collection changes.</summary>
		protected override void OnItems_CollectionChanged(object sender, CollectionChangeEventArgs e) {
			base.OnItems_CollectionChanged(sender, e);
			if (Items.Count > 0 && !isInitializing)
				Registry.SetValue(PreviewAddressEdit.registryPath, "RecentPreviewEmails", Items.OfType<string>().ToArray(), RegistryValueKind.MultiString);
		}
	}
}
