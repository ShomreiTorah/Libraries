using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.Utils.Serializing;
using System.Windows.Forms;
using DevExpress.Utils;

namespace ShomreiTorah.Data.UI.Grid {
	partial class SmartGridColumn {
		bool showEditorOnMouseDown;

		///<summary>Gets or sets whether the column's editor will always be shown on mousedown, regardless of the view's EditorShowMode.</summary>
		[Description("Gets or sets whether the column's editor will always be shown on mousedown, regardless of the view's EditorShowMode.")]
		[Category("Behavior")]
		[DefaultValue(false)]
		[XtraSerializableProperty]
		public bool ShowEditorOnMouseDown {
			get { return showEditorOnMouseDown; }
			set { showEditorOnMouseDown = value; }
		}
	}

	partial class SmartGridView {
		partial class MyHandler {
			protected override bool OnMouseDown(MouseEventArgs ev) {
				DXMouseEventArgs e = DXMouseEventArgs.GetMouseArgs(ev);
				base.OnMouseDown(e);
				if (e.Handled) return true;

				//If the user mouse-downed in a cell and the column
				//overrode EditorShowMode, show the editor.    This
				//code was copied from the GridHandler source.
				if (e.Button == MouseButtons.Left && View.IsDefaultState) {
					var col = DownPointHitInfo.Column as SmartGridColumn;
					if (col != null && DownPointHitInfo.InRowCell) {
						//If the column should be mouse activated,
						//but the rest of the grid should not, and
						//the user isn't holding down shift, focus
						//the editor.
						if (col.ShowEditorOnMouseDown
						 && View.GetShowEditorMode() != EditorShowMode.MouseDown
						 && Control.ModifierKeys == 0) {		//Don't interfere with multi-selection
							View.ShowEditorByMouse();

							if (SmartOwner != null) {	//The designer's feature browser uses a different control.
								if (View.IsEditing)
									SmartOwner.MouseCaptureOwner = null;
							}
						}
					}
				}

				return false;
			}
		}
	}
}