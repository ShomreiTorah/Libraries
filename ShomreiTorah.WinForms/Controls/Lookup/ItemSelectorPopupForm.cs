using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors;
using System.Collections;
using System.Drawing;
using DevExpress.XtraEditors.Popup;
using System.Windows.Forms;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	class ItemSelectorPopupForm : CustomBlobPopupForm {
		readonly DevExpress.XtraEditors.VScrollBar scrollBar;
		readonly ItemSelector owner;

		IList items;
		int itemHeight;
		int scrollOffset;
		public ItemSelectorPopupForm(ItemSelector owner)
			: base(owner) {
			this.owner = owner;
			scrollBar = new DevExpress.XtraEditors.VScrollBar();
			scrollBar.Scroll += ScrollBar_Scroll;
			scrollBar.ScrollBarAutoSize = true;
			scrollBar.LookAndFeel.Assign(Properties.LookAndFeel);
			Controls.Add(scrollBar);

		}

		public new RepositoryItemItemSelector Properties { get { return (RepositoryItemItemSelector)base.Properties; } }

		protected virtual void ScrollBar_Scroll(object sender, ScrollEventArgs e) {
		}

		void SetItems(IList items) {
			Refresh();
		}
		public override void ShowPopupForm() {
			base.ShowPopupForm();
		}
		public override void ProcessKeyDown(KeyEventArgs e) {
			base.ProcessKeyDown(e);
			//TODO: Keyboard Navigation
		}
		//TODO: Mouse events (selection)

		int ClientWidth {
			get {
				if (scrollBar.Visible)
					return Width - scrollBar.Width;
				return Width;
			}
		}
		protected override void UpdateControlPositionsCore() {
			base.UpdateControlPositionsCore();
			//TODO: Handle scrollbar?
		}

		void DrawItem(Graphics g, int rowIndex) {
			int x = Padding.Left;
			foreach (var column in Properties.Columns.Where(c => c.Visible)) {
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
