using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.Utils.Serializing;
using System.Windows.Forms;

namespace ShomreiTorah.Data.UI.Grid {
	partial class SmartGridColumn {
		bool allowKeyboardActivation = true;

		///<summary>Gets or sets whether the column's editor can be activated using the keyboard when the cell is focused.</summary>
		[Description("Gets or sets whether the column's editor can be activated using the keyboard when the cell is focused.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		[XtraSerializableProperty]
		public bool AllowKeyboardActivation {
			get { return allowKeyboardActivation; }
			set { allowKeyboardActivation = value; }
		}
	}
	partial class SmartGridView {
		///<summary>Activates an editor for the focused row cell and passes a specific key to it.</summary>
		public override void ShowEditorByKey(KeyEventArgs e) {
			var column = FocusedColumn as SmartGridColumn;
			if (column == null || column.AllowKeyboardActivation)
				base.ShowEditorByKey(e);
		}
		///<summary>Activates an editor for the focused row cell and passes a specific key to it.</summary>
		public override void ShowEditorByKeyPress(KeyPressEventArgs e) {
			var column = FocusedColumn as SmartGridColumn;
			if (column == null || column.AllowKeyboardActivation)
				base.ShowEditorByKeyPress(e);
		}
	}
}
