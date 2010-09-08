using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors;
using System.Collections;
using System.Drawing;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	class ResultsList : SimpleControl {
		readonly VScrollBar scrollBar;
		readonly ItemSelector owner;

		IList items;
		int itemHeight;
		int scrollOffset;
		public ResultsList(ItemSelector owner) {
			this.owner = owner;
			scrollBar = new VScrollBar();

		}

		void SetItems(IList items) {
			Refresh();
		}

		int ClientWidth {
			get {
				if (scrollBar.Visible)
					return Width - scrollBar.Width;
				return Width;
			}
		}

		void DrawItem(Graphics g, int rowIndex) {
			int x = Padding.Left;
			foreach (var column in owner.Columns.Where(c => c.Visible)) {
				DrawCell(g, x, rowIndex, column);

				x += column.Width + 4;
			}
		}

		readonly StringFormat cellFormat = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, LineAlignment = StringAlignment.Center };
		void DrawCell(Graphics g, int x, int rowIndex, ResultColumn column) {
			var location = new PointF(x, itemHeight * rowIndex - scrollOffset);
			var cellWidth = Math.Min(column.Width, ClientWidth - Padding.Right - x);
			g.DrawString(column.GetValue(items[rowIndex]), Font, SystemBrushes.ControlText, new RectangleF(location, new SizeF(itemHeight, cellWidth)), cellFormat);
		}

		///<summary>Releases the unmanaged resources used by the ResultsList and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				cellFormat.Dispose();
			}
			base.Dispose();
		}
	}
}
