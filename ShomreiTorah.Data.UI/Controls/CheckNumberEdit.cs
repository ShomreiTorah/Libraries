using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors;
using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Drawing;
using DevExpress.Accessibility;
using System.Diagnostics.CodeAnalysis;
using ShomreiTorah.WinForms;
using System.Globalization;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>An editor for the check number field that warns about duplicate check numbers.</summary>
	[Description("An editor for the check number field that warns about duplicate check numbers.")]
	[ToolboxBitmap(typeof(BaseEdit), "Bitmaps256.TextEdit.bmp")]
	public class CheckNumberEdit : TextEdit {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static CheckNumberEdit() { RepositoryItemCheckNumberEdit.Register(); }
		///<summary>Gets the editor's type name.</summary>
		public override string EditorTypeName { get { return "CheckNumberEdit"; } }

		///<summary>Gets the Payment row that this editor is bound to, if any.</summary>
		private Payment BoundRow {
			get {
				if (BindingManager.Count == 0) return null;
				return BindingManager.Current as Payment;
			}
		}
		///<summary>Raises the Enter event.</summary>
		protected override void OnEnter(EventArgs e) {
			base.OnEnter(e);
			lastWarnedValue = null;
		}
		string lastWarnedValue;
		///<summary>Raises the Validating event.</summary>
		protected override void OnValidating(CancelEventArgs e) {
			base.OnValidating(e);
			if (e.Cancel) return;
			if (lastWarnedValue == Text || String.IsNullOrWhiteSpace(Text)) return;
			var payment = BoundRow;
			if (payment == null || payment.Person == null) return;

			var duplicate = payment.Person.Payments
				.FirstOrDefault(p => p != payment && String.Equals(p.CheckNumber, Text, StringComparison.CurrentCultureIgnoreCase));
			if (duplicate == null) return;

			lastWarnedValue = Text;
			e.Cancel = !Dialog.Warn(String.Format(CultureInfo.CurrentCulture, "{0} #{1} for {2} has already been entered ({3:d}, {4:c}).\r\nAre you sure you aren't making a mistake?",
																			  duplicate.Method, duplicate.CheckNumber, duplicate.Person.FullName, duplicate.Date, duplicate.Amount));
		}
	}

	///<summary>Holds settings for a CheckNumberEdit.</summary>
	[UserRepositoryItem("Register")]
	public class RepositoryItemCheckNumberEdit : RepositoryItemTextEdit {
		#region Editor Registration
		static RepositoryItemCheckNumberEdit() { Register(); }
		///<summary>Registers the RepositoryItem.</summary>
		public static void Register() {
			EditorRegistrationInfo.Default.Editors.Add(
			new EditorClassInfo(
				"CheckNumberEdit",
				typeof(CheckNumberEdit),
				typeof(RepositoryItemCheckNumberEdit),
				typeof(TextEditViewInfo),
				new TextEditPainter(),
				true, EditImageIndexes.TextEdit,
				typeof(TextEditAccessible)
			)
		);
		}

		///<summary>Gets the owning editor's type name.</summary>
		public override string EditorTypeName { get { return "CheckNumberEdit"; } }
		///<summary>Gets the owning CheckNumberEdit.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new CheckNumberEdit OwnerEdit { get { return (CheckNumberEdit)base.OwnerEdit; } }
		#endregion
	}
}
