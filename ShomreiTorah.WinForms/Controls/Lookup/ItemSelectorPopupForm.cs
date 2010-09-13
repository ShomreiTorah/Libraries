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
			ViewInfo.SelectedIndex = null;
		}
		public override void ProcessKeyDown(KeyEventArgs e) {
			base.ProcessKeyDown(e);
			//TODO: Keyboard Navigation
		}

		bool SelectByCoordinate(int y) {
			var index = ViewInfo.GetRowIndex(y);
			if (index.HasValue)
				ViewInfo.SelectedIndex = index.Value;
			return index.HasValue;
		}
		internal void OnEditorMouseWheel(DXMouseEventArgs e) {
			ScrollBy(-SystemInformation.MouseWheelScrollLines * Math.Sign(e.Delta));
			e.Handled = true;
			SelectByCoordinate(PointToClient(MousePosition).Y);		//e.Y is relative to the editor
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			SelectByCoordinate(e.Y);
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				if (SelectByCoordinate(e.Y))
					ViewInfo.SelectionState = ObjectState.Pressed;
			}
			//TODO: Drag-scroll timer
		}
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left && ViewInfo.SelectedIndex.HasValue)
				ViewInfo.SelectionState = ObjectState.Hot;
		}
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

		#region Layout
		public int ScrollTop {
			get { return Form.ScrollBar.Visible ? Form.ScrollBar.Value : 0; }
			set {
				if (ScrollTop == value) return;
				if (!Form.ScrollBar.Visible) throw new InvalidOperationException("Scrollbar is hidden");
				Form.ScrollBar.Value = value;
			}
		}

		public Rectangle RowsArea { get; private set; }
		public Rectangle HeaderArea { get; private set; }
		public int RowHeight { get; private set; }

		public int FirstVisibleRow { get { return ScrollTop / RowHeight; } }

		public IEnumerable<ResultColumn> VisibleColumns { get { return Form.Properties.Columns.Where(c => c.Visible); } }

		public int GetRowCoordinate(int rowIndex) {
			return RowsArea.Top + (rowIndex * RowHeight) - ScrollTop;
		}

		///<summary>Gets the index of the row at the specified Y coordinate in pixels.</summary>
		public int? GetRowIndex(int y) {
			if (y < RowsArea.Top || y > RowsArea.Bottom) return null;

			var index = (y + ScrollTop - RowsArea.Top) / RowHeight;
			if (index < 0 || index >= Form.Items.Count)
				return null;
			return index;
		}

		///<summary>Ensures that a row is completely visible in the current viewport.</summary>	
		public void EnsureVisible(int rowIndex) {
			var top = GetRowCoordinate(rowIndex);
			//ScrollTop does not include the height of the header row.
			if (top < RowsArea.Top)
				ScrollTop = rowIndex * RowHeight;
			else if (top + RowHeight > RowsArea.Bottom)
				ScrollTop = (rowIndex + 1) * RowHeight - RowsArea.Height - 1;	//Ensure that the bottom of the row is visible
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

				Form.ScrollBar.Left = ContentRect.Right - Form.ScrollBar.Width;
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
		#endregion

		#region Column Headers
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
			last.Bounds = new Rectangle(last.Bounds.X, last.Bounds.Y, HeaderArea.Right - last.Bounds.X, HeaderArea.Height);
			last.HeaderPosition = HeaderPositionKind.Right;

			foreach (var header in ColumnHeaderArgs)
				HeaderPainter.CalcObjectBounds(header);
		}
		#endregion

		#region Selection
		public SkinElement SelectionElement { get; private set; }

		protected override void UpdatePainters() {
			base.UpdatePainters();
			//Alternatives:
			//Ribbon/Button
			//Ribbon/ButtonGroupButton
			//Editors/NavigatorButton
			SelectionElement = NavPaneSkins.GetSkin(Form.Properties.LookAndFeel)[NavPaneSkins.SkinOverflowPanelItem];
		}

		int? selectedIndex;
		ObjectState selectionState;

		public int? SelectedIndex {
			get { return selectedIndex; }
			set {
				if (selectedIndex == value) return;
				if (value < 0 || value >= Form.Items.Count) throw new ArgumentOutOfRangeException("value");

				selectedIndex = value;

				if (SelectedIndex.HasValue)
					EnsureVisible(SelectedIndex.Value);

				Form.Invalidate();
			}
		}
		public ObjectState SelectionState {
			get { return selectionState; }
			set {
				if (SelectionState == value) return;
				selectionState = value;
				Form.Invalidate();
			}
		}
		#endregion
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

		static void DrawColumnHeaders(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			foreach (var header in info.ColumnHeaderArgs) {
				header.Cache = args.Cache;
				info.HeaderPainter.DrawObject(header);
				header.Cache = null;
			}
		}

		static void DrawVertLines(PopupFormGraphicsInfoArgs args) {
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

		static void DrawRows(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			using (args.Cache.ClipInfo.SaveAndSetClip(info.RowsArea)) {
				DrawSelectionHighlight(args);	//The selection highlight should be clipped

				for (int rowIndex = info.FirstVisibleRow; rowIndex < info.Form.Items.Count; rowIndex++) {
					int y = info.GetRowCoordinate(rowIndex);		//TODO: Selection
					if (y > info.RowsArea.Bottom) break;

					DrawRow(args, rowIndex);
				}
			}
		}
		static void DrawRow(PopupFormGraphicsInfoArgs args, int rowIndex) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			int x = info.RowsArea.X + info.SelectionElement.ContentMargins.Left + 1;
			foreach (var column in info.VisibleColumns) {
				DrawCell(args, rowIndex, column, x);

				x += column.Width + 4;
				if (x > info.RowsArea.Right) break;
			}
		}

		static void DrawCell(PopupFormGraphicsInfoArgs args, int rowIndex, ResultColumn column, int x) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			var location = new Point(x, info.GetRowCoordinate(rowIndex));
			var cellWidth = Math.Min(column.Width, info.RowsArea.Right - info.SelectionElement.ContentMargins.Right - x);
			var cellBounds = new Rectangle(location, new Size(cellWidth, info.RowHeight));

			var text = column.GetValue(info.Form.Items[rowIndex]);

			info.AppearanceResults.DrawString(args.Cache, text, cellBounds);
		}

		static void DrawSelectionHighlight(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			if (info.SelectedIndex == null) return;

			SkinElementInfo elemInfo = new SkinElementInfo(info.SelectionElement,
				new Rectangle(info.RowsArea.X, info.GetRowCoordinate(info.SelectedIndex.Value), info.RowsArea.Width, info.RowHeight)
			);

			elemInfo.Cache = args.Cache;
			elemInfo.State = info.SelectionState;
			elemInfo.ImageIndex = info.SelectionState == ObjectState.Pressed ? 2 : 1;

			SkinElementPainter.Default.DrawObject(elemInfo);
		}
	}
}
