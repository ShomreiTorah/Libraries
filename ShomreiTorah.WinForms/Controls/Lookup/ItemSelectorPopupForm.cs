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
	///<summary>The popup form that displays the results of an ItemSelector control.</summary>
	///<remarks>
	/// This class handles interaction; the ViewInfo tracks layout, and the Painter draws the form.
	/// Since I don't have InfoArgs objects for each row and cell, the Painter also needs to do some layout calculation.
	///</remarks>
	sealed class ItemSelectorPopupForm : CustomBlobPopupForm {
		public DevExpress.XtraEditors.VScrollBar ScrollBar { get; private set; }
		///<summary>Gets the items currently displayed in the results grid.</summary>
		///<remarks>This is a filtered list maintained by ItemSelector.</remarks>
		public IList Items { get { return OwnerEdit.CurrentItems; } }

		readonly Timer dragScrollTimer;

		public ItemSelectorPopupForm(ItemSelector owner)
			: base(owner) {
			base.fCloseButtonStyle = BlobCloseButtonStyle.None;

			ScrollBar = new DevExpress.XtraEditors.VScrollBar();
			ScrollBar.ScrollBarAutoSize = true;
			ScrollBar.Scroll += ScrollBar_Scroll;
			ScrollBar.LookAndFeel.Assign(Properties.LookAndFeel);
			Controls.Add(ScrollBar);

			dragScrollTimer = new Timer();
			dragScrollTimer.Tick += DragScrollTimer_Tick;
		}

		protected override PopupBaseFormPainter CreatePainter() { return new ItemSelectorPopupFormPainter(); }
		protected override PopupBaseFormViewInfo CreateViewInfo() { return new ItemSelectorPopupFormViewInfo(this); }

		public new ItemSelector OwnerEdit { get { return (ItemSelector)base.OwnerEdit; } }
		public new RepositoryItemItemSelector Properties { get { return (RepositoryItemItemSelector)base.Properties; } }
		public new ItemSelectorPopupFormViewInfo ViewInfo { get { return (ItemSelectorPopupFormViewInfo)base.ViewInfo; } }

		void ScrollBar_Scroll(object sender, ScrollEventArgs e) { Invalidate(); }


		///<summary>Recalculates and redraws the grid in response to a change in the displayed items.</summary>
		public void RefreshItems() {
			ViewInfo.HoveredIndex = null;
			ViewInfo.ScrollTop = 0;

			Bounds = ConstrainHeight(Bounds, Properties.UserPopupHeight);

			LayoutChanged();		//This call recalculates the ViewInfo.
		}

		#region Sizing control
		///<summary>Gets the natural height of the popup that would not require a scrollbar.</summary>
		int NaturalHeight { get { return Items.Count * ViewInfo.RowHeight + ViewInfo.VerticalFrameHeight; } }

		///<summary>Gets the bounds of the form as the user resizes it.</summary>
		///<remarks>The base class sets this property as the user drags</remarks>
		protected override Rectangle SizingBounds {
			get { return base.SizingBounds; }
			set {
				//If there are enough items that the user's preferred
				//height isn't being overridden, update it.  If there
				//are too few items, resizes are not persisted, since
				//they are subject to the limiy.
				if (value.Height < NaturalHeight && NaturalHeight > Properties.UserPopupHeight)
					Properties.UserPopupHeight = value.Height;

				base.SizingBounds = ConstrainHeight(value);
			}
		}
		protected override Size MinFormSize { get { return new Size(OwnerEdit.Width, ViewInfo.VerticalFrameHeight + ViewInfo.RowHeight); } }

		///<summary>Gets the popup's initial size.</summary>
		protected override Size CalcFormSizeCore() {
			var baseSize = base.CalcFormSizeCore();	//Read user-set width; the minimum will be applied by MinFormSize
			return new Size(baseSize.Width, Math.Min(NaturalHeight, Properties.UserPopupHeight));
		}

		///<summary>Constrains the height of a rectangle to ensure that it is not taller than necessary to display all of the results.</summary>
		Rectangle ConstrainHeight(Rectangle rect, int? heightOverride = null) {
			var naturalHeight = NaturalHeight;
			var desiredHeight = heightOverride ?? rect.Height;

			if (desiredHeight > naturalHeight) {
				if (ViewInfo.IsTopSizeBar)	//If the popup is above the editor, adjust the Y coordinate to preserve the bottom
					rect.Y += desiredHeight - naturalHeight;
				rect.Height = naturalHeight;
			} else
				rect.Height = desiredHeight;

			return rect;
		}
		#endregion

		public override void ShowPopupForm() {
			base.ShowPopupForm();
			//Reset the popup's state
			ViewInfo.HoveredIndex = null;
			dragScrollTimer.Stop();
			isTrackingMouseDown = false;
			isKeyDown = false;
		}

		///<summary>Sets the result of the popup to the currently hovered item.</summary>
		void SetResult() {
			if (!ViewInfo.HoveredIndex.HasValue)
				throw new InvalidOperationException("No item is hovered");

			var item = Items[ViewInfo.HoveredIndex.Value];
			if (!Properties.RaiseItemSelecting(ResultValue))
				return;
			popupResultValue = item;
			ClosePopup(PopupCloseMode.Normal);
		}
		object popupResultValue;
		///<summary>Gets the final result of the popup.</summary>
		///<remarks>This is only set when the user clicks an item.
		///Otherwise, clicking outside the popup would select this item, which would be undesirable.</remarks>
		public override object ResultValue { get { return popupResultValue; } }

		public void ScrollBy(int rows) {
			ScrollTo(ScrollBar.Value + rows * ViewInfo.RowHeight);
		}
		public void ScrollTo(int pos) {
			if (pos < 0) pos = 0;
			ScrollBar.Value = Math.Min(pos, ScrollBar.Maximum - ViewInfo.RowsArea.Height + 1);	//Subtract the height of the thumb so that the last row is on the bottom
			Invalidate();
		}
		///<summary>Sets the specified row to be drawn with a hover effect.</summary>
		void SetHoveredRow(int rowIndex) {
			if (rowIndex < 0)
				ViewInfo.HoveredIndex = 0;
			else if (rowIndex >= Items.Count)
				ViewInfo.HoveredIndex = Items.Count - 1;
			else
				ViewInfo.HoveredIndex = rowIndex;
		}

		bool isKeyDown;
		public override void ProcessKeyDown(KeyEventArgs e) {
			base.ProcessKeyDown(e);
			isKeyDown = true;

			int currentIndex = ViewInfo.HoveredIndex ?? -1;
			int? newIndex = null;
			switch (e.KeyCode) {
				case Keys.Enter:
					if (ViewInfo.HoveredIndex.HasValue) {
						SetResult();
						e.Handled = true;
						return;
					}
					break;

				#region Navigation
				case Keys.Up: newIndex = currentIndex - 1; break;
				case Keys.Down: newIndex = currentIndex + 1; break;

				case Keys.PageUp:
					newIndex = currentIndex - ViewInfo.MaxVisibleRows;
					break;
				case Keys.PageDown:
					newIndex = currentIndex + ViewInfo.MaxVisibleRows;
					break;

				case Keys.Home:
					if (e.Control) newIndex = 0;
					break;
				case Keys.End:
					if (e.Control) newIndex = Items.Count - 1;
					break;
				#endregion
			}
			if (newIndex.HasValue) {
				SetHoveredRow(newIndex.Value);
				e.Handled = true;
			}
		}
		public override void ProcessKeyUp(KeyEventArgs e) {
			base.ProcessKeyUp(e);
			isKeyDown = false;
		}
		#region Mouse Handling
		///<summary>Applies the hover effect to the row at the given Y coordinate.</summary>
		///<remarks>This method ignores the X coordinate.</remarks>
		bool HoverByCoordinate(int y) {
			//When selecting a row by point, we should ignore the X coordinate.
			//This allows the user to drag next to the popup.
			return HoverByPoint(new Point(ViewInfo.RowsArea.X + 1, y));
		}
		///<summary>Applies the hover effect to the row at the given point.</summary>
		///<remarks>If the point is next to (but not inside) the grid, nothing will happen.</remarks>
		bool HoverByPoint(Point pt) {
			var index = ViewInfo.GetRowIndex(pt);
			if (index.HasValue)
				ViewInfo.HoveredIndex = index.Value;
			return index.HasValue;
		}
		internal void OnEditorMouseWheel(DXMouseEventArgs e) {
			ScrollBy(-SystemInformation.MouseWheelScrollLines * Math.Sign(e.Delta));
			e.Handled = true;

			//If the user turns the scroll wheel over a row, the row should be hovered.
			if (!isKeyDown)
				HoverByPoint(PointToClient(MousePosition));		//e.Location is relative to the editor
		}

		bool isTrackingMouseDown;
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				if (HoverByPoint(e.Location)) {
					isTrackingMouseDown = true;
					ViewInfo.HoveredItemState = ObjectState.Pressed;
				}
			}
		}
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			//If the user started dragging on anything other than a row (eg, resize), ignore it.
			//If the user is dragging a row, select the row level with the mouse, even if the mouse is to the side of the grid.
			if (isTrackingMouseDown)
				HoverByCoordinate(e.Y);
			else if (!isKeyDown && e.Button == MouseButtons.None)	//If the mouse is directly over a row, hover the row.
				HoverByPoint(e.Location);

			if (isTrackingMouseDown && (e.Y < ViewInfo.RowsArea.Top || e.Y > ViewInfo.RowsArea.Bottom)) {
				dragScrollTimer.Enabled = true;

				//Depending whether the mouse is below or above the grid, select the correct distance
				int distance = Math.Max(ViewInfo.RowsArea.Top - e.Y, e.Y - ViewInfo.RowsArea.Bottom);
				dragScrollTimer.Interval = 100 / Math.Max(1, (distance / ViewInfo.RowHeight));
			} else
				dragScrollTimer.Enabled = false;
		}

		void DragScrollTimer_Tick(object sender, EventArgs e) {
			var y = PointToClient(MousePosition).Y;

			//If y is lower than the grid bounds, scroll down.
			//The HoveredRow setter will automatically scroll.
			SetHoveredRow((ViewInfo.HoveredIndex ?? 0) + Math.Sign(y - ViewInfo.RowsArea.Bottom));
		}
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			dragScrollTimer.Stop();
			if (e.Button == MouseButtons.Left && isTrackingMouseDown) {
				ViewInfo.HoveredItemState = ObjectState.Hot;

				//If the mouseup happened exactly on the row, select the row.
				//(But not if the mouse is to the side of the grid)
				if (ViewInfo.HoveredIndex == ViewInfo.GetRowIndex(e.Location))
					SetResult();
			}
			isTrackingMouseDown = false;
		}
		#endregion

		///<summary>Releases the unmanaged resources used by the ItemSelectorPopupForm and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (ViewInfo != null) ViewInfo.Dispose();
				dragScrollTimer.Dispose();
			}
			base.Dispose(disposing);
		}
	}
	///<summary>Stores and manages the layout of the results popup.</summary>
	sealed class ItemSelectorPopupFormViewInfo : CustomBlobPopupFormViewInfo, IDisposable {
		public ItemSelectorPopupFormViewInfo(ItemSelectorPopupForm form)
			: base(form) {
			ShowSizeBar = Form.Properties.AllowResize;

			AppearanceColumnHeader = new AppearanceObject();
			AppearanceResults = new AppearanceObject();
			AppearanceMatch = new AppearanceObject();

			ColumnHeaderArgs = new List<HeaderObjectInfoArgs>();
			HeaderPainter = Form.Properties.LookAndFeel.Painter.Header;

			//This looks nicer in many skins, but has bad padding.
			//HoverElement = NavPaneSkins.GetSkin(Form.Properties.LookAndFeel)[NavPaneSkins.SkinOverflowPanelItem];
			switch (Form.Properties.LookAndFeel.ActiveSkinName) {
				case "Darkroom":	//Workaround for unsolveable issue - their ribbon button is transparent
					HoverElement = CommonSkins.GetSkin(Form.Properties.LookAndFeel)[CommonSkins.SkinButton];
					break;
				default:
					HoverElement = RibbonSkins.GetSkin(Form.Properties.LookAndFeel)[RibbonSkins.SkinButton];
					break;
			}

			if (IsSkinned) {
				var image = (Bitmap)((SkinHeaderObjectPainter)HeaderPainter).Element.Image.GetImages().Images[0];
				LinePen = new Pen(image.GetPixel(image.Width - 1, image.Height - 1));
			} else
				LinePen = new Pen(Color.DarkGray);	//Don't use a shared pen because I need to dispose it later
		}

		///<summary>Releases all resources used by the ItemSelectorPopupFormViewInfo.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the ItemSelectorPopupFormViewInfo and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		void Dispose(bool disposing) {
			if (disposing) {
				if (LinePen != null) LinePen.Dispose();
			}
		}
		public new ItemSelectorPopupForm Form { get { return (ItemSelectorPopupForm)base.Form; } }

		public IEnumerable<string> FilterWords { get { return Form.OwnerEdit.FilterWords; } }

		#region Appearances
		bool IsSkinned { get { return Form.Properties.LookAndFeel.ActiveStyle == ActiveLookAndFeelStyle.Skin; } }

		///<summary>Gets the appearance used to paint column headers.</summary>
		public AppearanceObject AppearanceColumnHeader { get; private set; }
		AppearanceDefault ColumnHeaderDefault {
			get {
				if (IsSkinned)
					return GridSkins.GetSkin(Form.LookAndFeel)[GridSkins.SkinHeader].GetAppearanceDefault();

				return new AppearanceDefault(GetSystemColor(SystemColors.ControlText), GetSystemColor(SystemColors.Control), HorzAlignment.Center, VertAlignment.Center);
			}
		}
		///<summary>Gets the appearance used to paint column values.</summary>
		public AppearanceObject AppearanceResults { get; private set; }
		AppearanceObject ResultsDefault {
			get {
				var retVal = new AppearanceObject {
					BackColor = Form.BackColor,
					ForeColor = Form.ForeColor
				};
				retVal.TextOptions.Trimming = Trimming.EllipsisCharacter;
				return retVal;
			}
		}
		///<summary>Gets the appearance used to paint the matched portion of a column value.</summary>
		public AppearanceObject AppearanceMatch { get; private set; }
		AppearanceObject MatchDefault {
			get {
				//I try to color the matches blue, without breaking dark skins.
				var defaultColor = AppearanceResults.GetForeColor();
				var newColor = new HSL(2 / 3.0, 1, defaultColor.GetBrightness());
				return new AppearanceObject { ForeColor = newColor.GetColor(), Font = new Font(AppearanceResults.GetFont(), FontStyle.Bold) };
			}
		}
		public override void UpdatePaintAppearance() {
			base.UpdatePaintAppearance();
			AppearanceHelper.Combine(AppearanceColumnHeader,
				new[] { Form.Properties.AppearanceColumnHeader, StyleController == null ? null : StyleController.AppearanceDropDownHeader }, ColumnHeaderDefault);

			AppearanceHelper.Combine(AppearanceResults, Form.Properties.AppearanceDropDown, ResultsDefault);

			AppearanceHelper.Combine(AppearanceMatch, new[] { Form.Properties.AppearanceMatch, MatchDefault, AppearanceResults });
		}

		#region HSL
		class HSL {
			public HSL(double h, double s, double l) { H = h; S = s; L = l; }
			public HSL(Color c) : this(c.GetHue() / 360.0, c.GetSaturation(), c.GetBrightness()) { }

			double _h;
			double _s;
			double _l;

			public double H {
				get { return _h; }
				set { _h = Constrain(value); }
			}

			public double S {
				get { return _s; }
				set { _s = Constrain(value); }
			}

			public double L {
				get { return _l; }
				set { _l = Constrain(value); }
			}

			static double Constrain(double val) {
				if (val < 0) return 0;
				if (val > 1) return 1;
				return val;
			}

			/// <summary>
			/// Sets the absolute brightness of a color
			/// </summary>
			/// <param name="c">Original color</param>
			/// <param name="brightness">The luminance level to impose</param>
			/// <returns>an adjusted color</returns>
			public static Color SetBrightness(Color c, double brightness) {
				HSL hsl = new HSL(c);
				hsl.L = brightness;
				return hsl.GetColor();
			}

			/// <summary>
			/// Modifies an existing brightness level
			/// </summary>
			/// <remarks>
			/// To reduce brightness use a number smaller than 1. To increase brightness use a number larger tnan 1
			/// </remarks>
			/// <param name="c">The original color</param>
			/// <param name="brightness">The luminance delta</param>
			/// <returns>An adjusted color</returns>
			public static Color ModifyBrightness(Color c, double brightness) {
				HSL hsl = new HSL(c);
				hsl.L *= brightness;
				return hsl.GetColor();
			}

			/// <summary>
			/// Sets the absolute saturation level
			/// </summary>
			/// <remarks>Accepted values 0-1</remarks>
			/// <param name="c">An original color</param>
			/// <param name="Saturation">The saturation value to impose</param>
			/// <returns>An adjusted color</returns>
			public static Color SetSaturation(Color c, double Saturation) {
				HSL hsl = new HSL(c);
				hsl.S = Saturation;
				return hsl.GetColor();
			}

			/// <summary>
			/// Modifies an existing Saturation level
			/// </summary>
			/// <remarks>
			/// To reduce Saturation use a number smaller than 1. To increase Saturation use a number larger tnan 1
			/// </remarks>
			/// <param name="c">The original color</param>
			/// <param name="Saturation">The saturation delta</param>
			/// <returns>An adjusted color</returns>
			public static Color ModifySaturation(Color c, double Saturation) {
				HSL hsl = new HSL(c);
				hsl.S *= Saturation;
				return hsl.GetColor();
			}

			/// <summary>
			/// Sets the absolute Hue level
			/// </summary>
			/// <remarks>Accepted values 0-1</remarks>
			/// <param name="c">An original color</param>
			/// <param name="Hue">The Hue value to impose</param>
			/// <returns>An adjusted color</returns>
			public static Color SetHue(Color c, double Hue) {
				HSL hsl = new HSL(c);
				hsl.H = Hue;
				return hsl.GetColor();
			}

			/// <summary>
			/// Modifies an existing Hue level
			/// </summary>
			/// <remarks>
			/// To reduce Hue use a number smaller than 1. To increase Hue use a number larger than 1
			/// </remarks>
			/// <param name="c">The original color</param>
			/// <param name="Hue">The Hue delta</param>
			/// <returns>An adjusted color</returns>
			public static Color ModifyHue(Color c, double Hue) {
				HSL hsl = new HSL(c);
				hsl.H *= Hue;
				return hsl.GetColor();
			}

			/// <summary>
			/// Converts a color from HSL to RGB
			/// </summary>
			/// <remarks>Adapted from the algorithm in Foley and Van-Dam</remarks>
			/// <returns>A Color structure containing the equivalent RGB values</returns>
			public Color GetColor() {
				double r = 0, g = 0, b = 0;
				double temp1, temp2;

				if (L == 0) {
					r = g = b = 0;
				} else {
					if (S == 0) {
						r = g = b = L;
					} else {
						temp2 = ((L <= 0.5) ? L * (1.0 + S) : L + S - (L * S));
						temp1 = 2.0 * L - temp2;

						double[] t3 = new double[] { H + 1.0 / 3.0, H, H - 1.0 / 3.0 };
						double[] clr = new double[] { 0, 0, 0 };
						for (int i = 0; i < 3; i++) {
							if (t3[i] < 0)
								t3[i] += 1.0;
							if (t3[i] > 1)
								t3[i] -= 1.0;

							if (6.0 * t3[i] < 1.0)
								clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
							else if (2.0 * t3[i] < 1.0)
								clr[i] = temp2;
							else if (3.0 * t3[i] < 2.0)
								clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
							else
								clr[i] = temp1;
						}
						r = clr[0];
						g = clr[1];
						b = clr[2];
					}
				}

				return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));

			}
		}
		#endregion
		#endregion

		#region Layout
		///<summary>Gets the Y coordinate of the top of the viewport.</summary>
		public int ScrollTop {
			get { return Form.ScrollBar.Visible ? Form.ScrollBar.Value : 0; }
			set {
				if (ScrollTop == value) return;
				if (!Form.ScrollBar.Visible) throw new InvalidOperationException("Scrollbar is hidden");
				Form.ScrollBar.Value = value;
			}
		}

		///<summary>Gets the bounds of the portion of the results grid that displays rows.</summary>
		public Rectangle RowsArea { get; private set; }
		///<summary>Gets the bounds of the header row.</summary>
		public Rectangle HeaderArea { get; private set; }

		///<summary>Gets the padding applied to each cell.</summary>
		public int CellPaddingLeft { get { return HoverElement.ContentMargins.Left + 1; } }

		///<summary>Gets the bottom of the last visible row.</summary>
		///<remarks>If the last visible row is cut off, this will be in the middle of the row.
		///If the popup is too tall, this will be above the bottom of the row area.</remarks>
		public int RowsBottom { get { return RowsArea.Top + Math.Min(RowsArea.Height, Form.Items.Count * RowHeight); } }

		///<summary>Gets the height occupied by portions outside the results grid.</summary>
		public int VerticalFrameHeight { get; private set; }

		private void CalcRowHeight() {
			GInfo.AddGraphics(null);
			try {
				rowHeight = AppearanceMatch.CalcTextSize(GInfo.Graphics, "Wg", 0).ToSize().Height + HoverElement.ContentMargins.Height;
			} finally { GInfo.ReleaseGraphics(); }
		}

		int? rowHeight;
		///<summary>Gets the height of a single row.</summary>
		public int RowHeight {
			get {
				if (!rowHeight.HasValue) CalcRowHeight();
				return rowHeight.Value;
			}
		}

		///<summary>Gets the index of the first row that intersects the viewport.</summary>
		///<remarks>The row might not be entirely visible.</remarks>
		public int FirstVisibleRow { get { return ScrollTop / RowHeight; } }
		///<summary>Gets the number of rows that fit in the viewport.</summary>
		///<remarks>There may be more rows currently being displayed if the scroll position cuts off part of the top row.  (Allowing an additional row to be drawn below it)</remarks>
		public int MaxVisibleRows { get { return RowsArea.Height / RowHeight; } }	//This property is not completely accurate; if half the the top row is scrolled off-screen, one additional row will be visible.


		public IEnumerable<ResultColumn> VisibleColumns { get { return Form.Properties.Columns.Where(c => c.Visible); } }

		///<summary>Gets the onscreen Y coordinate of the given row index, relative to the popup form.</summary>
		///<remarks>This method respects the scrollbar.</remarks>
		public int GetRowCoordinate(int rowIndex) {
			return RowsArea.Top + (rowIndex * RowHeight) - ScrollTop;
		}

		///<summary>Gets the index of the row at the specified location (relative to the popup form).</summary>
		public int? GetRowIndex(Point pt) {
			if (!RowsArea.Contains(pt)) return null;

			var index = (pt.Y + ScrollTop - RowsArea.Top) / RowHeight;
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
				ScrollTop = (rowIndex + 1) * RowHeight - RowsArea.Height;	//Ensure that the bottom of the hover element is visible
		}

		protected override void CalcContentRect(Rectangle bounds) {
			base.CalcContentRect(bounds);

			var headerHeight = Form.Properties.ShowColumnHeaders ? 20 : 0;
			var rowAreaHeight = ContentRect.Height - headerHeight;

			Form.ScrollBar.Maximum = RowHeight * Form.Items.Count;
			Form.ScrollBar.LargeChange = rowAreaHeight;
			Form.ScrollBar.Visible = Form.ScrollBar.Maximum > rowAreaHeight;

			VerticalFrameHeight = (Form.Height - rowAreaHeight);

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
			Form.ScrollTo(Form.ScrollBar.Value);
		}
		#endregion

		#region Column Headers
		///<summary>Gets the pen used to draw vertical grid lines.</summary>
		public Pen LinePen { get; private set; }
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

		#region Item Hovering
		///<summary>Gets the SkinElement used to paint the background of the hovered row.</summary>
		public SkinElement HoverElement { get; private set; }
		int? hoveredIndex;
		ObjectState hoveredItemState;

		///<summary>Gets or sets the index of the currently hovered row.</summary>
		///<remarks>This row can be set using the mouse or the keyboard.
		///Clicking the row or pressing enter will select in as the popup's result.</remarks>
		public int? HoveredIndex {
			get { return hoveredIndex; }
			set {
				if (hoveredIndex == value) return;
				if (value < 0 || value >= Form.Items.Count) throw new ArgumentOutOfRangeException("value");

				hoveredIndex = value;

				if (HoveredIndex.HasValue)
					EnsureVisible(HoveredIndex.Value);

				Form.Invalidate();
			}
		}
		///<summary>Gets or sets the state of the hovered row.</summary>
		///<remarks>This is set by the mouse event handlers.</remarks>
		public ObjectState HoveredItemState {
			get { return hoveredItemState; }
			set {
				if (HoveredItemState == value) return;
				hoveredItemState = value;
				Form.Invalidate();
			}
		}
		#endregion
	}
	///<summary>Draws the ItemSelector's results popup.</summary>
	class ItemSelectorPopupFormPainter : PopupBaseSizeableFormPainter {
		protected override void DrawContent(PopupFormGraphicsInfoArgs info) {
			base.DrawContent(info);

			var vi = (ItemSelectorPopupFormViewInfo)info.ViewInfo;
			if (vi.Form.Properties.ShowColumnHeaders)
				DrawColumnHeaders(info);


			if (vi.Form.Properties.AllowResize)
				DrawInnerBorders(info);
			using (info.Cache.ClipInfo.SaveAndSetClip(vi.RowsArea)) {
				DrawHoverBackground(info);

				if (vi.Form.Properties.ShowVerticalLines)
					DrawVertLines(info);
				DrawRows(info);
			}
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

			var x = info.RowsArea.X;

			args.Graphics.DrawLine(info.LinePen,
				x, info.RowsArea.Top,
				x, info.RowsBottom
			);
			x--;
			for (int i = 0; i < info.ColumnHeaderArgs.Count; i++) {
				x += info.ColumnHeaderArgs[i].Bounds.Width;
				args.Graphics.DrawLine(info.LinePen,
					x, info.RowsArea.Top,
					x, info.RowsBottom
				);
			}
		}

		static void DrawHoverBackground(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			if (info.HoveredIndex == null) return;

			SkinElementInfo elemInfo = new SkinElementInfo(info.HoverElement,
				new Rectangle(info.RowsArea.X, info.GetRowCoordinate(info.HoveredIndex.Value), info.RowsArea.Width, info.RowHeight)
			);

			elemInfo.Cache = args.Cache;
			elemInfo.State = info.HoveredItemState;
			elemInfo.ImageIndex = info.HoveredItemState == ObjectState.Pressed ? 2 : 1;

			SkinElementPainter.Default.DrawObject(elemInfo);
		}

		static void DrawRows(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			for (int rowIndex = info.FirstVisibleRow; rowIndex < info.Form.Items.Count; rowIndex++) {
				int y = info.GetRowCoordinate(rowIndex);
				if (y > info.RowsArea.Bottom) break;

				DrawRow(args, rowIndex);
			}
		}
		static void DrawRow(PopupFormGraphicsInfoArgs args, int rowIndex) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			int x = info.RowsArea.X;
			foreach (var column in info.VisibleColumns) {
				DrawCell(args, rowIndex, column, x);

				x += column.Width;
				if (x > info.RowsArea.Right) break;
			}
		}

		static void DrawCell(PopupFormGraphicsInfoArgs args, int rowIndex, ResultColumn column, int x) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			//Subtract the padding from the left edge and the width.  (To preserve the right edge)
			x += info.CellPaddingLeft;
			var location = new Point(x, info.GetRowCoordinate(rowIndex));
			var cellWidth = Math.Min(column.Width - info.CellPaddingLeft, info.RowsArea.Right - info.HoverElement.ContentMargins.Right - x);
			var cellBounds = new Rectangle(location, new Size(cellWidth, info.RowHeight));

			var text = column.GetValue(info.Form.Items[rowIndex]);

			//If this cell matched the filter, find the matching prefix. 
			//I take the prefix from the cell value instead of the actual
			//word to allow for variations in case and hyphens.
			string matchedPart = null;
			if (column.ShouldFilter && info.FilterWords.Any()) {
				var match = info.FilterWords.FirstOrDefault(fw => ItemSelector.ValueMatches(filterWord: fw, columnValue: text));
				if (match != null)
					matchedPart = text.Substring(0, match.Length);	//The match might be the entire value
			}

			Brush colorOverride = null;
			if (rowIndex == info.HoveredIndex
			 && info.HoverElement.Color.ForeColor != info.AppearanceResults.GetForeColor())
				colorOverride = args.Cache.GetSolidBrush(info.HoverElement.Color.ForeColor);

			if (String.IsNullOrEmpty(matchedPart))
				info.AppearanceResults.DrawStringDefaultColor(args.Cache, text, cellBounds, colorOverride);
			else {
				info.AppearanceMatch.DrawStringDefaultColor(args.Cache, matchedPart, cellBounds, colorOverride);

				var matchSize = Size.Ceiling(info.AppearanceMatch.CalcTextSize(args.Cache, matchedPart, cellWidth));
				matchSize.Width++;	//DevExpress measures very aggressively
				cellBounds.X += matchSize.Width;
				cellBounds.Width -= matchSize.Width;

				info.AppearanceResults.DrawStringDefaultColor(args.Cache, text.Substring(matchedPart.Length), cellBounds, colorOverride);
			}
		}

		static void DrawInnerBorders(PopupFormGraphicsInfoArgs args) {
			var info = (ItemSelectorPopupFormViewInfo)args.ViewInfo;

			args.Graphics.DrawLine(info.LinePen,
				info.ContentRect.Left, info.RowsBottom,
				info.ContentRect.Right - 1, info.RowsBottom
			);
		}
	}
	static class Extensions {
		public static void DrawStringDefaultColor(this AppearanceObject app, GraphicsCache cache, string text, Rectangle bounds, Brush foreground) {
			if (foreground != null)
				app.DrawString(cache, text, bounds, foreground);
			else
				app.DrawString(cache, text, bounds);
		}
	}
}
