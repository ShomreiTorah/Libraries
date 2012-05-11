using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.WinForms.Controls {
	///<summary>An interactive Hebrew calendar.</summary>
	[Description("An interactive Hebrew calendar.")]
	[ToolboxBitmap(typeof(HebrewCalendar), "Images.Calendar.png")]
	[ToolboxItem(true)]
	[DefaultEvent("SelectionChanged")]
	[DefaultProperty("Mode")]
	public class HebrewCalendar : SimpleControl {
		static CultureInfo Culture { get { return CultureInfo.CurrentCulture; } }

		///<summary>Creates a new HebrewCalendar instance.</summary>
		[SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "Only runs while mouse is down")]
		public HebrewCalendar() {
			SetStyle(ControlStyles.AllPaintingInWmPaint
			   | ControlStyles.UserPaint
			   | ControlStyles.Opaque
			   | ControlStyles.OptimizedDoubleBuffer
			   | ControlStyles.ResizeRedraw
			   | ControlStyles.Selectable,
				 true);

			TodayFont = new Font(Font, FontStyle.Bold);
			Painter = new WhiteCalendarPainter(this);
			ContentRenderer = new DefaultContentRenderer(Painter);
			toolTipCreator = new DXToolTipCreator(this);

			SetView(HebrewDate.Today, moveSelection: false);

			navigateTimer = new Timer { Interval = 500 };
			navigateTimer.Tick += navigateTimer_Tick;

			hoverTimer = new Timer { Interval = SystemInformation.MouseHoverTime };
			hoverTimer.Tick += hoverTimer_Tick;
		}

		CalendarType mode;
		int hebrewYear, englishYear, englishMonth;
		HebrewMonth hebrewMonth;

		#region Designer Properties
		///<summary>Gets or sets the type of calendar to display.</summary>
		[Description("Gets or sets the type of calendar to display.")]
		[Category("Appearance")]
		[DefaultValue(CalendarType.English)]
		[RefreshProperties(RefreshProperties.All)]
		public CalendarType Mode {
			get { return mode; }
			set {
				mode = value;
				if (value == CalendarType.English)	//TODO: If an extremity is selected, the selection may move off-screen
					SetView(new DateTime(EnglishYear, EnglishMonth, 1), moveSelection: false);
				else
					SetView(new HebrewDate(HebrewYear, HebrewMonth, 1), moveSelection: false);
			}
		}

		///<summary>Gets or sets the Hebrew year being displayed.</summary>
		[Description("Gets or sets the Hebrew year being displayed.")]
		[Category("Behavior")]
		[RefreshProperties(RefreshProperties.All)]
		public int HebrewYear {
			get { return hebrewYear; }
			set { mode = CalendarType.Hebrew; SetView(new HebrewDate(value, HebrewMonth, 1), moveSelection: true); }
		}
		///<summary>Gets or sets the Hebrew month being displayed.</summary>
		[Description("Gets or sets the Hebrew month being displayed.")]
		[Category("Behavior")]
		[RefreshProperties(RefreshProperties.All)]
		public HebrewMonth HebrewMonth {
			get { return hebrewMonth; }
			set { mode = CalendarType.Hebrew; SetView(new HebrewDate(HebrewYear, value, 1), moveSelection: true); }
		}
		///<summary>Gets or sets the English year being displayed.</summary>
		[Description("Gets or sets the English year being displayed.")]
		[Category("Behavior")]
		[RefreshProperties(RefreshProperties.All)]
		public int EnglishYear {
			get { return englishYear; }
			set { mode = CalendarType.English; SetView(new DateTime(value, EnglishMonth, 1), moveSelection: true); }
		}
		///<summary>Gets or sets the English month being displayed.</summary>
		[Description("Gets or sets the English month being displayed.")]
		[Category("Behavior")]
		[RefreshProperties(RefreshProperties.All)]
		public int EnglishMonth {
			get { return englishMonth; }
			set { mode = CalendarType.English; SetView(new DateTime(EnglishYear, value, 1), moveSelection: true); }
		}

		private void ResetHebrewMonth() { SetView(HebrewDate.Today, moveSelection: true); }
		private void ResetHebrewYear() { SetView(HebrewDate.Today, moveSelection: true); }
		private void ResetEnglishYear() { SetView(HebrewDate.Today, moveSelection: true); }
		private void ResetEnglishMonth() { SetView(HebrewDate.Today, moveSelection: true); }


		private bool ShouldSerializeHebrewYear() { return Mode == CalendarType.Hebrew && !IsSameHebrewMonth(HebrewDate.Today); }
		private bool ShouldSerializeHebrewMonth() { return ShouldSerializeHebrewYear(); }

		private bool ShouldSerializeEnglishYear() { return Mode == CalendarType.English && !IsSameEnglishMonth(DateTime.Today); }
		private bool ShouldSerializeEnglishMonth() { return ShouldSerializeEnglishYear(); }
		#endregion

		#region Date logic
		///<summary>Gets or sets the selected date.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public HebrewDate? SelectedDate {
			get { return selectedDate; }
			set { SetSelection(value, true); }
		}
		const int RowCount = 6;

		HebrewDate? selectedDate;

		///<summary>Gets the beginning of the primary month being displayed.</summary>
		[Browsable(false)]
		public HebrewDate MonthStart { get { return Mode == CalendarType.Hebrew ? new HebrewDate(HebrewYear, HebrewMonth, 1) : new HebrewDate(new DateTime(EnglishYear, EnglishMonth, 1)); } }

		///<summary>The date displayed in the first cell.</summary>
		HebrewDate firstDate;

		///<summary>Displays the month containing a specific date.</summary>
		///<param name="date">The date to focus on in the view.</param>
		///<param name="moveSelection">If true, the selection will be moved to remain in its current cell (visual position).
		///This should only be false if the currently selected date is guaranteed to remain visible in the new view.</param>
		public void SetView(HebrewDate date, bool moveSelection) {
			var selectionOffset = selectedDate.HasValue ? (selectedDate.Value - firstDate).Days : -1;
			if (selectionOffset > RowCount * 7) selectionOffset = -1;

			if (mode == CalendarType.English)
				date = new DateTime(date.EnglishDate.Year, date.EnglishDate.Month, 1);
			else
				date = new HebrewDate(date.HebrewYear, date.HebrewMonth, 1);
			firstDate = date.Last(DayOfWeek.Sunday);
			if (Mode == CalendarType.English) {
				englishYear = date.EnglishDate.Year;
				englishMonth = date.EnglishDate.Month;

				//Find the dominant Hebrew month
				HebrewDate hebrewDate;
				if (firstDate.HebrewDay == 1)
					hebrewDate = firstDate;
				else {
					//Get the beginning of the next month
					hebrewDate = firstDate + (1 + firstDate.MonthLength - firstDate.HebrewDay);
					//If it's more than halfway into the display, use the previous one.
					if (hebrewDate > firstDate.AddDays(1 + 7 * RowCount / 2))
						hebrewDate = hebrewDate.AddMonths(-1);
				}

				hebrewYear = hebrewDate.HebrewYear;
				hebrewMonth = hebrewDate.HebrewMonth;
			} else {	//Mode == CalendarType.Hebrew
				hebrewYear = date.HebrewYear;
				hebrewMonth = date.HebrewMonth;

				//Find the dominant English month
				DateTime englishDate = firstDate;
				if (englishDate.Day > 1) {
					//Get the beginning of the next month
					englishDate = englishDate.AddDays(1 + DateTime.DaysInMonth(englishDate.Year, englishDate.Month) - englishDate.Day);

					//If it's more than halfway into the display, use the previous one.
					if (englishDate > firstDate.AddDays(1 + 7 * RowCount / 2))
						englishDate = englishDate.AddMonths(-1);
				}

				englishYear = englishDate.Year;
				englishMonth = englishDate.Month;
			}
			//If the mouse is over a date, the date needs to be updated.
			if (hoverItem.Item == CalendarItem.DayCell)
				hoverItem = HitTest(PointToClient(MousePosition));

			if (moveSelection) {
				if (selectionOffset >= 0)
					selectedDate = firstDate + selectionOffset;
				else if (selectedDate.HasValue && (selectedDate.Value < firstDate || selectedDate.Value > (firstDate + RowCount * 7)))
					selectedDate = null;
			} else		//If we're not moving the selection, it ought to still be visible.
				Debug.Assert(selectedDate == null || IsVisible(selectedDate.Value));

			Invalidate();
			OnSelectionChanged();
		}

		///<summary>Sets the selection, optionally changing the view.</summary>
		///<param name="date">The date to select.  If this parameter is null, the selection will be cleared.</param>
		///<param name="setView">If true, the view will be changed even if the new selection is within the previous or next month in the current view.</param>
		public void SetSelection(HebrewDate? date, bool setView) {
			selectedDate = date;
			if (date != null && (setView || !IsVisible(date.Value)))
				SetView(date.Value, moveSelection: false);
			else {
				Invalidate();			//SetView calls Invalidate and OnSelectionChanged; if we didn't call SetView, we have to invalidate by ourselves.
				OnSelectionChanged();
			}
		}

		///<summary>Moves the view by a number of months.</summary>
		public void OffsetMonths(int months) {
			if (Mode == CalendarType.Hebrew)
				SetView(MonthStart.AddMonths(months), moveSelection: true);
			else
				SetView(MonthStart.EnglishDate.AddMonths(months), moveSelection: true);
		}
		///<summary>Moves the view by a number of years.</summary>
		public void OffsetYears(int years) {
			if (Mode == CalendarType.Hebrew)
				SetView(MonthStart.AddYears(years), moveSelection: true);
			else
				SetView(MonthStart.EnglishDate.AddYears(years), moveSelection: true);
		}

		///<summary>Checks whether the current view includes the given date.</summary>
		public bool IsVisible(DateTime date) {
			return date >= firstDate && date <= firstDate + RowCount * 7;
		}

		///<summary>Checks whether a date is in the primary month being displayed by the calendar.</summary>
		public bool IsSameMonth(HebrewDate date) { return Mode == CalendarType.English ? IsSameEnglishMonth(date) : IsSameHebrewMonth(date); }
		bool IsSameHebrewMonth(HebrewDate date) { return date.HebrewYear == HebrewYear && date.HebrewMonth == HebrewMonth; }
		bool IsSameEnglishMonth(DateTime date) { return date.Year == EnglishYear && date.Month == EnglishMonth; }
		#endregion

		#region Painting
		///<summary>Gets the font used to draw the today's cell.</summary>
		public Font TodayFont { get; private set; }
		///<summary>Raises the FontChanged event.</summary>
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged(e);
			if (TodayFont != null) TodayFont.Dispose();
			TodayFont = new Font(Font, FontStyle.Bold);
		}

		BaseContentRenderer contentRenderer;
		///<summary>Gets or sets a custom content renderer for this calendar.</summary>
		public BaseContentRenderer ContentRenderer {
			get { return contentRenderer; }
			set {
				contentRenderer = value ?? new DefaultContentRenderer(Painter);
			}
		}
		///<summary>Gets this calendar's painter.  This value should be passed to the BaseContentRenderer constructor.</summary>
		public ICalendarPainter CalendarPainter { get { return Painter; } }
		abstract class BaseCalendarPainter : ICalendarPainter, IDisposable {
			public HebrewCalendar Calendar { get; protected set; }
			//CS1690: Accessing a member on 'ShomreiTorah.WinForms.Controls.HebrewCalendar.hoverDate' may cause a runtime exception because it is a field of a marshal-by-reference class
			public CalendarHitTestInfo HoverItem { get { return Calendar.hoverItem; } }

			protected BaseCalendarPainter(HebrewCalendar calendar) { Calendar = calendar; }

			#region Layout Properties
			Rectangle calendarBounds;
			public Rectangle CalendarBounds {
				get { return calendarBounds; }
				set {
					if (value.Width < 215) value.Width = 215;
					if (value.Height < 168) value.Height = 168;

					if (calendarBounds == value) return;
					calendarBounds = value;
					PerformLayout();
				}
			}
			public Rectangle ContentBounds { get { return CalendarBounds.DeflateBy(ContentPadding); } }

			public int MonthHeaderHeight { get; protected set; }
			protected virtual int WeekHeaderHeight { get { return 20; } }
			protected virtual int GridTop { get { return MonthHeaderHeight + WeekHeaderHeight; } }
			protected virtual Padding ContentPadding { get { return Padding.Empty; } }

			protected virtual Padding CellPadding { get { return Padding.Empty; } }

			///<summary>Gets the size of the buttons used to switch months.</summary>
			public abstract Size ButtonSize { get; }
			///<summary>Gets the bounds of the button used to switch to the previous month.</summary>
			public Rectangle PreviousButton { get; protected set; }
			///<summary>Gets the bounds of the button used to switch to the next month.</summary>
			public Rectangle NextButton { get; protected set; }

			protected Rectangle MonthHeaderBounds { get { return new Rectangle(ContentBounds.Location, new Size(ContentBounds.Width, MonthHeaderHeight)); } }
			protected Rectangle WeekHeaderBounds { get { return new Rectangle(ContentBounds.X, ContentBounds.Y + MonthHeaderHeight, ContentBounds.Width, WeekHeaderHeight); } }

			/// <summary>The total area of the grid containing the days.</summary>
			public Rectangle GridArea { get; private set; }
			/// <summary>The size of the cell for a single day.</summary>
			public Size DaySize { get; private set; }
			/// <summary>The bounds of the cell for a single day, at location (0, 0).</summary>
			public Rectangle DayBounds { get; private set; }
			/// <summary>The bounds of the content of a cell for a single day, at location (0, 0).</summary>
			public Rectangle DayContentBounds { get { return DayBounds.DeflateBy(CellPadding); } }
			#endregion

			public BaseContentRenderer CellRenderer { get { return Calendar.ContentRenderer; } }

			///<summary>Initializes this painter to paint a new size.</summary>
			protected virtual void PerformLayout() {
				MonthHeaderHeight = 24;

				calendarBounds.Width -= ContentBounds.Width % 7;
				calendarBounds.Height -= (ContentBounds.Height - GridTop) % RowCount;
				if (Calendar.Size != calendarBounds.Size)
					Calendar.Size = calendarBounds.Size;

				GridArea = new Rectangle(ContentBounds.X, ContentBounds.Y + GridTop, ContentBounds.Width, ContentBounds.Height - GridTop);
				DaySize = new Size(GridArea.Width / 7, GridArea.Height / RowCount);

				var buttonY = MonthHeaderBounds.Y + (MonthHeaderBounds.Height - ButtonSize.Height) / 2;

				PreviousButton = new Rectangle((int)(MonthHeaderBounds.Left + ButtonSize.Width * .5), buttonY, ButtonSize.Width, ButtonSize.Height);
				NextButton = new Rectangle((int)(MonthHeaderBounds.Right - ButtonSize.Width * 1.5), buttonY, ButtonSize.Width, ButtonSize.Height);

				DayBounds = new Rectangle(Point.Empty, DaySize);
			}

			public virtual CalendarHitTestInfo HitTest(Point location) {
				var column = (location.X - GridArea.Left) / DaySize.Width;
				var row = (location.Y - GridArea.Top) / DaySize.Height;

				if (row < 0) row = 0;
				if (column < 0) column = 0;
				if (row >= RowCount) row = RowCount - 1;
				if (column >= 7) column = 6;

				var nearestDate = Calendar.firstDate + (column + 7 * row);

				if (GridArea.Contains(location))	//If the point was actually on the cell,
					return new CalendarHitTestInfo(nearestDate);

				if (WeekHeaderBounds.Contains(location))
					return new CalendarHitTestInfo((DayOfWeek)((7 * (location.X - WeekHeaderBounds.X)) / WeekHeaderBounds.Width), nearestDate);

				if (PreviousButton.Contains(location))
					return new CalendarHitTestInfo(CalendarItem.PreviousButton, nearestDate);
				if (NextButton.Contains(location))
					return new CalendarHitTestInfo(CalendarItem.NextButton, nearestDate);

				return new CalendarHitTestInfo(CalendarItem.None, nearestDate);
			}
			public virtual Rectangle GetDayBounds(HebrewDate date) {
				if (date < Calendar.firstDate || date > Calendar.firstDate + RowCount * 7) throw new ArgumentOutOfRangeException("date");
				var row = (date - Calendar.firstDate).Days / 7;
				var column = (int)date.DayOfWeek;
				return new Rectangle(new Point(GridArea.X + column * DaySize.Width, GridArea.Y + row * DaySize.Height), DaySize);
			}
			public virtual Point GetToolTipLocation(HebrewDate date) {
				var bounds = GetDayBounds(date);
				return new Point(bounds.Left, bounds.Bottom);
			}

			public virtual void Render(Graphics g, Rectangle clipRectangle) {
				CellRenderer.OnBeginPaint();
				DrawBackground(g);

				if (clipRectangle.IntersectsWith(MonthHeaderBounds))
					DrawMonthHeader(g);

				if (clipRectangle.IntersectsWith(PreviousButton))
					DrawPreviousButton(g, ObjectState.Normal);
				if (clipRectangle.IntersectsWith(NextButton))
					DrawNextButton(g, ObjectState.Normal);

				for (HebrewDate date = Calendar.firstDate, lastDay = Calendar.firstDate + RowCount * 7; date < lastDay; date++) {
					RenderDay(g, date);
				}

				if (clipRectangle.IntersectsWith(WeekHeaderBounds))
					DrawWeekHeader(g);
			}
			protected virtual void RenderDay(Graphics g, HebrewDate date) {
				var dayRect = GetDayBounds(date);

				if (!g.ClipBounds.IntersectsWith(dayRect)) return;

				g.TranslateTransform(dayRect.X, dayRect.Y);

				var isSelected = date == (Calendar.selectingDate ?? Calendar.selectedDate);

				DrawDay(g, date, isSelected);
				g.ResetTransform();
			}

			protected abstract void DrawPreviousButton(Graphics g, ObjectState state);
			protected abstract void DrawNextButton(Graphics g, ObjectState state);
			protected abstract void DrawBackground(Graphics g);
			protected abstract void DrawMonthHeader(Graphics g);
			protected abstract void DrawWeekHeader(Graphics g);

			protected static readonly ReadOnlyCollection<string> ShortDayNames = new ReadOnlyCollection<string>(Makeשבת(Culture.DateTimeFormat.AbbreviatedDayNames));
			protected static readonly ReadOnlyCollection<string> LongDayNames = new ReadOnlyCollection<string>(Makeשבת(Culture.DateTimeFormat.DayNames));

			static string[] Makeשבת(string[] dayNames) { dayNames[6] = "שבת"; return dayNames; }

			protected virtual void DrawDay(Graphics g, HebrewDate date, bool isSelected) {
				CellRenderer.DrawCell(g, date, isSelected);
			}

			///<summary>Releases all resources used by the CalendarPainter.</summary>
			public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
			///<summary>Releases the unmanaged resources used by the CalendarPainter and optionally releases the managed resources.</summary>
			///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
			protected virtual void Dispose(bool disposing) { }
		}

		///<summary>Draws the UI elements of the calendar (everything except the date cells) using DevExpress skins.</summary>
		abstract class SkinChromeCalendarPainter : BaseCalendarPainter {
			[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
			protected SkinChromeCalendarPainter(HebrewCalendar calendar)
				: base(calendar) {
				calendar.LookAndFeel.StyleChanged += delegate { OnStyleChanged(); };
				OnStyleChanged();
			}
			EditorButtonPainter buttonPainter;
			SkinElement background;
			Color titleColor;
			protected virtual void OnStyleChanged() {
				background = CommonSkins.GetSkin(Calendar.LookAndFeel)[CommonSkins.SkinToolTipWindow];
				buttonPainter = new EditorButtonPainter(Calendar.LookAndFeel.Painter.Button);
				titleColor = LookAndFeelHelper.GetSystemColor(Calendar.LookAndFeel, SystemColors.InfoText);
				CreateHeaders();
			}

			protected override void DrawBackground(Graphics g) {
				var elemInfo = new SkinElementInfo(background, CalendarBounds) { Cache = new GraphicsCache(g) };
				SkinElementPainter.Default.DrawObject(elemInfo);
			}
			protected override Padding ContentPadding { get { return new Padding(4, 4, 5, 5); } }
			protected override Padding CellPadding { get { return new Padding(2, 3, 2, 2); } }
			public override Size ButtonSize { get { return new Size(18, 20); } }

			static readonly Font MonthHeaderFont = new Font("Segoe UI", 14, FontStyle.Bold);
			protected override void DrawMonthHeader(Graphics g) {
				if (Calendar.Mode == CalendarType.English)
					TextRenderer.DrawText(g, Calendar.MonthStart.EnglishDate.ToString("MMMM  yyyy", Culture), MonthHeaderFont, MonthHeaderBounds, titleColor,
										  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				else
					TextRenderer.DrawText(g, Calendar.MonthStart.ToString("MMMM  yyyy"), MonthHeaderFont, MonthHeaderBounds, titleColor,
										  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
			}

			static readonly EditorButton ebPrevious = new EditorButton(ButtonPredefines.Left);
			protected override void DrawPreviousButton(Graphics g, ObjectState state) {
				var args = new EditorButtonObjectInfoArgs(ebPrevious, AppearanceObject.ControlAppearance) { Bounds = PreviousButton, Graphics = g };
				if (Calendar.NavigateButtonDirection == -1) {
					//if (PreviousButton.Contains(Calendar.PointToClient(MousePosition)))
					args.State = ObjectState.Pressed;
				} else if (HoverItem.Item == CalendarItem.PreviousButton)
					args.State = ObjectState.Hot;

				buttonPainter.DrawObject(args);
			}

			static readonly EditorButton ebNext = new EditorButton(ButtonPredefines.Right);
			protected override void DrawNextButton(Graphics g, ObjectState state) {
				var args = new EditorButtonObjectInfoArgs(ebNext, AppearanceObject.ControlAppearance) { Bounds = NextButton, Graphics = g };

				if (Calendar.NavigateButtonDirection == 1) {
					//if (NextButton.Contains(Calendar.PointToClient(MousePosition)))
					args.State = ObjectState.Pressed;
				} else if (HoverItem.Item == CalendarItem.NextButton)
					args.State = ObjectState.Hot;

				buttonPainter.DrawObject(args);
			}

			protected override void PerformLayout() {
				base.PerformLayout();
				CreateHeaders();
			}
			void CreateHeaders() {

				var cellWidth = WeekHeaderBounds.Width / 7;
				var dayNames = cellWidth > 65 ? LongDayNames : ShortDayNames;

				//For some reason, the first header is one pixel too narrow
				weekHeaderArgs = dayNames.Select((c, i) => new HeaderObjectInfoArgs {
					Caption = c,
					Bounds = new Rectangle(WeekHeaderBounds.X + cellWidth * i + Math.Sign(i), WeekHeaderBounds.Y, cellWidth + 1 - Math.Sign(i), WeekHeaderBounds.Height)
				}).ToArray();
				weekHeaderArgs.First().HeaderPosition = HeaderPositionKind.Left;
				weekHeaderArgs.Last().HeaderPosition = HeaderPositionKind.Right;

				var headerAppearance = new AppearanceObject(GridSkins.GetSkin(Calendar.LookAndFeel)[GridSkins.SkinHeader].GetAppearanceDefault());
				headerAppearance.TextOptions.HAlignment = HorzAlignment.Center;
				headerAppearance.TextOptions.VAlignment = VertAlignment.Top;

				foreach (var header in weekHeaderArgs) {
					header.SetAppearance(headerAppearance);
					Calendar.LookAndFeel.Painter.Header.CalcObjectBounds(header);
				}
			}
			HeaderObjectInfoArgs[] weekHeaderArgs;
			protected override void DrawWeekHeader(Graphics g) {
				var hoverDay = Calendar.selectingDate ?? HoverItem.Date;

				for (int i = 0; i < weekHeaderArgs.Length; i++) {
					var args = weekHeaderArgs[i];

					if (hoverDay.HasValue && (int)hoverDay.Value.DayOfWeek == i)
						args.State = ObjectState.Hot;
					else
						args.State = ObjectState.Normal;

					args.Graphics = g;
					Calendar.LookAndFeel.Painter.Header.DrawObject(args);
					args.Graphics = null;
				}
				var lnf = Calendar.LookAndFeel;
				//These skins have header elements with no outer border.
				if (lnf.ActiveStyle == ActiveLookAndFeelStyle.Skin
					&& (lnf.ActiveSkinName.StartsWith("Office 2010", StringComparison.OrdinalIgnoreCase)
					 || lnf.ActiveSkinName.StartsWith("Seven", StringComparison.OrdinalIgnoreCase)
					 || lnf.ActiveSkinName == "DevExpress Style")) {
					using (var pen = new Pen(Utilities.GetHeaderLineColor(lnf))) {
						g.DrawLine(pen, WeekHeaderBounds.Left, WeekHeaderBounds.Top, WeekHeaderBounds.Left, WeekHeaderBounds.Bottom);
						g.DrawLine(pen, WeekHeaderBounds.Left, WeekHeaderBounds.Top, WeekHeaderBounds.Right, WeekHeaderBounds.Top);
						g.DrawLine(pen, WeekHeaderBounds.Right, WeekHeaderBounds.Top, WeekHeaderBounds.Right, WeekHeaderBounds.Bottom);
					}
				}
			}
		}
#if UNUSED
		class SkinCalendarPainter : SkinChromeCalendarPainter {
			public SkinCalendarPainter(HebrewCalendar calendar) : base(calendar) { }

			public override void Render(Graphics g, Rectangle clipRectangle) {
				base.Render(g, clipRectangle);

				if (HoverItem.Date.HasValue)
					RenderDay(g, HoverItem.Date.Value);

				if (Calendar.SelectedDate.HasValue)
					RenderDay(g, Calendar.SelectedDate.Value);
			}

			protected override void DrawDay(Graphics g, HebrewDate date, bool isSelected) {
				SkinElementInfo elemInfo;

				var bgRect = new Rectangle(DayBounds.X, DayBounds.Y, DayBounds.Width + 1, DayBounds.Height + 1);
				//if (isSelected) {
				//    elemInfo = new SkinElementInfo(DockingSkins.GetSkin(Calendar.LookAndFeel)[DockingSkins.SkinDockWindowButton], DayBounds) { Graphics = g };

				//    if (date == Calendar.selectingDate)
				//        elemInfo.ImageIndex = 2;
				//    else
				//        elemInfo.ImageIndex = (date == HoverItem.Date ? 1 : 0);
				//} else if (date == HoverItem.Date) {
				//    elemInfo = new SkinElementInfo(GridSkins.GetSkin(Calendar.LookAndFeel)[GridSkins.SkinCardSelected], bgRect) { Graphics = g };
				//} else {
				//    elemInfo = new SkinElementInfo(GridSkins.GetSkin(Calendar.LookAndFeel)[GridSkins.SkinCard], bgRect) { Graphics = g };
				//}

				//1 for hover
				//var skin = CommonSkins.GetSkin(Calendar.LookAndFeel)[CommonSkins.SkinLayoutItemBackground];

				elemInfo = new SkinElementInfo(NavPaneSkins.GetSkin(Calendar.LookAndFeel)[NavPaneSkins.SkinGroupButtonSelected], bgRect) { Cache = new GraphicsCache(g) };

				if (date == Calendar.selectedDate)
					elemInfo.ImageIndex = (date == HoverItem.Date) ? 1 : 0;
				else if (date == Calendar.selectingDate)
					elemInfo = new SkinElementInfo(NavPaneSkins.GetSkin(Calendar.LookAndFeel)[NavPaneSkins.SkinGroupButton], bgRect) { Graphics = g, ImageIndex = 2 };
				else if (date == HoverItem.Date)
					elemInfo = new SkinElementInfo(NavPaneSkins.GetSkin(Calendar.LookAndFeel)[NavPaneSkins.SkinGroupButton], bgRect) { Graphics = g, ImageIndex = 1 };
				else
					elemInfo = new SkinElementInfo(RibbonSkins.GetSkin(Calendar.LookAndFeel)[RibbonSkins.SkinGalleryPane], bgRect) { Graphics = g, ImageIndex = 1 };

				var matrix = GetColorMatrix(date);
				using (elemInfo.Attributes = matrix == null ? null : matrix.CreateAttributes())
					SkinElementPainter.Default.DrawObject(elemInfo);

				base.DrawDay(g, date, isSelected);
			}

			static QColorMatrix GetColorMatrix(HebrewDate date) {
				if (date.Info.Isשבת) return new QColorMatrix().Scale(1, 1, 5, 1);

				var holiday = date.Info.Holiday;
				if (holiday != null) {
					switch (holiday.Category) {
						case HolidayCategory.דאריתא: return new QColorMatrix().Scale(1, 1, 4, 1);
						case HolidayCategory.חולהמועד: return new QColorMatrix().Scale(1, 1, 2, 1);
						case HolidayCategory.דרבנן: return new QColorMatrix().Scale(1, 3, 1, 1);
						case HolidayCategory.Minor: return new QColorMatrix().Scale(1, 2, 1, 1);
						case HolidayCategory.תענית: return new QColorMatrix().Scale(3, 1, 1, 1);
						case HolidayCategory.Specialשבת:
						case HolidayCategory.Fourפרשיות: return new QColorMatrix().Scale(1, 2, 5, 1);
						default:
							break;
					}
				}
				if (date.Info.Isראשחודש) return new QColorMatrix().Scale(1, 3, 1, 1);
				return null;
			}
		}
#endif
		class WhiteCalendarPainter : SkinChromeCalendarPainter {
			public WhiteCalendarPainter(HebrewCalendar calendar) : base(calendar) { }

			readonly Brush daysAreaBackground = new SolidBrush(Color.FromArgb(128, Color.White));

			public override void Render(Graphics g, Rectangle clipRectangle) {
				base.Render(g, clipRectangle);

				using (var pen = new Pen(Utilities.GetHeaderLineColor(Calendar.LookAndFeel))) {
					int x = GridArea.Left;
					for (int c = 0; c <= 7; c++) {
						g.DrawLine(pen, x, GridArea.Top, x, GridArea.Bottom);
						x += DaySize.Width;	//I want lines on both edges of the grid.
					}
					int y = GridArea.Top;
					for (int c = 0; c < RowCount; c++) {
						y += DaySize.Height;
						g.DrawLine(pen, GridArea.Left, y, GridArea.Right, y);
					}
				}
			}
			protected override void DrawBackground(Graphics g) {
				base.DrawBackground(g);
				g.FillRectangle(daysAreaBackground, GridArea);
			}
			SkinElement dateHighlightElement;
			protected override void OnStyleChanged() {
				base.OnStyleChanged();
				dateHighlightElement = RibbonSkins.GetSkin(Calendar.LookAndFeel)[RibbonSkins.SkinButton];
			}

			class ResultSelector<TInput, TOutput> : Collection<KeyValuePair<Func<TInput, bool>, TOutput>> {
				public void Add(Func<TInput, bool> predicate, TOutput value) {
					if (predicate == null) throw new ArgumentNullException("predicate");
					Add(new KeyValuePair<Func<TInput, bool>, TOutput>(predicate, value));
				}
				public TOutput GetValue(TInput input) {
					return this.FirstOrDefault(kvp => kvp.Key(input)).Value;
				}
			}
			sealed class BrushSelector<TInput> : ResultSelector<TInput, Brush>, IDisposable {
				public void Add(Func<TInput, bool> predicate, int opacity, Color color) {
					Add(predicate, new SolidBrush(Color.FromArgb(opacity, color)));
				}

				public void Dispose() {
					foreach (var kvp in this) {
						kvp.Value.Dispose();
					}
				}
			}

			readonly BrushSelector<HebrewDate> dateColorizers = new BrushSelector<HebrewDate> {
				{ d => d.Info.Isשבת,							48, Color.Blue		},
				{ d => d.Info.Is(HolidayCategory.דאריתא),		32, Color.Purple	},
				{ d => d.Info.Is(HolidayCategory.חולהמועד),	16, Color.Purple	},
				{ d => d.Info.Is(HolidayCategory.תענית),		32, Color.Red		},
				{ d => d.Info.Isראשחודש,						32, Color.Yellow	},
				{ d => d.Info.Is(HolidayCategory.דרבנן),		32, Color.Orange	},
				{ d => d.Info.Is(HolidayCategory.Minor),		24, Color.Orange	},
			};

			protected override void DrawDay(Graphics g, HebrewDate date, bool isSelected) {
				SkinElementInfo elemInfo = null;

				var bgRect = new Rectangle(DayBounds.X, DayBounds.Y, DayBounds.Width + 1, DayBounds.Height + 1);

				if (date == Calendar.selectedDate)
					elemInfo = new SkinElementInfo(dateHighlightElement, bgRect) { Graphics = g, ImageIndex = (date == HoverItem.Date) ? 4 : 3 };
				else if (date == Calendar.selectingDate)
					elemInfo = new SkinElementInfo(dateHighlightElement, bgRect) { Graphics = g, ImageIndex = 2 };
				else if (date == HoverItem.Date)
					elemInfo = new SkinElementInfo(dateHighlightElement, bgRect) { Graphics = g, ImageIndex = 1 };

				if (elemInfo != null)
					SkinElementPainter.Default.DrawObject(elemInfo);
				else {
					var brush = dateColorizers.GetValue(date);
					if (brush != null)
						g.FillRectangle(brush, DayBounds);
				}
				CellRenderer.DrawCell(g, date, isSelected);
			}
			protected override void Dispose(bool disposing) {
				if (disposing) {
					daysAreaBackground.Dispose();
					dateColorizers.Dispose();
				}
				base.Dispose(disposing);
			}
		}
		sealed class DefaultContentRenderer : BaseContentRenderer {
			public DefaultContentRenderer(BaseCalendarPainter painter) : base(painter) { }

			protected override void DrawContent() {
				if (ContentBounds.Width < 45) {
					if (Calendar.Mode == CalendarType.English)
						DrawString(Date.EnglishDate.Day.ToString(Culture));
					else
						DrawString(Date.HebrewDay.ToHebrewString(HebrewNumberFormat.LetterQuoted), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
				} else {
					DrawString(Date.ToString(ContentBounds.Width > 90 || (ContentBounds.Width > 60 && Calendar.Mode == CalendarType.English) ? "M" : "%d"), TextFormatFlags.RightToLeft);

					string englishStr = Date.EnglishDate.ToString("%d", Culture);
					if (ContentBounds.Width > 60 && Calendar.Mode == CalendarType.Hebrew)
						englishStr = Date.EnglishDate.ToString("MMM d", Culture);
					if (ContentBounds.Width > 90) {
						englishStr = Date.EnglishDate.ToString("M", Culture);
						if (TextRenderer.MeasureText(englishStr, Font).Width > ContentBounds.Width / 2)
							englishStr = Date.EnglishDate.ToString("MMM d", Culture);
					}

					DrawString(englishStr, TextFormatFlags.Right);
					var dateHeight = TextRenderer.MeasureText(Graphics, englishStr, Font).Height;

					if (ContentBounds.Width > 40) {
						var caption = new StringBuilder();

						if (Date.Info.Isשבת) {
							var parsha = Date.Parsha;
							if (!String.IsNullOrEmpty(parsha))
								caption.AppendLine(parsha);
						}

						var rh = Date.Info.ראשחודשCaption;
						if (!String.IsNullOrEmpty(rh)) {
							if (TextRenderer.MeasureText(rh, Font).Width >= ContentBounds.Width)
								rh = rh.Replace("ראש חודש", "ר״ח");
							caption.AppendLine(rh);
						}

						var holiday = Date.Info.Holiday;
						if (holiday != null)
							caption.AppendLine(holiday.Name);

						if (caption.Length > 0) {
							var dayBounds = Painter.DayContentBounds;
							var text = caption.ToString().Trim();
							if (TextRenderer.MeasureText(
										Graphics, text, Font,
										new Size(dayBounds.Width, 1024),
										TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
									).Height < dayBounds.Height - dateHeight - 4) {
								TextRenderer.DrawText(
									Graphics, caption.ToString().Trim(), Font,
									new Rectangle(dayBounds.X, dayBounds.Y + dateHeight, dayBounds.Width, dayBounds.Height - dateHeight),
									TextColor,
									TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform
								);
							} else {
								TextRenderer.DrawText(
									Graphics, "...", Font,
									new Rectangle(dayBounds.X, dayBounds.Y + dateHeight, dayBounds.Width, dayBounds.Height - dateHeight),
									TextColor,
									TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform
								);
							}
						}
						//DrawString(caption.ToString().Trim(), TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

						//if ((holiday == null || holiday.Category != HolidayCategory.דאריתא)
						// && !Date.Info.Isשבת
						// && (Date.DayOfWeek == DayOfWeek.Friday || Date.AddDays(1).Info.HolidayCategory == HolidayCategory.דאריתא)) {
						//    var candleLighting = Date.GetZmanim().Sunset - TimeSpan.FromMinutes(18);
						//    DrawString(String.Format(Culture, "Candle lighting: {0}:{1:00}", candleLighting.Hours % 12, candleLighting.Minutes), TextFormatFlags.Bottom);
						//}
					}
				}
			}
		}

		BaseCalendarPainter Painter { get; set; }

		///<summary>Raises the Paint event.</summary>
		protected override void OnPaint(PaintEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnPaint(e);
			Painter.Render(e.Graphics, e.ClipRectangle);
		}
		///<summary>Raises the Resize event.</summary>
		protected override void OnResize(EventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnResize(e);
			Painter.CalendarBounds = ClientRectangle;
		}
		#endregion

		///<summary>Finds the calendar element at a given point.</summary>
		///<param name="location">The point, relative to the control, to look for.</param>
		public CalendarHitTestInfo HitTest(Point location) { return Painter.HitTest(location); }

		#region Mouse
		///<summary>The date that the user is dragging over during selection.</summary>
		///<remarks>This date isn't really selected, but should be drawn as such</remarks>
		HebrewDate? selectingDate;

		readonly Timer navigateTimer;

		///<summary>The direction of the navigation button currently being pressed.</summary>
		int navigateButtonDirection;

		CalendarHitTestInfo hoverItem;

		///<summary>Raises the MouseDown event.</summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseDown(e);
			ResetHover();
			toolTipCreator.HideTip();
			Focus();
			if (e.Button == MouseButtons.Left) {
				var hitInfo = HitTest(e.Location);
				switch (hitInfo.Item) {
					case CalendarItem.PreviousButton:
						OffsetMonths(NavigateButtonDirection = -1);
						break;
					case CalendarItem.NextButton:
						OffsetMonths(NavigateButtonDirection = 1);
						break;
					case CalendarItem.DayCell:

						selectingDate = hitInfo.Date;
						if (hitInfo.Date != null) {
							selectingDate = hitInfo.Date;
							SelectedDate = null;
						}
						break;
				}
			}
			hoverItem = new CalendarHitTestInfo();
			Invalidate();
		}

		///<summary>Raises the MouseMove event.</summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseMove(e);
			var hitInfo = HitTest(e.Location);
			if (selectingDate != null) {
				var newDate = hitInfo.Date ?? hitInfo.NearestDate;

				if (selectingDate == newDate)
					return;

				selectingDate = newDate;
				Invalidate();
				//During a selection, there is no hover logic.
			} else if (e.Button == MouseButtons.None) {
				if (hitInfo != hoverItem) {
					toolTipCreator.HideTip();
					hoverItem = hitInfo;
					Invalidate();
				}
				CheckHover(e.Location);
			}
		}
		///<summary>Raises the MouseUp event.</summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseUp(e);
			NavigateButtonDirection = 0;
			if (selectingDate != null) {
				hoverItem = HitTest(e.Location);

				var date = selectingDate;
				selectingDate = null;

				SetSelection(date, false);
				OnDateClicked(new HebrewDateEventArgs(date.Value));
			}
		}

		///<summary>Gets or sets the direction of the navigation button currently being pressed.</summary>
		int NavigateButtonDirection {
			get { return navigateButtonDirection; }
			set {
				navigateButtonDirection = value;
				navigateTimer.Enabled = value != 0;
			}
		}
		void navigateTimer_Tick(object sender, EventArgs e) {
			//If we lost focus, we'll never get a MouseUp.
			//Stop immediately to avoid scrolling forever.
			//This happens in the Schedulizer UI renderer,
			//which will show a popup from the paint event
			//when loading data.
			if (!Focused)
				NavigateButtonDirection = 0;
			else
				OffsetMonths(NavigateButtonDirection);
		}


		///<summary>Raises the MouseLeave event.</summary>
		protected override void OnMouseLeave(EventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseLeave(e);
			ResetHover();
			if (hoverItem.Item != CalendarItem.None) {
				hoverItem = new CalendarHitTestInfo();
				Invalidate();
			}
		}

		///<summary>Raises the MouseDoubleClick event.</summary>
		protected override void OnMouseDoubleClick(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseDoubleClick(e);
			if (e.Button == MouseButtons.Left) {
				var date = HitTest(e.Location).Date;
				if (date != null) {
					if (!IsSameMonth(date.Value))
						SetView(date.Value, moveSelection: true);
					OnDateDoubleClicked(new HebrewDateEventArgs(date.Value));
				}
			}
		}

		///<summary>Raises the MouseWheel event.</summary>
		protected override void OnMouseWheel(MouseEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnMouseWheel(e);
			OffsetMonths(-Math.Sign(e.Delta));
		}
		#endregion

		#region Keyboard
		///<summary>Determines whether the specified key is a regular input key or a special key that requires preprocessing.</summary>
		protected override bool IsInputKey(Keys keyData) {
			return base.IsInputKey(keyData)
				|| keyData == Keys.Up
				|| keyData == Keys.Right
				|| keyData == Keys.Down
				|| keyData == Keys.Left;
		}

		///<summary>Raises the KeyDown event.</summary>
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void OnKeyDown(KeyEventArgs e) {
			if (e == null) throw new ArgumentNullException("e");
			base.OnKeyDown(e);
			if (!SelectedDate.HasValue) return;
			if (e.Control) {
				switch (e.KeyCode) {
					//Ctrl+Arrows navigate within the
					//current view.  Even if they end
					//up navigating outside the month,
					//the view should not change.
					case Keys.Up: SetSelection(firstDate + (int)SelectedDate.Value.DayOfWeek, false); break;
					case Keys.Down: SetSelection(firstDate + (7 * (RowCount - 1) + (int)SelectedDate.Value.DayOfWeek), false); break;

					case Keys.Left: SetSelection(SelectedDate - (int)SelectedDate.Value.DayOfWeek, false); break;
					case Keys.Right: SetSelection(SelectedDate + 6 - (int)SelectedDate.Value.DayOfWeek, false); break;

					case Keys.PageUp:
					case Keys.PageDown:
						OffsetYears(e.KeyCode == Keys.PageUp ? -1 : 1);
						break;
				}
			} else {
				switch (e.KeyCode) {
					case Keys.Escape: SelectedDate = null; break;

					case Keys.Up: SetSelection(SelectedDate - 7, false); break;
					case Keys.Down: SetSelection(SelectedDate + 7, false); break;

					case Keys.Left: SetSelection(SelectedDate - 1, false); break;
					case Keys.Right: SetSelection(SelectedDate + 1, false); break;

					case Keys.PageUp:
					case Keys.PageDown:
						OffsetMonths(e.KeyCode == Keys.PageUp ? -1 : 1);
						break;
				}
			}
		}


		#endregion

		#region Events
		///<summary>Occurs when a date is double-clicked.</summary>
		public event EventHandler<HebrewDateEventArgs> DateDoubleClicked;
		///<summary>Raises the DateDoubleClicked event.</summary>
		///<param name="e">A HebrewDateEventArgs object that provides the event data.</param>
		internal protected virtual void OnDateDoubleClicked(HebrewDateEventArgs e) {
			if (DateDoubleClicked != null)
				DateDoubleClicked(this, e);
		}
		///<summary>Occurs when a date is clicked.</summary>
		public event EventHandler<HebrewDateEventArgs> DateClicked;
		///<summary>Raises the DateClicked event.</summary>
		///<param name="e">A HebrewDateEventArgs object that provides the event data.</param>
		internal protected virtual void OnDateClicked(HebrewDateEventArgs e) {
			if (DateClicked != null)
				DateClicked(this, e);
		}

		///<summary>Occurs when the selection is changed.</summary>
		public event EventHandler SelectionChanged;
		///<summary>Raises the SelectionChanged event.</summary>
		internal protected virtual void OnSelectionChanged() { OnSelectionChanged(EventArgs.Empty); }
		///<summary>Raises the SelectionChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnSelectionChanged(EventArgs e) {
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}
		#endregion

		#region Tooltips
		readonly Timer hoverTimer;
		readonly Size hoverLimit = SystemInformation.MouseHoverSize;	//Get this once at the same time as HoverTime in the ctor
		static readonly Point InvalidPoint = new Point(int.MinValue, int.MaxValue);
		Point hoverStart = InvalidPoint;
		void CheckHover(Point mouse) {
			if (hoverStart == InvalidPoint
			 || Math.Abs(mouse.X - hoverStart.X) > hoverLimit.Width / 2
			 || Math.Abs(mouse.Y - hoverStart.Y) > hoverLimit.Height / 2) {
				hoverStart = mouse;
				hoverTimer.Stop();
				hoverTimer.Start();
			}
		}
		void ResetHover() {
			hoverTimer.Stop();
			hoverStart = InvalidPoint;
		}

		void hoverTimer_Tick(object sender, EventArgs e) {
			var location = hoverStart;
			ResetHover();
			if (location != InvalidPoint)
				OnHover(location);
		}
		interface IToolTipCreator : IDisposable {
			void ShowTip(Point location, string text);
			void HideTip();
		}
		readonly IToolTipCreator toolTipCreator;

		sealed class DXToolTipCreator : IToolTipCreator {
			public DXToolTipCreator(HebrewCalendar calendar) {
				this.calendar = calendar;
				controller.BeforeShow += controller_BeforeShow;
			}

			void controller_BeforeShow(object sender, ToolTipControllerShowEventArgs e) {
				e.SelectedControl = calendar;	//Force tooltip to inherit calendar's LookAndFeel
			}
			readonly HebrewCalendar calendar;

			readonly ToolTipController controller = new ToolTipController { ToolTipType = ToolTipType.SuperTip };

			public void ShowTip(Point location, string text) {
				controller.ShowHint(text, calendar.PointToScreen(location));
			}

			public void HideTip() {
				controller.HideHint();
			}

			public void Dispose() { controller.Dispose(); }
		}
		///<summary>Called when the mouse hovers over a point in the calendar.</summary>
		protected virtual void OnHover(Point location) {
			if (MouseButtons != MouseButtons.None || toolTipCreator == null) return;

			var hitInfo = HitTest(PointToClient(MousePosition));
			switch (hitInfo.Item) {
				case CalendarItem.None:
				case CalendarItem.WeekdayHeader:
					toolTipCreator.HideTip();
					break;
				case CalendarItem.PreviousButton:
					toolTipCreator.ShowTip(new Point(Painter.PreviousButton.Left, Painter.PreviousButton.Bottom), "Click to navigate to the previous month");
					break;
				case CalendarItem.NextButton:
					toolTipCreator.ShowTip(new Point(Painter.NextButton.Left, Painter.NextButton.Bottom), "Click to navigate to the next month");
					break;
				case CalendarItem.DayCell:
					HebrewDate date = hitInfo.Date.Value;

					var args = new CalendarToolTipEventArgs(date, Painter.GetToolTipLocation(date));
					args.ToolTipText.AppendLine(args.Date.EnglishDate.ToLongDateString());
					args.ToolTipText.AppendLine(args.Date.ToString());

					if (date.Info.Isשבת) {
						var parsha = date.Parsha;
						if (parsha != null)
							args.ToolTipText.AppendLine().Append("פרשת ").AppendLine(parsha);
					}

					var holiday = date.Info.Holiday;
					if (holiday != null)
						args.ToolTipText.AppendLine().AppendLine(holiday.Name);	//Disappoint Aryeh and don't say ראש חודש תשרי
					else if (date.Info.Isראשחודש)
						args.ToolTipText.AppendLine().AppendLine(date.Info.ראשחודשCaption);

					OnDateToolTip(args);
					if (args.DisplayTip)
						toolTipCreator.ShowTip(args.Location, args.ToolTipText.ToString());
					break;
				default:
					break;
			}
		}

		///<summary>Occurs when a tooltip for a date is about to be shown.</summary>
		public event EventHandler<CalendarToolTipEventArgs> DateToolTip;
		///<summary>Raises the DateToolTip event.</summary>
		///<param name="e">A CalendarToolTipEventArgs object that provides the event data.</param>
		internal protected virtual void OnDateToolTip(CalendarToolTipEventArgs e) {
			if (DateToolTip != null)
				DateToolTip(this, e);
		}
		#endregion

		///<summary>Releases resources used by the control.</summary>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (Painter != null) Painter.Dispose();
				if (toolTipCreator != null) toolTipCreator.Dispose();
				if (navigateTimer != null) navigateTimer.Dispose();
				if (TodayFont != null) TodayFont.Dispose();
			}
			base.Dispose(disposing);
		}
	}
	///<summary>A calendar painter.  This interface is implemented internally by HebrewCalendar.</summary>
	public interface ICalendarPainter {
		///<summary>Gets the calendar that is being painted.</summary>
		HebrewCalendar Calendar { get; }
		///<summary>Gets a rectangle with the bounds of the cell content.</summary>
		Rectangle DayContentBounds { get; }

	}
	///<summary>A base class that draws text in calendar cells.</summary>
	public abstract class BaseContentRenderer {
		///<summary>Creates a content renderer from a calendar painter.</summary>
		protected BaseContentRenderer(ICalendarPainter painter) { Painter = painter; }

		///<summary>Gets the painter that this content renderer draws for.</summary>
		public ICalendarPainter Painter { get; private set; }
		///<summary>Gets the calendar that is being painted.</summary>
		public HebrewCalendar Calendar { get { return Painter.Calendar; } }
		///<summary>Gets a rectangle with the bounds of the cell content.</summary>
		public Rectangle ContentBounds { get { return Painter.DayContentBounds; } }

		///<summary>Gets the font to use for the current date.</summary>
		public Font Font { get { return Date == DateTime.Today ? Calendar.TodayFont : Calendar.Font; } }

		///<summary>Called when the calendar begins painting itself.</summary>
		protected internal virtual void OnBeginPaint() { }

		///<summary>Draws a cell.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
		public void DrawCell(Graphics g, HebrewDate date, bool isSelected) {
			Date = date;
			Graphics = g;

			if (isSelected)
				TextColor = Color.Black;
			else if (Calendar.IsSameMonth(Date))
				TextColor = Color.Navy;
			else
				TextColor = Color.DarkGray;
			DrawContent();
		}
		///<summary>Draws the content of the current cell.</summary>
		protected abstract void DrawContent();

		///<summary>Gets the date that is currently being drawn.</summary>
		protected HebrewDate Date { get; private set; }
		///<summary>Gets the color used to draw text for the date that is currently being drawn.</summary>
		protected Color TextColor { get; private set; }
		///<summary>Gets the Graphics object for the date that is currently being drawn.</summary>
		protected Graphics Graphics { get; private set; }

		///<summary>Draws a string centered on the cell.</summary>
		protected void DrawString(string text) { DrawString(text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
		///<summary>Draws a string on the cell.</summary>
		protected void DrawString(string text, Point location) { DrawString(text, location, TextFormatFlags.Default); }

		///<summary>Draws a string on the cell.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "API Term")]
		protected void DrawString(string text, Point location, TextFormatFlags flags) {
			TextRenderer.DrawText(Graphics, text, Font, location, TextColor, flags | TextFormatFlags.PreserveGraphicsTranslateTransform);
		}
		///<summary>Draws a string on the cell.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "API Term")]
		protected void DrawString(string text, TextFormatFlags flags) {
			TextRenderer.DrawText(Graphics, text, Font, Painter.DayContentBounds, TextColor, flags | TextFormatFlags.PreserveGraphicsTranslateTransform);
		}
	}


	///<summary>Provides data for the CellToolTip event.</summary>
	public class CalendarToolTipEventArgs : HebrewDateEventArgs {
		/// <summary>Creates a CalendarToolTipEventArgs from a Hebrew date.</summary>
		public CalendarToolTipEventArgs(HebrewDate date, Point location) : base(date) { Location = location; DisplayTip = true; ToolTipText = new StringBuilder(); }

		///<summary>Gets the text that will be displayed in the tooltip.  To customize the tooltip, append to this StringBuilder.</summary>
		public StringBuilder ToolTipText { get; private set; }

		///<summary>Gets or sets whether to show the tooltip.</summary>
		public bool DisplayTip { get; set; }

		///<summary>Gets the location that the tooltip will be shown at.</summary>
		public Point Location { get; private set; }
	}

	///<summary>A type of calendar.</summary>
	public enum CalendarType {
		///<summary>An English calendar.</summary>
		English,
		///<summary>A Hebrew calendar.</summary>
		Hebrew
	}

	///<summary>A location on a HebrewCalendar control.</summary>
	public struct CalendarHitTestInfo : IEquatable<CalendarHitTestInfo> {
		///<summary>Creates a CalendarHitTestInfo for the background or the navigation buttons.</summary>
		public CalendarHitTestInfo(CalendarItem item, HebrewDate nearestDate) : this() { Item = item; NearestDate = nearestDate; }
		///<summary>Creates a CalendarHitTestInfo for a weekday header.</summary>
		public CalendarHitTestInfo(DayOfWeek weekHeader, HebrewDate nearestDate) : this(CalendarItem.WeekdayHeader, nearestDate) { DayOfWeek = weekHeader; }
		///<summary>Creates a CalendarHitTestInfo for a date cell.</summary>
		public CalendarHitTestInfo(HebrewDate date) : this(CalendarItem.DayCell, date) { Date = date; }

		///<summary>Gets the item at the location.</summary>
		public CalendarItem Item { get; private set; }

		///<summary>Gets the date closest to the point.</summary>
		///<remarks>This is used when during selection.</remarks>
		public HebrewDate NearestDate { get; private set; }

		///<summary>Gets the week day of the header at the location, or null if the location isn't a weekday header.</summary>
		public DayOfWeek? DayOfWeek { get; private set; }
		///<summary>Gets the date of the cell at the location, or null if the location isn't a date cell.</summary>
		public HebrewDate? Date { get; private set; }

		///<summary>Checks whether this CalendarHitTestInfo is equal to another object.</summary>
		public override bool Equals(object obj) { return obj is CalendarHitTestInfo && Equals((CalendarHitTestInfo)obj); }
		///<summary>Checks whether this CalendarHitTestInfo is equal to another CalendarHitTestInfo value.</summary>
		public bool Equals(CalendarHitTestInfo other) { return other.Item == Item && other.NearestDate == NearestDate && other.Date == Date && other.DayOfWeek == DayOfWeek; }

		///<summary>Returns the hash code for this instance.</summary>
		public override int GetHashCode() { return Item.GetHashCode() ^ NearestDate.GetHashCode() ^ DayOfWeek.GetHashCode() ^ Date.GetHashCode(); }

		///<summary>Checks whether two CalendarHitTestInfo values are equal.</summary>
		public static bool operator ==(CalendarHitTestInfo first, CalendarHitTestInfo second) { return first.Equals(second); }
		///<summary>Checks whether two CalendarHitTestInfo values are unequal.</summary>
		public static bool operator !=(CalendarHitTestInfo first, CalendarHitTestInfo second) { return !first.Equals(second); }
	}
	///<summary>The types of items in the HebrewCalendar control.</summary>
	public enum CalendarItem {
		///<summary>The background of the calendar.</summary>
		None,
		///<summary>The button that navigates to the previous month.</summary>
		PreviousButton,
		///<summary>The button that navigates to the next month.</summary>
		NextButton,
		///<summary>A header for the day of the week.</summary>
		WeekdayHeader,
		///<summary>A cell containing a date.</summary>
		DayCell
	}
}