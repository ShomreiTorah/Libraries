using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Rows;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.Forms {
	class ColumnValueDisplay : VGridControl {
		public ColumnValueDisplay() {
			this.OptionsBehavior.Editable = true;
		}

		public void SetDisplay(Row row) {
			SetDisplay(
				row.Table,
				row.Schema.Columns.ToDictionary(c => c, c => row[c])
			);
		}
		public void SetDisplay(Table owner, Dictionary<Column, object> values) {
			Rows.Clear();

			foreach (var column in owner.Schema.Columns) {
				var row = new EditorRow { Name = column.Name };
				row.Properties.ReadOnly = true;

				object value;
				values.TryGetValue(column, out value);
				row.Properties.Value = value;

				Rows.Add(row);
			}
		}
	}
}
