using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Common.Calendar {
	///<summary>Describes a Hebrew date.</summary>
	public sealed partial class HebrewDayInfo {
		internal HebrewDayInfo(HebrewDate date) { Date = date; }

		///<summary>Gets the Hebrew date described by this instance.</summary>
		public HebrewDate Date { get; private set; }

		///<summary>Gets whether this date is ראש חודש, including ראש השנה.</summary>
		public bool Isראשחודש { get { return Date.HebrewDay == 1 || Date.HebrewDay == 30; } }
		///<summary>Gets the name of the month that this date is ראש חודש for, or null if this date is not ראש חודש.</summary>
		public string ראשחודשMonth { get { return Isראשחודש ? (Date + 15).HebrewMonthName : null; } }

		///<summary>Gets the full name of the day of ראש חודש of this date, or null if this date is not ראש חודש or is ראש השנה.</summary>
		public string ראשחודשCaption {
			get {
				var month = ראשחודשMonth;
				if (string.IsNullOrEmpty(month) || month == "תשרי") return null;//Disappoint Aryeh and don't say ראש חודש תשרי

				if (Date.HebrewDay == 30)
					return "א' ראש חודש " + month;
				else if ((Date - 1).HebrewDay == 30)
					return "ב' ראש חודש " + month;
				else
					return "ראש חודש " + month;
			}
		}

		///<summary>Gets whether this date is שבת.</summary>
		public bool Isשבת { get { return Date.DayOfWeek == DayOfWeek.Saturday; } }
		///<summary>Gets whether this date is either a שבת or a יום טוב דאריתא.</summary>
		public bool Isשבתיוםטוב { get { return Isשבת || HolidayCategory == HolidayCategory.דאריתא; } }

		///<summary>Gets the יום טוב on this day, or null for an ordinary day.</summary>
		public Holiday Holiday { get { return Holiday.All.FirstOrDefault(h => h.Date.Is(Date)); } }

		///<summary>Gets the category of the יום טוב (if any) on this day.</summary>
		public HolidayCategory HolidayCategory { get { var h = Holiday; return h == null ? HolidayCategory.None : h.Category; } }

		///<summary>Checks whether is date is part of a multi-day יום טוב.</summary>
		public bool Is(HolidayGroup group) { return group != null && group.Days.Any(h => h.Date.Is(Date)); }
		///<summary>Checks whether is date is a יום טוב.</summary>
		public bool Is(Holiday holiday) { return holiday != null && holiday.Date.Is(Date); }
		///<summary>Checks whether is date is a category of יום טוב.</summary>
		public bool Is(HolidayCategory category) { return Holiday.All.Any(h => h.Category == category && h.Date.Is(Date)); ; }
	}
}
