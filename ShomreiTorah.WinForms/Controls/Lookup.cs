using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;
using System.Globalization;

using DevExpress.XtraEditors;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using DevExpress.Utils.Win;
using System.Collections.ObjectModel;

namespace ShomreiTorah.WinForms.Controls {

	///<summary>A control that allows the user to select an item from a list.</summary>
	[DefaultEvent("ItemSelected")]
	[Description("A control that allows the user to select an item from a list.")]
	public partial class Lookup : XtraUserControl {
		//private class ScrollPanel : XtraScrollableControl { internal void RaiseScroll(MouseEventArgs e) { OnMouseWheel(e); } }

		DataView results = new DataView();
		int pMaxPopupHeight = 400;
		string pDefaultMessage = "Click here to see the directory, or type to search.";
		bool pPopupOpen;
		string pFilter = "";

		int pSelectedIndex = -1;
		ResultsLocation pResultsLocation = ResultsLocation.Top;

		///<summary>Occurs when a row is selected.</summary>
		public event EventHandler<ItemSelectionEventArgs> ItemSelected;
		///<summary>Raises the ItemSelected event.</summary>
		///<param name="e">An ItemSelectionEventArgs object that provides the event data.</param>
		internal protected virtual void OnItemSelected(ItemSelectionEventArgs e) {
			if (ItemSelected != null)
				ItemSelected(this, e);
		}


		///<summary>Creates a new Lookup instance.</summary>
		public Lookup() {
			InitializeComponent();
			RecreateBrushes(null, null);

			lineHeight = (int)matchFont.GetHeight();
			SetStyle(ControlStyles.Selectable, false);
			TabStop = false;

			input.MaskBox.PreviewKeyDown += MaskBox_PreviewKeyDown;
			input.ForeColor = Color.Gray;
			input.Text = pDefaultMessage;
			Controls.Remove(popupPanel);
			MinimumSize = Size.Empty;
			PopupHeight = 0;
		}

		#region Handle Table
		///<summary>Gets or sets the table that this control will search.</summary>
		[Description("Gets or sets the table that this control will search.")]
		[Category("Data")]
		public DataTable SearchTable {
			get { return results.Table; }
			set {
				if (results.Table != null) {
					results.Table.RowDeleted -= Table_RowChanged;
					results.Table.RowChanged -= Table_RowChanged;
					results.Table.TableCleared -= Table_TableCleared;
				}

				results.Table = value;

				if (results.Table != null) {
					results.Table.RowDeleted += Table_RowChanged;
					results.Table.RowChanged += Table_RowChanged;
					results.Table.TableCleared += Table_TableCleared;
					results.Sort = "LastName";
				}

				if (PopupOpen) {
					UpdateDisplay();
				}
			}
		}

		void Table_TableCleared(object sender, DataTableClearEventArgs e) {
			if (PopupOpen) {
				UpdateDisplay();
			}
		}

		void Table_RowChanged(object sender, DataRowChangeEventArgs e) {
			if (PopupOpen) {
				UpdateDisplay();
			}
		}
		#endregion
		#region Properties
		///<summary>Gets or sets the location of the results list.</summary>
		[Description("Gets or sets the location of the results list.")]
		[Category("Layout")]
		[DefaultValue(ResultsLocation.Top)]
		public ResultsLocation ResultsLocation {
			get { return pResultsLocation; }
			set { pResultsLocation = value; }
		}
		///<summary>Gets or sets the title of the text box.</summary>
		[Description("Gets or sets the title of the text box.")]
		[Category("Appearance")]
		[DefaultValue("Type a name:")]
		public string Caption {
			get { return Title.Text; }
			set {
				value = value ?? "";
				Title.Text = value;
				Title.Width = TextRenderer.MeasureText(value, Title.Font).Width + 9;
				Title.Visible = value.Length > 0;
			}
		}
		///<summary>Gets or sets the default text of the text box.</summary>
		[Description("Gets or sets the default text of the text box.")]
		[Category("Appearance")]
		[DefaultValue("Click here to see the directory, or type to search.")]
		public string DefaultText {
			get { return pDefaultMessage; }
			set {
				pDefaultMessage = value;
				if (!input.Focused)
					input.Text = pDefaultMessage;
			}
		}
		///<summary>Gets or sets the columns in the results grid.</summary>
		[Description("Gets or sets the columns in the results grid.")]
		[Category("Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Collection<ColumnInfo> Columns { get { return columns; } }
		///<summary>Gets or sets a fixed filter to apply to the table.</summary>
		[Description("Gets or sets a fixed filter to apply to the table.")]
		[DefaultValue("")]
		[Category("Data")]
		public string Filter {
			get { return pFilter; }
			set { pFilter = value ?? ""; RunFilter(); }
		}

		///<summary>Gets or sets the maximum height of the popup.</summary>
		[Description("Gets or sets the maximum height of the popup.")]
		[Category("Layout")]
		[DefaultValue(400)]
		public int MaxPopupHeight {
			get { return pMaxPopupHeight; }
			set {
				pMaxPopupHeight = value;
				if (DesignMode) return;

				if (PopupOpen) {
					//if (PopupHeight == Results.Count * LineHeight || PopupHeight > value)
					//    PopupHeight = Math.Min(Results.Count * LineHeight, value);
					if (PopupHeight > TargetPopupHeight)
						PopupHeight = TargetPopupHeight;
					UpdateDisplay();
				}
			}
		}

		///<summary>Gets or sets a value indicating whether the popup is open.</summary>
		[Browsable(false)]
		public bool PopupOpen {
			get { return pPopupOpen; }
			set {
				if (pPopupOpen == value) return;
				pPopupOpen = value;

				input.Text = value ? "" : pDefaultMessage;
				input.ForeColor = value ? Color.Black : Color.Gray;

				IsAutoScrolling = false;
				results.RowFilter = String.Empty;
				popupPanel.BringToFront();
				UpdateDisplay();
				resizeAnimator.Start();
				if (MaxPopupHeight == 0)
					XtraMessageBox.Show("This lookup doesn't have a MaxPopupHeight set.", "Lookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		///<summary>Gets or sets the index in the results view of the selected row.</summary>
		[Browsable(false)]
		public int SelectedIndex {
			get { return pSelectedIndex; }
			set {
				if (results.Count == 0) {
					pSelectedIndex = -1;
					return;
				}
				pSelectedIndex = Math.Max(Math.Min(value, results.Count - 1), 0);

				if (popupScroller.Visible) {
					if (lineHeight * SelectedIndex < popupScroller.Value)
						popupScroller.Value = lineHeight * (SelectedIndex - 0);
					else if (lineHeight * (SelectedIndex + 1) > popupScroller.Value + popupCanvas.Height)
						popupScroller.Value = lineHeight * (SelectedIndex + 1) - popupCanvas.Height;
					else
						popupCanvas.Invalidate();
				} else
					popupCanvas.Invalidate();
			}
		}
		#endregion
		#region Keyboard Events
		string oldText = "";
		Keys pressedKey;																						//The key that was last depressed.  Used in KeyPressTimer.

		void MaskBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if (e.KeyCode == Keys.Tab)
				e.IsInputKey = true;
		}
		private void Input_KeyDown(object sender, KeyEventArgs e) {
			if (IsAutoScrolling) {
				IsAutoScrolling = false;
				return;
			}
			pressedKey = e.KeyData;

			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab) {
				SelectRow();
				return;
			}
			if (e.KeyData == Keys.Escape) {
				Title.Focus();
				PopupOpen = false;
				return;
			}
			PopupOpen = true;

			if (e.KeyCode == Keys.Up
			 || e.KeyCode == Keys.Down
			 || e.KeyCode == Keys.PageUp
			 || e.KeyCode == Keys.PageDown
			 && results.Count != 0) {	//If the up or down keys were pressed, and there are one or more results,
				KeyPressTimer_Tick(null, null);											//Handle the initial keydown,
				e.SuppressKeyPress = true;
			}

			if ((e.KeyValue > 'z' && e.KeyData != Keys.OemMinus) || (
				!Char.IsControl((Char)e.KeyValue)
				&& !Char.IsLetter((Char)e.KeyValue)
				&& e.KeyData != Keys.OemMinus
				&& e.KeyCode != Keys.Space
				&& e.KeyCode != Keys.Delete
				&& e.KeyCode != Keys.Left
				&& e.KeyCode != Keys.Right
				&& e.KeyCode != Keys.Up
				&& e.KeyCode != Keys.Down
				&& e.KeyCode != Keys.PageDown
				&& e.KeyCode != Keys.PageUp
				&& e.KeyCode != Keys.Home
				&& e.KeyCode != Keys.End
				))
				e.SuppressKeyPress = true;
		}

		private void KeyPressTimer_Tick(object sender, EventArgs e) {
			if (SelectedIndex == -1 && results.Count != 0) {
				SelectedIndex = 0;
				popupScroller.Value = 0;
				return;
			}

			switch (pressedKey) {
				case Keys.Up:
					SelectedIndex--;
					break;
				case Keys.Down:
					SelectedIndex++;
					break;
				case Keys.PageUp:
					SelectedIndex -= popupCanvas.Height / lineHeight;
					break;
				case Keys.PageDown:
					SelectedIndex += popupCanvas.Height / lineHeight;
					break;
			}
		}

		private void Input_KeyPress(object sender, KeyPressEventArgs e) {
			if (input.SelectionStart == 0 || input.Text[input.SelectionStart - 1] == ' ')
				e.KeyChar = char.ToUpper(e.KeyChar, CultureInfo.CurrentUICulture);
			else
				e.KeyChar = char.ToLower(e.KeyChar, CultureInfo.CurrentUICulture);
		}

		private void Input_KeyUp(object sender, KeyEventArgs e) {
			keyPressTimer.Stop();
			if (input.Text == oldText) return;								//If the text hasn't changed, don't rerun the query.
			oldText = input.Text;
			RunFilter();
		}
		void RunFilter() {
			if (results == null) return;
			if (input.Text.Length == 0)
				results.RowFilter = Filter;
			else {
				string[] Words = input.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);	//Build the query string
				StringBuilder actualFilter = new StringBuilder();

				if (!String.IsNullOrEmpty(Filter))
					actualFilter.Append("(").Append(Filter).Append(")");

				foreach (string cWord in Words) {							//For each word,
					if (actualFilter.Length != 0) actualFilter.Append(" And ");			//If this isn't the first word, add an OR.
					actualFilter.Append(											//Add this word to the query.
						"(HisName  LIKE '").Append(cWord).Append("*' OR ").Append(
						 "HerName  LIKE '").Append(cWord).Append("*' OR ").Append(
						 "LastName LIKE '").Append(cWord).Append("*')");

					if (cWord.Contains("-")) {
						var spacedWord = cWord.Replace('-', ' ');
						actualFilter.Append(											//Add this word to the query.
							"OR (HisName   LIKE '").Append(spacedWord).Append("*' OR ").Append(
								 "HerName  LIKE '").Append(spacedWord).Append("*' OR ").Append(
								 "LastName LIKE '").Append(spacedWord).Append("*')");
					}

				}
				try {
					results.RowFilter = actualFilter.ToString();
				} catch (SyntaxErrorException) { return; } catch (EvaluateException) { return; }
			}
			SelectedIndex = 0;

			UpdateDisplay();
		}

		///<summary>Focuses the textbox.</summary>
		public new void Focus() { input.Focus(); }
		#endregion
		#region Mouse
		void SelectRow() {
			if (SelectedIndex < 0) return;
			OnItemSelected(new ItemSelectionEventArgs(results[SelectedIndex].Row));
			if (PopupOpen)
				input.Text = "";

			results.RowFilter = "";
			UpdateDisplay();
		}

		///<summary>Raises the System.Windows.Forms.Control.MouseWheel event.</summary>
		protected override void OnMouseWheel(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseWheel(e);
			if (resizeAnimator.Enabled) return;											//Disable mouse selection while the popup is resizing.

			ScrollPosition -= SystemInformation.MouseWheelScrollLines * lineHeight * Math.Sign(e.Delta);

			var mouseLoc = popupCanvas.PointToClient(PointToScreen(e.Location));
			if (popupCanvas.ClientRectangle.Contains(mouseLoc))
				SelectedIndex = (mouseLoc.Y + popupScroller.Value) / lineHeight;
		}
		private void Title_Click(object sender, EventArgs e) {
			if (!resizeAnimator.Enabled)
				PopupOpen = !PopupOpen;
		}
		private void Input_Click(object sender, EventArgs e) { PopupOpen = true; }


		bool sawMouseDown;
		private void popupCanvas_MouseDown(object sender, MouseEventArgs e) {
			IsAutoScrolling = e.Button == MouseButtons.Middle && !IsAutoScrolling;
			sawMouseDown = !IsAutoScrolling;
			if (!IsAutoScrolling)
				popupCanvas.Invalidate();
		}
		private void PopupCanvas_MouseMove(object sender, MouseEventArgs e) {
			if (resizeAnimator.Enabled) return;											//Disable mouse selection while the popup is resizing.

			if (e.Button == MouseButtons.Left && !IsAutoScrolling && (e.Y < 0 || e.Y > PopupHeight)) {	//If the user drags off the canvas,
				autoScrollTimer.Start();
				autoScrollRoot = popupCanvas.PointToScreen(new Point(popupCanvas.Width / 2, popupCanvas.Height / 2));
				isFakeAutoScrolling = true;
			} else if (isFakeAutoScrolling && popupCanvas.ClientRectangle.Contains(e.Location))	//If the user has dragged back onto the form,
				autoScrollTimer.Stop();

			if (!IsAutoScrolling && !resizeAnimator.Enabled)
				SelectedIndex = (e.Y + popupScroller.Value) / lineHeight;
			popupCanvas.Invalidate();
		}
		private void popupCanvas_MouseUp(object sender, MouseEventArgs e) {
			var m = Control.MousePosition;
			if (isFakeAutoScrolling) {
				SelectedIndex = popupScroller.Value / lineHeight;
				if (e.Y > 10)
					SelectedIndex += popupCanvas.Height / lineHeight - 1;
			}
			IsAutoScrolling = e.Button == MouseButtons.Middle
				&& sawMouseDown
				&& Math.Abs(autoScrollRoot.X - m.X) < 4
				&& Math.Abs(autoScrollRoot.Y - m.Y) < 4;	//Handle middle-clicks
			if (!IsAutoScrolling)
				popupCanvas.Invalidate();
		}
		private void PopupCanvas_MouseClick(object sender, MouseEventArgs e) {
			if (resizeAnimator.Enabled) return;											//Disable mouse selection while the popup is resizing.
			if (e.Button == MouseButtons.Left && sawMouseDown)
				SelectRow();
			sawMouseDown = false;
		}
		#endregion
		#region AutoScroll
		static Bitmap autoScrollIcon = Properties.Resources.AutoScroll;
		class AutoScrollWindow : TopFormBase {
			Lookup parent;
			public AutoScrollWindow(Lookup control) {
				Parent = null;
				TopLevel = true;
				ControlBox = false;
				StartPosition = FormStartPosition.Manual;
				FormBorderStyle = FormBorderStyle.None;
				ShowInTaskbar = false;
				base.SetStyle(ControlStyles.Selectable, false);

				parent = control;
				Size = autoScrollIcon.Size;
			}

			protected override void OnShown(EventArgs e) {
				base.OnShown(e);
				Capture = true;
				UpdateWindow();
			}
			protected override CreateParams CreateParams {
				get {
					CreateParams createParams = base.CreateParams;
					createParams.ExStyle |= NativeMethods.LayeredWindow;						//To support transparency, the window must be a layered window.
					return createParams;
				}
			}
			protected override void OnMouseUp(MouseEventArgs e) {
				base.OnMouseUp(e);
				Capture = true;
			}
			protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); Stop(); }
			int oldMouseY = Int32.MinValue;
			protected override void OnMouseMove(MouseEventArgs e) {
				base.OnMouseMove(e);
				if (parent.isFakeAutoScrolling) return;

				var mouseY = MousePosition.Y;
				if (mouseY == oldMouseY) return;
				var root = parent.autoScrollRoot;

				if (Math.Abs(mouseY - root.Y) < 5)
					Cursor = Cursor.Current = Cursors.NoMoveVert;
				else if (mouseY > root.Y)
					Cursor = Cursor.Current = Cursors.PanSouth;
				else
					Cursor = Cursor.Current = Cursors.PanNorth;

				oldMouseY = mouseY;
			}

			protected override void WndProc(ref Message m) {
				if (parent.IsAutoScrolling)
					Capture = true;
				base.WndProc(ref m);
			}
			void Stop() { parent.IsAutoScrolling = false; }

			static Point Zero = new Point(0, 0);
			[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]//, MessageId = "ListMaker.Splash+NativeMethods.ReleaseDC(System.IntPtr,System.IntPtr)")]
			void UpdateWindow() {
				IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
				IntPtr imageDC = NativeMethods.CreateCompatibleDC(screenDC);
				IntPtr gdiBitmap = IntPtr.Zero;
				IntPtr oldBitmap = IntPtr.Zero;

				try {
					gdiBitmap = autoScrollIcon.GetHbitmap(Color.FromArgb(0));						//Get a GDI handle to the image.
					oldBitmap = NativeMethods.SelectObject(imageDC, gdiBitmap);						//Select the image into the DC, and cache the old bitmap.

					Size size = autoScrollIcon.Size;												//Get the size and location of the form, as integers.
					Point location = Location;

					BlendFunction alphaInfo = new BlendFunction { SourceConstantAlpha = 255, AlphaFormat = 1 };	//This struct provides information about the opacity of the form.

					NativeMethods.UpdateLayeredWindow(Handle, screenDC, ref location, ref size, imageDC, ref Zero, 0, ref alphaInfo, UlwType.Alpha);
				} finally {
					NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);									//Release the Screen's DC.

					if (gdiBitmap != IntPtr.Zero) {													//If we got a GDI btimap,
						NativeMethods.SelectObject(imageDC, oldBitmap);								//Select the old bitmap into the DC
						NativeMethods.DeleteObject(gdiBitmap);										//Delete the GDI bitmap,
					}
					NativeMethods.DeleteDC(imageDC);												//And delete the DC.
				}
			}
		}
		AutoScrollWindow asWindow;
		MouseButtons autoScrollButton;
		Point autoScrollRoot;
		bool isFakeAutoScrolling;

		bool IsMouseInRoot(Point screenMouse) {
			return Math.Abs(autoScrollRoot.X - screenMouse.X) < 4
				&& Math.Abs(autoScrollRoot.Y - screenMouse.Y) < 4;	//Handle middle-clicks
		}

		bool IsAutoScrolling {
			get { return autoScrollTimer.Enabled; }
			set {
				if (IsAutoScrolling == value) return;
				if (value) {
					if (asWindow == null)
						asWindow = new AutoScrollWindow(this);
					autoScrollRoot = MousePosition;
					autoScrollButton = MouseButtons;

					asWindow.Location = new Point(autoScrollRoot.X - autoScrollIcon.Width / 2,
												  autoScrollRoot.Y - autoScrollIcon.Height / 2);
					asWindow.Show();
					asWindow.Capture = true;
				} else {
					Cursor.Current = Cursors.Default;
					if (asWindow != null) {
						asWindow.Close();
						asWindow = null;
					}
				}
				isFakeAutoScrolling = false;
				autoScrollTimer.Enabled = value;
				popupCanvas.Invalidate();
			}
		}

		private void autoScrollTimer_Tick(object sender, EventArgs e) {
			var mb = MouseButtons;
			if (!isFakeAutoScrolling && autoScrollButton != mb) {
				if (IsMouseInRoot(MousePosition) && mb == MouseButtons.None)
					autoScrollButton = mb;
				else {
					IsAutoScrolling = false;
					return;
				}
			}
			var mouseY = MousePosition.Y;
			if (Math.Abs(mouseY - autoScrollRoot.Y) > 5) {
				var delta = (mouseY - autoScrollRoot.Y) / (isFakeAutoScrolling ? 50 : 30);
				if (delta == 0) delta = Math.Sign(mouseY - autoScrollRoot.Y);
				ScrollPosition += delta;
			}
		}
		#endregion
		#region Painting
		Collection<ColumnInfo> columns = new Collection<ColumnInfo>(){				//Name, left, and width of each column displayed.
			new ColumnInfo("LastName", 7, 75),
			new ColumnInfo("HisName", 90, 75),
			new ColumnInfo("HerName", 165, 75),

			new ColumnInfo("Phone", 245, 120),
			new ColumnInfo("Address", 370, 160),

			new ColumnInfo("Zip", 540, 50)
		};

		LinearGradientBrush selResultBrush;											//Cached copy of the brush used to draw the selected text.
		Font selResultFont = new Font("Segoe UI", 10, FontStyle.Bold);				//The font used to draw the selected text.

		LinearGradientBrush resultBrush;											//Cached copy of the brush used to draw the text.
		Font listFont = new Font("Segoe UI", 10);									//The font used to draw the part of the text that matches the search.
		Font matchFont = new Font("Segoe UI", 10, FontStyle.Bold);					//The font used to draw the part of the text that matches the search.
		StringFormat resultFormat = new StringFormat {
			Trimming = StringTrimming.EllipsisCharacter
		};
		int lineHeight = -1;														//The height of each item in the list.

		int oldWidth = -1;
		private void RecreateBrushes(object sender, EventArgs e) {
			var width = Math.Min(popupCanvas.Width, Columns[Columns.Count - 1].Left + Columns[Columns.Count - 1].Width);
			if (popupCanvas.Width == 0 || width == oldWidth || DesignMode) return;

			if (resultBrush != null)
				resultBrush.Dispose();
			resultBrush = new LinearGradientBrush(
				new Rectangle(0, 0, width, lineHeight),
				Color.Black, Color.DarkBlue, LinearGradientMode.Horizontal);

			if (selResultBrush != null)
				selResultBrush.Dispose();
			selResultBrush = new LinearGradientBrush(
				new Rectangle(0, 0, width, lineHeight),
				Color.Blue, Color.DarkBlue, LinearGradientMode.Horizontal);
			oldWidth = width;
		}
		Skin commonSkin;
		private void PopupCanvas_Paint(object sender, PaintEventArgs e) {
			commonSkin = CommonSkins.GetSkin(UserLookAndFeel.Default);

			SkinElementInfo elemInfo = new SkinElementInfo(commonSkin[CommonSkins.SkinToolTipWindow], popupCanvas.ClientRectangle) {
				Cache = new GraphicsCache(e),
				State = ObjectState.Hot
			};
			SkinElementPainter.Default.DrawObject(elemInfo);

			if (DesignMode) return;

			string[] words = input.Text == DefaultText ? new string[0] : input.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			for (var i = popupScroller.Value / lineHeight;
				i < Math.Min(results.Count, (popupCanvas.Height + popupScroller.Value) / lineHeight + 1); i++) {	//For each visible row
				bool selected = i == SelectedIndex && !resizeAnimator.Enabled && !IsAutoScrolling;
				//bool afterSelected = SelectedIndex >= 0 && i > SelectedIndex && !resizeAnimator.Enabled && !IsAutoScrolling;
				DrawRow(e.Graphics, results[i].Row, words, i * lineHeight - popupScroller.Value, selected);
			}
		}
		void DrawRow(Graphics g, DataRow row, string[] words, int top, bool selected) {
			Font TextFont = (selected ? selResultFont : listFont);
			Brush TextBrush = (selected ? selResultBrush : resultBrush);

			if (selected) {																	//If this is the selected result, and an animation is not in progress, draw its background.
				SkinElement skinElem = commonSkin[CommonSkins.SkinButton];

				SkinElementInfo elemInfo = new SkinElementInfo(skinElem, new Rectangle(0, Math.Max(0, top), popupCanvas.Width, lineHeight));
				elemInfo.Cache = new GraphicsCache(g);
				elemInfo.State = (sawMouseDown && MouseButtons == MouseButtons.Left)
									? ObjectState.Hot | ObjectState.Pressed : ObjectState.Hot;
				new SkinButtonObjectPainter(UserLookAndFeel.Default).DrawObject(elemInfo);
			}
			if (words.Length == 0) {														//If the user has not entered a search,
				foreach (ColumnInfo cColumn in Columns) {									//For each column in the list of columns to draw,
					var cellText = String.Format(CultureInfo.CurrentUICulture, cColumn.FormatString, row[cColumn.FieldName]);
					g.DrawString(															//Draw each column using the correct brush and font.
						cellText,
						TextFont,
						TextBrush,
						new RectangleF(cColumn.Left, top, cColumn.Width, lineHeight),
						resultFormat);
				}
			} else {																		//If the user has entered a search
				using (LinearGradientBrush BoldBrush = new LinearGradientBrush(				//Create the brush used to highlight the matches.
					new Rectangle(0, top, popupCanvas.Width, lineHeight), Color.Purple, Color.Red, LinearGradientMode.Vertical)) {

					if (selected)															//If this is the selected result, and an animation is not in progress,
						BoldBrush.LinearColors = new Color[] { Color.Red, Color.DarkRed };	//Change the color scheme of the highlight brush.

					foreach (ColumnInfo cColumn in Columns) {								//For each column in the list of columns to draw,
						var cellText = String.Format(CultureInfo.CurrentUICulture, cColumn.FormatString, row[cColumn.FieldName]);
						string MatchString = "";
						float MatchWidth = 0;

						foreach (string cWord in words) {									//For each word,
							if (cWord.Length > MatchString.Length && cellText.Replace(' ', '-').StartsWith(cWord, StringComparison.OrdinalIgnoreCase)) 	//If this word matches the column, and is longer than the previous match,
								MatchString = cellText.ToString().Substring(0, cWord.Length);//Get the match.
						}

						if (MatchString.Length > 0) {										//If we found a match,
							MatchWidth = g.MeasureStringWidth(MatchString, matchFont);		//Get its width in its special font.

							g.DrawString(													//Draw the match.
								MatchString,
								matchFont,
								BoldBrush,
								new RectangleF(cColumn.Left, top, cColumn.Width, lineHeight),
								resultFormat);
						}
						g.DrawString(														//Draw the remainder (or, if it didn't match, all) of the column using the normal brush and font, offset to account for the width of the match.
							cellText.Substring(MatchWidth == 0 ? 0 : MatchString.Length),
							TextFont,
							TextBrush,
							new RectangleF(cColumn.Left + MatchWidth, top, cColumn.Width, lineHeight),
							resultFormat);
					}
				}
			}
		}
		#endregion
		#region Layout
		void UpdateDisplay() {
			if (results.Count * lineHeight >= MaxPopupHeight) {
				popupScroller.Visible = true;
				popupScroller.Maximum = results.Count * lineHeight;
				popupScroller.Value = Math.Min(popupScroller.Value, MaxScroll);
			} else {
				popupScroller.Visible = false;
				popupScroller.Maximum = 0;
			}
			if (PopupOpen) {
				popupCanvas.ResizeRedraw = TargetPopupHeight > PopupHeight;
				popupCanvas.Invalidate();
				resizeAnimator.Start();
			}
		}
		///<summary>Gets or sets the Y coordinate of the top of the visible portion of the results view in pixels.</summary>
		[Browsable(false)]
		public int ScrollPosition {
			get { return popupScroller.Value; }
			set {
				popupScroller.Value = Math.Min(Math.Max(value, 0), MaxScroll);
			}
		}
		int MaxScroll { get { return results.Count * lineHeight - popupCanvas.Height; } }
		private void popupScroller_ValueChanged(object sender, EventArgs e) { popupCanvas.Invalidate(); }
		#endregion
		#region Animation
		private void Input_Enter(object sender, EventArgs e) { PopupOpen = true; }
		private void Input_Leave(object sender, EventArgs e) { PopupOpen = false; }

		private void ResizeAnimator_Tick(object sender, EventArgs e) {
			double Step = (TargetPopupHeight - PopupHeight) / 10.0;
			if (Math.Abs(Step) < 4)
				Step = 4 * Math.Sign(Step);

			if (Math.Abs(PopupHeight + Step - TargetPopupHeight) <= 8) {
				PopupHeight = TargetPopupHeight;

				resizeAnimator.Stop();
				popupCanvas.Invalidate();
				popupCanvas.ResizeRedraw = false;

				return;
			}
			PopupHeight += (int)Math.Ceiling(Step);
			//popupCanvas.Invalidate();
		}

		int TargetPopupHeight { get { return PopupOpen ? Math.Min(results.Count * lineHeight, MaxPopupHeight) : 0; } }
		///<summary>Gets the current height of the results popup.</summary>
		[Browsable(false)]
		public int PopupHeight {
			get { return popupPanel.Height; }
			private set {
				popupScroller.LargeChange = value;
				if (ResultsLocation == ResultsLocation.Top)
					popupPanel.SetBounds(Left, popupPanel.Top - (value - PopupHeight), Width, value);
				else
					popupPanel.Height = value;
				OnPopupHeightChanged();
			}
		}
		///<summary>Occurs when popup's height is changed.</summary>
		public event EventHandler PopupHeightChanged;
		///<summary>Raises the PopupHeightChanged event.</summary>
		internal protected virtual void OnPopupHeightChanged() { OnPopupHeightChanged(EventArgs.Empty); }
		///<summary>Raises the PopupHeightChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnPopupHeightChanged(EventArgs e) {
			if (PopupHeightChanged != null)
				PopupHeightChanged(this, e);
		}

		///<summary>Raises the System.Windows.Forms.Control.ParentChanged event.</summary>
		protected override void OnParentChanged(EventArgs e) { base.OnParentChanged(e); if (!DesignMode) Parent.Controls.Add(popupPanel); }
		///<summary>Raises the System.Windows.Forms.Control.SizeChanged event.</summary>
		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			Height = 20;
			if (!DesignMode) popupPanel.Width = Width;
		}
		///<summary>Raises the System.Windows.Forms.Control.LocationChanged event.</summary>
		protected override void OnLocationChanged(EventArgs e) {
			base.OnLocationChanged(e);
			if (DesignMode) return;
			popupPanel.Location = new Point(Left, ResultsLocation == ResultsLocation.Top ? Top - popupPanel.Height : Bottom);
		}
		///<summary>Raises the System.Windows.Forms.Control.Layout event.</summary>
		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
			if (ResultsLocation == ResultsLocation.Top)
				popupPanel.Top = Top - popupPanel.Height;
			else
				popupPanel.Top = Bottom;
		}
		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (components != null) components.Dispose();
				if (asWindow != null) asWindow.Dispose();
				listFont.Dispose();
				matchFont.Dispose();
				resultBrush.Dispose();
				resultFormat.Dispose();
				selResultBrush.Dispose();
				selResultFont.Dispose();
			}
			base.Dispose(disposing);
		}
	}
	///<summary>Contains information about a column in the results.</summary>
	public struct ColumnInfo : IEquatable<ColumnInfo> {
		///<summary>Gets the name of the column's data field.</summary>
		public string FieldName { get; private set; }
		///<summary>Gets the column's X coordinate in pixels.</summary>
		public int Left { get; private set; }
		///<summary>Gets the column's width in pixels.</summary>
		public int Width { get; private set; }
		///<summary>Gets the format string applied to the column's data.</summary>
		public string FormatString { get; private set; }

		///<summary>Gets the X coordinate of the column's right edge in pixels.</summary>
		public int Right { get { return Left + Width; } }

		///<summary>Creates a new ColumnInfo instance.</summary>
		///<param name="columnName">The name of the column's data field.</param>
		///<param name="left">The column's X coordinate in pixels.</param>
		///<param name="width">The column's width in pixels.</param>
		public ColumnInfo(string columnName, int left, int width) : this(columnName, left, width, "{0}") { }
		///<summary>Creates a new ColumnInfo instance.</summary>
		///<param name="columnName">The name of the column's data field.</param>
		///<param name="left">The column's X coordinate in pixels.</param>
		///<param name="width">The column's width in pixels.</param>
		///<param name="format">The format string applied to the column's data.</param>
		public ColumnInfo(string columnName, int left, int width, string format) : this() { FieldName = columnName; Left = left; Width = width; FormatString = format; }

		#region Equality
		///<summary>Checks whether this instance is equal to another object.</summary>
		public override bool Equals(object obj) { return obj is ColumnInfo && Equals((ColumnInfo)obj); }
		///<summary>Checks whether this instance is equal to another ColumnInfo object.</summary>
		public bool Equals(ColumnInfo other) { return FieldName == other.FieldName && Left == other.Left && Width == other.Width && FormatString == other.FormatString; }

		///<summary>Returns a hash value that uniquely describes this instance.</summary>
		public override int GetHashCode() { return (FieldName ?? "").GetHashCode() ^ Left.GetHashCode() ^ Width.GetHashCode() ^ (FormatString ?? "").GetHashCode(); }

		///<summary>CHecks whether two ColumnInfo values are equal.</summary>
		public static bool operator ==(ColumnInfo first, ColumnInfo second) { return first.Equals(second); }
		///<summary>CHecks whether two ColumnInfo values are unequal.</summary>
		public static bool operator !=(ColumnInfo first, ColumnInfo second) { return !first.Equals(second); }
		#endregion
	}

	///<summary>Specifies the location of the list of matches in the lookup control.</summary>
	public enum ResultsLocation {
		///<summary>The results appear above the textbox.</summary>
		Top,
		///<summary>The results appear below the textbox.</summary>
		Bottom
	}
	///<summary>Provides data for the ItemSelected event.</summary>
	public class ItemSelectionEventArgs : EventArgs {

		///<summary>Creates an ItemSelectionEventArgs for a DataRow.</summary>
		public ItemSelectionEventArgs(DataRow row) { SelectedRow = row; }
		///<Summary>Gets the selected row.</Summary>
		public DataRow SelectedRow { get; private set; }
	}
}

