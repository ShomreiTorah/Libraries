using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Common.Calendar {
	///<summary>Represents a date on the Hebrew calendar.</summary>
	public partial struct HebrewDate : IComparable<HebrewDate>, IEquatable<HebrewDate> {
		static readonly HebrewCalendar calendar = new HebrewCalendar();
		static readonly CultureInfo culture = new Func<CultureInfo>(() => { var c = new CultureInfo("he-IL"); c.DateTimeFormat.Calendar = calendar; return c; })();
		DateTime date;

		int hebrewYear, hebrewDay;
		HebrewMonth hebrewMonth;

		///<summary>Creates a Hebrew date from an English one.</summary>
		///<param name="englishDate">The english date to convert.</param>
		public HebrewDate(DateTime englishDate) {
			date = englishDate.Date;

			hebrewYear = calendar.GetYear(date);
			hebrewMonth = CalendarExtensions.ToHebrewMonth(hebrewYear, calendar.GetMonth(date));
			hebrewDay = calendar.GetDayOfMonth(englishDate);
		}
		///<summary>Creates a Hebrew date.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<param name="month">The Hebrew month.</param>
		///<param name="day">The Hebrew day of the month.</param>
		public HebrewDate(int hebrewYear, HebrewMonth month, int day) {
			if (month == 0) throw new ArgumentOutOfRangeException("month", "None is not a valid month.");
			this.hebrewYear = hebrewYear;

			if (!calendar.IsLeapYear(hebrewYear) && month == HebrewMonth.אדר1)
				month = HebrewMonth.אדר2;

			hebrewMonth = month;
			hebrewDay = day;
			date = calendar.ToDateTime(hebrewYear, month.Index(hebrewYear), day, 0, 0, 0, 0);
		}
		#region Statics
		///<summary>Checks whether a Hebrew year is a leap year.</summary>
		///<param name="hebrewYear">The Hebrew year to check.</param>
		///<returns>True if hebrewYear is a leap year.</returns>
		public static bool IsHebrewLeapYear(int hebrewYear) { return calendar.IsLeapYear(hebrewYear); }
		//This isn't named IsLeapYear because it conflicts with the 
		//instance property and I can't figure out where else to put it.
		#endregion
		#region Info
		///<summary>Gets the current Hebrew date.</summary>
		///<remarks>Ignores sunset.</remarks>
		public static HebrewDate Today { get { return new HebrewDate(DateTime.Today); } }

		///<summary>Gets the English date.</summary>
		public DateTime EnglishDate { get { return date; } }

		///<summary>Gets the Hebrew year.</summary>
		public int HebrewYear { get { return hebrewYear; } }
		///<summary>Gets the Hebrew year.</summary>
		public HebrewMonth HebrewMonth { get { return hebrewMonth; } }
		///<summary>Gets the Hebrew year.</summary>
		public int HebrewDay { get { return hebrewDay; } }

		///<summary>Gets the day of the week.</summary>
		public DayOfWeek DayOfWeek { get { return EnglishDate.DayOfWeek; } }

		///<summary>Gets the number of days in the Hebrew month.</summary>
		public int MonthLength { get { return HebrewMonth.Length(HebrewYear); } }
		///<summary>Gets the day of the Hebrew year.</summary>
		public int DayOfHebrewYear { get { return calendar.GetDayOfYear(EnglishDate); } }

		///<summary>Gets the name of the Hebrew month.</summary>
		public string HebrewMonthName { get { return HebrewMonth.ToString(HebrewYear); } }
		///<summary>Gets whether the Hebrew year is a leap year.</summary>
		public bool IsLeapYear { get { return calendar.IsLeapYear(HebrewYear); } }

		///<summary>Gets the type of year.</summary>
		public HebrewYearType YearType { get { return GetYearType(hebrewYear); } }

		///<summary>Gets information about this Hebrew date.</summary>
		public HebrewDayInfo Info { get { return new HebrewDayInfo(this); } }

		///<summary>Finds the type of a year.</summary>
		///<param name="hebrewYear">The year to check.</param>
		///<returns>The type of the year.</returns>
		public static HebrewYearType GetYearType(int hebrewYear) {
			var length = calendar.GetDaysInYear(hebrewYear);
			if (length > 380) length -= 30;
			return (HebrewYearType)length;
		}

		///<summary>Gets the Zmanim for this date.</summary>
		///<returns>A ZmanimInfo instance from the default ZmanimProvider.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Potentially expensive call")]
		public Zmanim.ZmanimInfo GetZmanim() { return Zmanim.ZmanimProvider.Default.GetZmanim(this); }
		#endregion
		#region Mutators
		///<summary>Adds a number of days to this date.</summary>
		///<param name="days">The number of days to add.  This parameter can be positive or negative.</param>
		public HebrewDate AddDays(int days) { return this + days; }

		///<summary>Adds a number of Hebrew months to this date.</summary>
		///<param name="hebrewMonths">The number of Hebrew months to add.  This parameter can be positive or negative.</param>
		public HebrewDate AddMonths(int hebrewMonths) { return calendar.AddMonths(EnglishDate, hebrewMonths); }

		///<summary>Adds a number of Hebrew years to this date.</summary>
		///<param name="hebrewYears">The number of Hebrew years to add.  This parameter can be positive or negative.</param>
		public HebrewDate AddYears(int hebrewYears) { return calendar.AddYears(EnglishDate, hebrewYears); }
		#endregion
		///<summary>Compares this Hebrew date to another Hebrew date.</summary>
		///<param name="other">The date to compare to.</param>
		///<returns>A signed number indicating the relative values of this instance and the value parameter. </returns>
		public int CompareTo(HebrewDate other) { return EnglishDate.CompareTo(other.EnglishDate); }
		///<summary>Checks whether this Hebrew date is equal to another Hebrew date.</summary>
		///<param name="other">The Hebrew date to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(HebrewDate other) { return EnglishDate.Equals(other.EnglishDate); }

		#region Operators & overloads
		///<summary>Checks whether this Hebrew date is equal to an object.</summary>
		///<param name="obj">The Hebrew date to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public override bool Equals(object obj) { return obj is HebrewDate && Equals((HebrewDate)obj); }
		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return EnglishDate.GetHashCode(); }
		///<summary>Converts this Hebrew date to its string representation.</summary>
		///<returns>A Hebrew date string, such as שבת ג' ניסן תשס"ט.</returns>
		public override string ToString() { return ToString("D"); }
		///<summary>Converts this Hebrew date to its string representation using a custom format string.</summary>
		///<param name="format">A DateTime format string.</param>
		///<returns>A Hebrew date string.</returns>
		public string ToString(string format) { return EnglishDate.ToString(format, culture); }


		///<summary>Converts a Hebrew date to an English date.</summary>
		///<param name="hebrewDate">The Hebrew date to convert.</param>
		///<returns>The underlying English date.</returns>
		public static implicit operator DateTime(HebrewDate hebrewDate) { return hebrewDate.EnglishDate; }
		///<summary>Converts an English date to a Hebrew date.</summary>
		///<param name="englishDate">The English date to convert.</param>
		///<returns>The corresponding Hebrew date.</returns>
		public static implicit operator HebrewDate(DateTime englishDate) { return new HebrewDate(englishDate); }

		///<summary>Adds a number of days to a Hebrew date.</summary>
		///<param name="date">The Hebrew date to add to.</param>
		///<param name="days">The number of days to add.  This parameter can be positive or negative.</param>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "AddDays")]
		public static HebrewDate operator +(HebrewDate date, int days) { return new HebrewDate(date.EnglishDate.AddDays(days)); }
		///<summary>Subtracts a number of days to a Hebrew date.</summary>
		///<param name="date">The Hebrew date to subtract from.</param>
		///<param name="days">The number of days to subtract.  This parameter can be positive or negative.</param>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "AddDays")]
		public static HebrewDate operator -(HebrewDate date, int days) { return date + (-days); }

		///<summary>Adds one day to a Hebrew date.</summary>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Not this one")]
		public static HebrewDate operator ++(HebrewDate date) { return date + 1; }
		///<summary>Subtracts one day from a Hebrew date.</summary>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Not this one")]
		public static HebrewDate operator --(HebrewDate date) { return date - 1; }

		///<summary>Adds a number of days to a Hebrew date.</summary>
		///<param name="date">The Hebrew date to add to.</param>
		///<param name="span">The number of days to add.  This parameter can be positive or negative.</param>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "AddDays")]
		public static HebrewDate operator +(HebrewDate date, TimeSpan span) { return date + span.Days; }
		///<summary>Subtracts a number of days to a Hebrew date.</summary>
		///<param name="date">The Hebrew date to subtract from.</param>
		///<param name="span">The number of days to subtract.  This parameter can be positive or negative.</param>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "AddDays")]
		public static HebrewDate operator -(HebrewDate date, TimeSpan span) { return date - span.Days; }

		//I need overloads for all 3 combinations.
		//If I don't have any, I can't use two HebrewDates
		//If I only have the first, I can't use one HebrewDate
		//and one DateTime becuase it can't figure out which
		//way to cast.  I could also solve that by removing 
		//one of the implicit casts, but I don't want to.
		#region Comparison
		///<summary>Subtracts two dates.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>The TimeSpan with the interval between the dates.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Clutter")]
		public static TimeSpan operator -(HebrewDate first, HebrewDate second) { return first.EnglishDate - second.EnglishDate; }

		///<summary>Checks whether two dates are equal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are equal.</returns>
		public static bool operator ==(HebrewDate first, HebrewDate second) { return first.EnglishDate == second.EnglishDate; }
		///<summary>Checks whether two dates are unequal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are unequal.</returns>
		public static bool operator !=(HebrewDate first, HebrewDate second) { return !(first == second); }

		///<summary>Checks whether two dates are equal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are equal.</returns>
		public static bool operator ==(HebrewDate first, DateTime second) { return first.EnglishDate == second; }
		///<summary>Checks whether two dates are unequal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are unequal.</returns>
		public static bool operator !=(HebrewDate first, DateTime second) { return !(first.EnglishDate == second); }

		///<summary>Checks whether two dates are equal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are equal.</returns>
		public static bool operator ==(DateTime first, HebrewDate second) { return first == second.EnglishDate; }
		///<summary>Checks whether two dates are unequal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two dates are unequal.</returns>
		public static bool operator !=(DateTime first, HebrewDate second) { return !(first == second.EnglishDate); }


		///<summary>Checks whether one date is after another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after the second date.</returns>
		public static bool operator >(HebrewDate first, HebrewDate second) { return first.EnglishDate > second.EnglishDate; }
		///<summary>Checks whether one date is before another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before the second date.</returns>
		public static bool operator <(HebrewDate first, HebrewDate second) { return first.EnglishDate < second.EnglishDate; }
		///<summary>Checks whether one date is after or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after or equal to the second date.</returns>
		public static bool operator >=(HebrewDate first, HebrewDate second) { return first.EnglishDate >= second.EnglishDate; }
		///<summary>Checks whether one date is before or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before or equal to the second date.</returns>
		public static bool operator <=(HebrewDate first, HebrewDate second) { return first.EnglishDate <= second.EnglishDate; }

		///<summary>Checks whether one date is after another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after the second date.</returns>
		public static bool operator >(HebrewDate first, DateTime second) { return first.EnglishDate > second; }
		///<summary>Checks whether one date is before another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before the second date.</returns>
		public static bool operator <(HebrewDate first, DateTime second) { return first.EnglishDate < second; }
		///<summary>Checks whether one date is after or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after or equal to the second date.</returns>
		public static bool operator >=(HebrewDate first, DateTime second) { return first.EnglishDate >= second; }
		///<summary>Checks whether one date is before or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before or equal to the second date.</returns>
		public static bool operator <=(HebrewDate first, DateTime second) { return first.EnglishDate <= second; }

		///<summary>Checks whether one date is after another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after the second date.</returns>
		public static bool operator >(DateTime first, HebrewDate second) { return first > second.EnglishDate; }
		///<summary>Checks whether one date is before another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before the second date.</returns>
		public static bool operator <(DateTime first, HebrewDate second) { return first < second.EnglishDate; }
		///<summary>Checks whether one date is after or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is after or equal to the second date.</returns>
		public static bool operator >=(DateTime first, HebrewDate second) { return first >= second.EnglishDate; }
		///<summary>Checks whether one date is before or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first date is before or equal to the second date.</returns>
		public static bool operator <=(DateTime first, HebrewDate second) { return first <= second.EnglishDate; }
		#endregion
		#region HebrewEvent operators

		///<summary>Returns the number of days between a Hebrew date and a day of that year.</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>The number of days between the date and the day of that year.  If the date is before the day of year, the result will be negative.</returns>
		public static int operator -(HebrewDate date, IHebrewEvent hebrewEvent) { return hebrewEvent == null ? 0 : (date - hebrewEvent.GetDate(date.HebrewYear)).Days; }

		///<summary>Checks whether a Hebrew date is a specific day of the year.</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the two values have the same month and day.</returns>
		public static bool operator ==(HebrewDate date, IHebrewEvent hebrewEvent) { return hebrewEvent != null && hebrewEvent.Is(date); }
		///<summary>Checks whether a Hebrew date isn't a specific day of the year.</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the two values have a different month or day.</returns>
		public static bool operator !=(HebrewDate date, IHebrewEvent hebrewEvent) { return !(date == hebrewEvent); }

		///<summary>Checks whether a Hebrew date occurs before a specific day of its year</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the date occurs before the day of year.</returns>
		public static bool operator <(HebrewDate date, IHebrewEvent hebrewEvent) { return hebrewEvent != null && date < hebrewEvent.GetDate(date.HebrewYear); }
		///<summary>Checks whether a Hebrew date occurs before or on a specific day of its year</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the date occurs before or on the day of year.</returns>
		public static bool operator <=(HebrewDate date, IHebrewEvent hebrewEvent) { return hebrewEvent != null && date <= hebrewEvent.GetDate(date.HebrewYear); }

		///<summary>Checks whether a Hebrew date occurs after a specific day of its year</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the date occurs after the day of year.</returns>
		public static bool operator >(HebrewDate date, IHebrewEvent hebrewEvent) { return !(date <= hebrewEvent); }
		///<summary>Checks whether a Hebrew date occurs after or on a specific day of its year</summary>
		///<param name="date">The Hebrew date.</param>
		///<param name="hebrewEvent">The day of the year.</param>
		///<returns>True if the date occurs after or on the day of year.</returns>
		public static bool operator >=(HebrewDate date, IHebrewEvent hebrewEvent) { return !(date < hebrewEvent); }
		#endregion
		#endregion
	}

	///<summary>Represents a Hebrew month.</summary>
	public enum HebrewMonth {
		///<summary>No month.</summary>
		None = 0,
		///<summary>The month of תשרי.</summary>
		תשרי = 1,
		///<summary>The month of חשון.</summary>
		חשון,
		///<summary>The month of כסלו.</summary>
		כסלו,
		///<summary>The month of טבת.</summary>
		טבת,
		///<summary>The month of שבט.</summary>
		שבט,
		///<summary>The month of אדר א.</summary>
		אדר1 = 6,
		///<summary>The month of אדר ב, or אדר in a non-leap year.</summary>
		אדר2 = 7,
		///<summary>The month of ניסן.</summary>
		ניסן,
		///<summary>The month of אייר.</summary>
		אייר,
		///<summary>The month of סיון.</summary>
		סיון,
		///<summary>The month of תמוז.</summary>
		תמוז,
		///<summary>The month of אב.</summary>
		אב,
		///<summary>The month of אלול.</summary>
		אלול
	}
	///<summary>Represents the type of a Hebrew year.</summary>
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Unnecessary")]
	public enum HebrewYearType {
		///<summary>The year is חסר.  It has 353 or 383 days.  Both חשון and כסלו have 29 days.</summary>
		חסר = 353,
		///<summary>The year is regular.  It has 354 or 384 days.  חשון has 29 days; כסלו has 30 days.</summary>
		כסדרן,
		///<summary>The year is מלא.  It has 355 or 385 days.  Both חשון and כסלו have 30 days.</summary>
		מלא
	}

	///<summary>Provides data for an event concerning a Hebrew date.</summary>
	public class HebrewDateEventArgs : EventArgs {
		///<summary>Creates a HebrewDateEventArgs from a Hebrew date.</summary>
		public HebrewDateEventArgs(HebrewDate date) { Date = date; }

		///<summary>Gets the date.</summary>
		public HebrewDate Date { get; private set; }
	}
}
