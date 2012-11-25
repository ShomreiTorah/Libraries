using System;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Data.UI.Forms {
	partial class RowDeletedPrompt : DevExpress.XtraEditors.XtraForm {
		private readonly RowDeletedException exception;

		public RowDeletedPrompt(RowDeletedException ex) {
			InitializeComponent();

			this.exception = ex;

			rowDisplay.SetDisplay(ex.Row);
		}

		private void delete_Click(object sender, EventArgs e) {
			exception.Row.RemoveRow();
			Close();
		}
	}
}