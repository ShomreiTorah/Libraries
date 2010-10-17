using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.Utils.Serializing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Container;
using DevExpress.XtraEditors.Repository;

namespace ShomreiTorah.Data.UI.Grid {
	partial class SmartGridView {
		bool allowEditorWheel;
		///<summary>Gets or sets whether the view mouse wheel events will be processed by the active editor.</summary>
		[Description("Gets or sets whether the view mouse wheel events will be processed by the active editor.")]
		[Category("Behavior")]
		[DefaultValue(false)]
		[XtraSerializableProperty]
		public bool AllowEditorMouseWheel {
			get { return allowEditorWheel; }
			set { allowEditorWheel = value; }
		}

		protected override void UpdateEditor(RepositoryItem ritem, UpdateEditorInfoArgs args) {
			base.UpdateEditor(ritem, args);
			if (ActiveEditor != null) {
				ActiveEditor.MouseWheel += ActiveEditor_MouseWheel;
				var te = ActiveEditor as TextEdit;
				if (te != null)
					te.MaskBox.MouseWheel += ActiveEditor_MouseWheel;
			}
		}
		///<summary>Hides the currently active editor, discarding any changes.</summary>
		public override void HideEditor() {
			if (ActiveEditor != null) {
				ActiveEditor.MouseWheel -= ActiveEditor_MouseWheel;
				var te = ActiveEditor as TextEdit;
				if (te != null)
					te.MaskBox.MouseWheel -= ActiveEditor_MouseWheel;
			}
			base.HideEditor();
		}

		void ActiveEditor_MouseWheel(object sender, MouseEventArgs e) {
			if (!AllowEditorMouseWheel) {
				DXMouseEventArgs ee = DXMouseEventArgs.GetMouseArgs(e);
				this.Handler.ProcessMouseWheel(ee);
				ee.Handled = true;
			}
		}

		partial class MyHandler {
			internal void ProcessMouseWheel(DXMouseEventArgs ee) { OnMouseWheel(ee); }
			protected override bool OnMouseWheel(MouseEventArgs e) {
				DXMouseEventArgs ee = DXMouseEventArgs.GetMouseArgs(e);
				//Copied from GridHandler source.
				//Originally called base.OnMouseWheel, which called View.RaiseMouseWheel.
				View.RaiseMouseWheel(ee);

				if (ee.Handled) return true;
				try {
					if (View.AllowEditorMouseWheel && View.IsEditing) return false;
					if (View.FilterPopup != null) return true;
					View.TopRowIndex += (e.Delta > 0 ? -GetScrollLinesCount() : GetScrollLinesCount());
					return true;
				} finally {
					if (View != null && View.ScrollInfo != null && View.ScrollInfo.VScroll != null && View.ScrollInfo.VScroll.Visible) ee.Handled = true;
				}
			}
			int GetScrollLinesCount() {
				return SystemInformation.MouseWheelScrollLines == -1 ? View.ScrollPageSize : SystemInformation.MouseWheelScrollLines;
			}
		}
	}
}
