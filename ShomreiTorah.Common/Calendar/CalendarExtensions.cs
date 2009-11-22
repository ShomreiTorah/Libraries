using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Common.Calendar {
	///<summary>Contains calendar-related extension methods.</summary>
	public static class CalendarExtensions {

		///<summary>Checks whether this Holiday instance is within a HolidayGroup.</summary>
		public static bool Is(this Holiday holiday, HolidayGroup group) {
			if (group == null)
				return holiday == null;
			return holiday != null && group != null && group.Days.Contains(holiday);
		}
		///<summary>Checks whether this Holiday instance is equal to another Holiday.</summary>
		public static bool Is(this Holiday holiday, Holiday other) { return holiday == other; }
		///<summary>Checks whether this Holiday instance is within a holiday category.</summary>
		public static bool Is(this Holiday holiday, HolidayCategory category) {
			if (category == HolidayCategory.None)
				return holiday == null;
			return holiday != null && holiday.Category == category;
		}


		static readonly HebrewCalendar calendar = new HebrewCalendar();
		///<summary>Gets the first week day following a date.</summary>
		///<param name="date">The date.</param>
		///<param name="dayOfWeek">The day of week to return.</param>
		///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
		public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek) { return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek); }
		///<summary>Gets the last week day preceding a date.</summary>
		///<param name="date">The date.</param>
		///<param name="dayOfWeek">The day of week to return.</param>
		///<returns>The last dayOfWeek day preceding date, or date if it is on dayOfWeek.</returns>
		public static DateTime Last(this DateTime date, DayOfWeek dayOfWeek) { return date.AddDays((dayOfWeek > date.DayOfWeek ? -7 : 0) + dayOfWeek - date.DayOfWeek); }


		///<summary>Gets the first week day following a Hebrew date.</summary>
		///<param name="date">The date.</param>
		///<param name="dayOfWeek">The day of week to return.</param>
		///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
		public static HebrewDate Next(this HebrewDate date, DayOfWeek dayOfWeek) { return date.EnglishDate.Next(dayOfWeek); }	//Implicit cast
		///<summary>Gets the last week day preceding a Hebrew date.</summary>
		///<param name="date">The date.</param>
		///<param name="dayOfWeek">The day of week to return.</param>
		///<returns>The last dayOfWeek day preceding date, or date if it is on dayOfWeek.</returns>
		public static HebrewDate Last(this HebrewDate date, DayOfWeek dayOfWeek) { return date.EnglishDate.Last(dayOfWeek); }

		///<summary>Converts a .Net Hebrew year / month to a HebrewMonth enum.</summary>
		///<param name="hebrewYear">The Hebrew year containing the month.  This is checked for leap years.</param>
		///<param name="month">The month number within the year.</param>
		///<returns>A HebrewMonth enum.  אדר on non-leap years is converted to HebrewMonth.אדר2.</returns>
		public static HebrewMonth ToHebrewMonth(int hebrewYear, int month) {
			if (calendar.IsLeapYear(hebrewYear) || month < (int)HebrewMonth.אדר1)
				return (HebrewMonth)month;
			return (HebrewMonth)(month + 1);
		}
		///<summary>Gets the index of a month in year.</summary>
		///<param name="month">The name of the month.</param>
		///<param name="hebrewYear">The year to check.</param>
		///<returns>The month number within the year.  (Can be given to .Net HebrewCalendars)</returns>
		public static int Index(this HebrewMonth month, int hebrewYear) { return Index(hebrewYear, (int)month); }
		///<summary>Gets the index of a month in year.</summary>
		///<param name="month">The integer value of the HebrewMonth enum.</param>
		///<param name="hebrewYear">The year to check.</param>
		///<returns>The month number within the year.  (Can be given to .Net HebrewCalendars)</returns>
		public static int Index(int hebrewYear, int month) {
			if (month == 0) throw new ArgumentOutOfRangeException("month", "None is not a valid month.");

			if (calendar.IsLeapYear(hebrewYear))
				return month;
			else if (month == (int)HebrewMonth.אדר1)
				return 6;
			else if (month > 6)
				return month - 1;
			else
				return month;
		}
		///<summary>Gets the length of a Hebrew month.</summary>
		///<param name="month">The month.</param>
		///<param name="hebrewYear">The year.</param>
		///<returns>The number of days in the month.</returns>
		public static int Length(this HebrewMonth month, int hebrewYear) {
			if (month == 0) throw new ArgumentOutOfRangeException("month", "None is not a valid month.");
			return calendar.GetDaysInMonth(hebrewYear, month.Index(hebrewYear));
		}
		///<summary>Gets the name of the given month in the given Hebrew year.</summary>
		///<param name="month">The Hebrew month.</param>
		///<param name="hebrewYear">The Hebrew year (for leap years).</param>
		///<returns>The name of the month.</returns>
		public static string ToString(this HebrewMonth month, int hebrewYear) {
			switch (month) {
				case 0: throw new ArgumentOutOfRangeException("month", "None is not a valid month.");

				case HebrewMonth.אדר1: return calendar.IsLeapYear(hebrewYear) ? "אדר א" : "אדר";
				case HebrewMonth.אדר2: return calendar.IsLeapYear(hebrewYear) ? "אדר ב" : "אדר";
				default: return month.ToString();
			}
		}

		///<summary>Gets the human-readable name of a Zman enum value.</summary>
		public static string ToZmanName(this Zmanim.Zman zman) { return Zmanim.ZmanimInfo.ToZmanName(zman); }
	}
}
