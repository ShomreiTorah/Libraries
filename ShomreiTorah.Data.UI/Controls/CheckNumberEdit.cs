using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.Accessibility;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using ShomreiTorah.WinForms;

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
				if (IsDesignMode) return null;
				if (BindingManager == null) {
					var grid = Parent as GridControl;
					if (grid != null) {
						var view = (ColumnView)grid.FocusedView;
						var row = view.GetFocusedRow() as Payment;
						return row;
					}
				}

				if (BindingManager.Count == 0) return null;
				return BindingManager.Current as Payment;
			}
		}
		Payment lastValidatedRow;
		string lastValidatedValue;
		///<summary>Raises the Validating event.</summary>
		protected override void OnValidating(CancelEventArgs e) {
			base.OnValidating(e);
			if (e.Cancel) return;
			var payment = BoundRow;
			if (payment == null) return;

			//Don't show the same warning dialog twice in a row.  (But do show the icon)
			bool showDialog = lastValidatedRow != payment || lastValidatedValue != Text;
			lastValidatedRow = payment; lastValidatedValue = Text;

			var duplicate = payment.FindDuplicate(Text);
			if (duplicate == null) {
				SetWarning(null);
				return;
			}
			if (showDialog) {
				var message = String.Format(CultureInfo.CurrentCulture, "{0} #{1} for {2} has already been entered ({3:d}, {4:c}).",
																		duplicate.Method, duplicate.CheckNumber, duplicate.Person.FullName, duplicate.Date, duplicate.Amount);
				Dialog.Show(message, MessageBoxIcon.Warning);
			}
			SetWarning(String.Format(CultureInfo.CurrentCulture, "Potential duplicate of {0} #{1} ({2:d}, {3:c}).",
																 duplicate.Method.ToLowerInvariant(), duplicate.CheckNumber, duplicate.Date, duplicate.Amount));
		}
		void SetWarning(string message) {
			ErrorText = message;
			ErrorIcon = String.IsNullOrEmpty(message) ? null : WarningIcon;
			var grid = Parent as GridControl;
			if (grid != null) {
				var view = (ColumnView)grid.FocusedView;
				view.SetColumnError(view.FocusedColumn, ErrorText, ErrorType.Warning);
			}
		}
		[ThreadStatic]
		static Image warningIcon;
		static Image WarningIcon {
			get {
				if (warningIcon == null)
					warningIcon = ResourceImageHelper.CreateImageFromResources("DevExpress.XtraEditors.Images.Warning.png", typeof(DXErrorProvider).Assembly);
				return warningIcon;
			}
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
