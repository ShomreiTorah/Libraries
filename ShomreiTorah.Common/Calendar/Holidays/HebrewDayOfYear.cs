using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>Represents a day of any Hebrew year.</summary>
	public struct HebrewDayOfYear : IHebrewEvent, IEquatable<HebrewDayOfYear>, IComparable<HebrewDayOfYear> {
		///<summary>Creates a HebrewDayOfYear.</summary>
		///<param name="month">The Hebrew month.</param>
		///<param name="day">The Hebrew day.</param>
		public HebrewDayOfYear(HebrewMonth month, int day)
			: this() {
			if (month == HebrewMonth.None) throw new ArgumentOutOfRangeException("month", "None is not a valid month");
			if (day <= 0) throw new ArgumentOutOfRangeException("day");
			if (day > 30 || (day == 30 && (month == HebrewMonth.טבת || month == HebrewMonth.אדר2 || month == HebrewMonth.אייר || month == HebrewMonth.תמוז || month == HebrewMonth.אלול)))
				throw new ArgumentOutOfRangeException("day");
			Month = month;
			Day = day;
		}

		///<summary>Gets the Hebrew month.</summary>
		public HebrewMonth Month { get; private set; }
		///<summary>Gets the Hebrew day.</summary>
		public int Day { get; private set; }

		///<summary>Converts this HebrewDayOfYear to its string representation.</summary>
		///<returns>A Hebrew date string, such as ג' ניסן.</returns>
		public override string ToString() {
			string month;
			switch (Month) {
				case HebrewMonth.אדר1: month = "אדר א"; break;
				case HebrewMonth.אדר2: month = "אדר ב"; break;
				default: month = Month.ToString(); break;
			}

			return Day.ToHebrewString(HebrewNumberFormat.LetterQuoted) + " " + month;
		}

		#region Equality
		///<summary>Checks whether this HebrewDayOfYear is equal to an object.</summary>
		///<param name="obj">The HebrewDayOfYear to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public override bool Equals(object obj) { return obj is HebrewDayOfYear && Equals((HebrewDayOfYear)obj); }
		///<summary>Checks whether this HebrewDayOfYear is equal to another Hebrew event.</summary>
		///<param name="other">The HebrewEvent to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(IHebrewEvent other) { return other is HebrewDayOfYear && Equals(other); }
		///<summary>Checks whether this HebrewDayOfYear is equal to another Hebrew date.</summary>
		///<param name="other">The HebrewDayOfYear to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(HebrewDayOfYear other) { return other.Month == Month && other.Day == Day; }
		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return Month.GetHashCode() ^ Day.GetHashCode(); }

		///<summary>Compares this HebrewDayOfYear to another Hebrew date.</summary>
		///<param name="other">The date to compare to.</param>
		///<returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
		public int CompareTo(HebrewDayOfYear other) {
			if (Month == other.Month) return Day.CompareTo(other.Day);
			return Month.CompareTo(other.Month);
		}

		///<summary>Checks whether two HebrewDayOfYears are equal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two HebrewDayOfYears are equal.</returns>
		public static bool operator ==(HebrewDayOfYear first, HebrewDayOfYear second) { return first.Equals(second); }
		///<summary>Checks whether two HebrewDayOfYears are unequal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two HebrewDayOfYears are unequal.</returns>
		public static bool operator !=(HebrewDayOfYear first, HebrewDayOfYear second) { return !(first == second); }

		///<summary>Checks whether one HebrewDayOfYear is after another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first HebrewDayOfYear is after the second.</returns>
		public static bool operator >(HebrewDayOfYear first, HebrewDayOfYear second) { return first.CompareTo(second) > 0; }
		///<summary>Checks whether one HebrewDayOfYear is before another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first HebrewDayOfYear is before the second.</returns>
		public static bool operator <(HebrewDayOfYear first, HebrewDayOfYear second) { return first.CompareTo(second) < 0; }

		///<summary>Checks whether one HebrewDayOfYear is after or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first HebrewDayOfYear is after or equal to the second.</returns>
		public static bool operator >=(HebrewDayOfYear first, HebrewDayOfYear second) { return first.CompareTo(second) >= 0; }
		///<summary>Checks whether one HebrewDayOfYear is before or equal to another.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the first HebrewDayOfYear is before or equal to the second.</returns>
		public static bool operator <=(HebrewDayOfYear first, HebrewDayOfYear second) { return first.CompareTo(second) <= 0; }
		#endregion

		///<summary>Checks whether this value describes a date.</summary>
		///<param name="date">The date to check.</param>
		///<returns>True if the date matches this instance's month and year.</returns>
		public bool Is(HebrewDate date) { return date.HebrewMonth == Month && date.HebrewDay == Day; }

		///<summary>Gets this date in the given year.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<returns>The HebrewDate of the given year and this instance's month and day.</returns>
		public HebrewDate GetDate(int hebrewYear) { return new HebrewDate(hebrewYear, Month, Day); }
	}
}
