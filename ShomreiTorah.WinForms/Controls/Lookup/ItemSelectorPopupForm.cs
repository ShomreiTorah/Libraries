using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Popup;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	class ItemSelectorPopupForm : CustomBlobPopupForm {
		public DevExpress.XtraEditors.VScrollBar ScrollBar { get; private set; }
		public IList Items { get { return OwnerEdit.AllItems; } }

		public ItemSelectorPopupForm(ItemSelector owner)
			: base(owner) {

			ScrollBar = new DevExpress.XtraEditors.VScrollBar();
			ScrollBar.ScrollBarAutoSize = true;
			ScrollBar.Scroll += ScrollBar_Scroll;
			ScrollBar.LookAndFeel.Assign(Properties.LookAndFeel);
			Controls.Add(ScrollBar);
		}

		protected override PopupBaseFormPainter CreatePainter() { return new ItemSelectorPopupFormPainter(); }
		protected override PopupBaseFormViewInfo CreateViewInfo() { return new ItemSelectorPopupFormViewInfo(this); }
		public new RepositoryItemItemSelector Properties { get { return (RepositoryItemItemSelector)base.Properties; } }
		public new ItemSelector OwnerEdit { get { return (ItemSelector)base.OwnerEdit; } }
		public new ItemSelectorPopupFormViewInfo ViewInfo { get { return (ItemSelectorPopupFormViewInfo)base.ViewInfo; } }

		protected virtual void ScrollBar_Scroll(object sender, ScrollEventArgs e) {
			Invalidate();
		}

		public void ScrollBy(int rows) {
			SetScrollPos(ScrollBar.Value + rows * ViewInfo.RowHeight);
		}
		public void SetScrollPos(int pos) {
			if (pos < 0) pos = 0;
			ScrollBar.Value = Math.Min(pos, ScrollBar.Maximum - ViewInfo.RowsArea.Height + 1);	//Subtract the height of the thumb so that the last row is on the bottom
			Invalidate();
		}

		public override void ShowPopupForm() {
			base.ShowPopupForm();
		}
		public override void ProcessKeyDown(KeyEventArgs e) {
			base.ProcessKeyDown(e);
			//TODO: Keyboard Navigation
		}
		//TODO: Mouse events (selection)
	}
	class ItemSelectorPopupFormViewInfo : CustomBlobPopupFormViewInfo {
		public ItemSelectorPopupFormViewInfo(ItemSelectorPopupForm form)
			: base(form) {
			AppearanceColumnHeader = new AppearanceObject();
			AppearanceResults = new AppearanceObject();

			HeaderPainter = Form.Properties.LookAndFeel.Painter.Header;
			ColumnHeaderArgs = new List<HeaderObjectInfoArgs>();
		}

		public new ItemSelectorPopupForm Form { get { return (ItemSelectorPopupForm)base.Form; } }

		#region Appearances
		bool IsSkinned { get { return Form.Properties.LookAndFeel.ActiveStyle == ActiveLookAndFeelStyle.Skin; } }

		public AppearanceObject AppearanceColumnHeader { get; private set; }
		AppearanceDefault ColumnHeadersDefault {
			get {
				if (IsSkinned)
					return GridSkins.GetSkin(Form.LookAndFeel)[GridSkins.SkinHeader].GetAppearanceDefault();

				return new AppearanceDefault(GetSystemColor(SystemColors.ControlText), GetSystemColor(SystemColors.Control), HorzAlignment.Center, VertAlignment.Center);
			}
		}
		public AppearanceObject AppearanceResults { get; private set; }
		AppearanceObject ResultsDefault {
			get {
				var retVal = new AppearanceObject {
					ForeColor = GetSystemColor(SystemColors.ControlText),
					BackColor = GetSystemColor(SystemColors.Control)
				};
				retVal.TextOptions.Trimming = Trimming.EllipsisCharacter;
				return retVal;
			}
		}
		public override void UpdatePaintAppearance() {
			base.UpdatePaintAppearance();
			AppearanceHelper.Combine(AppearanceColumnHeader, new[] { Form.Properties.AppearanceColumnHeader, StyleController == null ? null :
																						  StyleController.AppearanceDropDownHeader }, ColumnHeadersDefault);
			AppearanceHelper.Combine(AppearanceResults, Form.Properties.AppearanceResults, ResultsDefault);
		}
		#endregion

		public int ScrollTop { get { return Form.ScrollBar.Visible ? Form.ScrollBar.Value : 0; } }

		public Rectangle RowsArea { get; private set; }
		public Rectangle HeaderArea { get; private set; }
		public int RowHeight { get; private set; }

		public int FirstVisibleRow { get { return ScrollTop / RowHeight; } }
		//public int VisibleRows { get { return Math.Min(Form.Items.Count,  RowsArea.Height / RowHeight); } }

		public IEnumerable<ResultColumn> VisibleColumns { get { return Form.Properties.Columns.Where(c => c.Visible); } }

		public int GetRowCoordinate(int rowIndex) {
			return RowsArea.Top + (rowIndex * RowHeight) - ScrollTop;
		}

		protected override void CalcContentRect(Rectangle bounds) {
			base.CalcContentRect(bounds);

			var headerHeight = Form.Properties.ShowColumnHeaders ? 20 : 0;
			RowHeight = 20;	//TODO: Calculate
			var rowAreaHeight = ContentRect.Height - headerHeight;

			Form.ScrollBar.Maximum = RowHeight * Form.Items.Count;
			Form.ScrollBar.LargeChange = rowAreaHeight;
			Form.ScrollBar.Visible = Form.ScrollBar.Maximum > rowAreaHeight;

			int availableWidth = ContentRect.Width;
			if (Form.ScrollBar.Visible) {
				availableWidth -= Form.ScrollBar.Width;

				Form.ScrollBar.Left = ContentRect.Width - Form.ScrollBar.Width;
				Form.ScrollBar.Top = ContentRect.Top + headerHeight;
				Form.ScrollBar.Height = rowAreaHeight;
			}

			HeaderArea = new Rectangle(ContentRect.Location, new Size(ContentRect.Width, headerHeight));	//The header should go over the scrollbar
			RowsArea = new Rectangle(HeaderArea.Left, HeaderArea.Bottom, availableWidth, rowAreaHeight);

			CreateColumnHeaderArgs();

			//If the scroll range shrunk enough that the 
			//current position is past the end, update it
			//(the scrollbar will only constrain Value to
			//Maximum, so that there might be some blank 
			//space)
			Form.SetScrollPos(Form.ScrollBar.Value);
		}

		public HeaderObjectPainter HeaderPainter { get; private set; }
		public List<HeaderObjectInfoArgs> ColumnHeaderArgs { get; private set; }
		void CreateColumnHeaderArgs() {
			ColumnHeaderArgs.Clear();
			int x = HeaderArea.X;

			foreach (var column in VisibleColumns) {
				var header = new HeaderObjectInfoArgs {
					Bounds = new Rectangle(x, HeaderArea.Y, Math.Min(column.Width, HeaderArea.Width - x), HeaderArea.Height),
					Caption = column.Caption,
				};
				if (ColumnHeaderArgs.Count == 0) header.HeaderPosition = HeaderPositionKind.Left;
				header.SetAppearance(AppearanceColumnHeader);
				ColumnHeaderArgs.Add(header);
				x += column.Width;
				if (x > HeaderArea.Right) break;
			}

			//Stretch the last header to fill the entire width
			var last = ColumnHeaderArgs.Last();
			last.Bounds = new Rectangle(last.Bounds.X, last.Bounds.Y, HeaderArea.Width - last.Bounds.X, HeaderArea.Height);
			last.HeaderPosition = HeaderPositionKind.Right;

			foreach (var header in ColumnHeaderArgs)
				HeaderPainter.CalcObjectBounds(header);
		}
	}
	class ItemSelectorPopupFormPainter : PopupBaseSizeableFormPainter {
		protected override void DrawContent(PopupFormGraphicsInfoArgs info) {
			base.DrawContent(info);

			var vi = (ItemSelectorPopupFormViewInfo)info.ViewInfo;
			if (vi.Form.Properties.ShowColumnHeaders)
				DrawColumnHeaders(info);
			DrawRows(info);
			if (vi.Form.Properties.ShowVerticalLines)
				DrawVertLines(info);
		}

		void DrawColumnHeaders(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			foreach (var header in info.ColumnHeaderArgs) {
				header.Cache = args.Cache;
				info.HeaderPainter.DrawObject(header);
				header.Cache = null;
			}
		}

		void DrawVertLines(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			var cols = info.VisibleColumns.ToArray();
			var x = info.RowsArea.X + cols.First().Width - 1;

			for (int i = 1; i < cols.Length; i++) {
				args.Graphics.DrawLine(Pens.DarkGray,
					x, info.RowsArea.Top,
					x, info.RowsArea.Top + Math.Min(info.RowsArea.Height, info.Form.Items.Count * info.RowHeight)
				);
				x += cols[i].Width;
			}
		}

		void DrawRows(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			using (args.Cache.ClipInfo.SaveAndSetClip(info.RowsArea)) {

				for (int rowIndex = info.FirstVisibleRow; rowIndex < info.Form.Items.Count; rowIndex++) {
					int y = info.GetRowCoordinate(rowIndex);		//TODO: Selection
					if (y > info.RowsArea.Bottom) break;

					DrawRow(args, rowIndex);
				}
			}
		}
		void DrawRow(PopupFormGraphicsInfoArgs args, int rowIndex) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			int y = info.GetRowCoordinate(rowIndex);		//TODO: Selection

			int x = info.RowsArea.X;
			foreach (var column in info.VisibleColumns) {
				DrawCell(args, rowIndex, column, x);

				x += column.Width + 4;
			}
		}

		void DrawCell(PopupFormGraphicsInfoArgs args, int rowIndex, ResultColumn column, int x) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			var location = new Point(x, info.GetRowCoordinate(rowIndex));
			var cellWidth = Math.Min(column.Width, info.RowsArea.Right - x);
			var cellBounds = new Rectangle(location, new Size(cellWidth, info.RowHeight));

			var text = column.GetValue(info.Form.Items[rowIndex]);

			info.AppearanceResults.DrawString(args.Cache, text, cellBounds);
		}
	}
}
