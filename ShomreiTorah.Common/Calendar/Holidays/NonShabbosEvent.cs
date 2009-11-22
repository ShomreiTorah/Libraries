using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>An event that cannot occur on שבת.</summary>
	public struct NonשבתEvent : IHebrewEvent, IEquatable<NonשבתEvent> {
		///<summary>Creates a NonשבתEvent.</summary>
		///<param name="month">The month that the event occurs on.</param>
		///<param name="normalDay">The day of the month that this event normally occurs on.</param>
		///<param name="fallback">The day of the week that this event occurs on instead of שבת.</param>
		public NonשבתEvent(HebrewMonth month, int normalDay, DayOfWeek fallback)
			: this() {
			if (month == HebrewMonth.None) throw new ArgumentOutOfRangeException("month", "None is not a valid month");
			if (normalDay <= 0
			 || normalDay > 30
			 || (normalDay == 30 && (month == HebrewMonth.טבת || month == HebrewMonth.אדר2 || month == HebrewMonth.אייר || month == HebrewMonth.תמוז || month == HebrewMonth.אלול)))
				throw new ArgumentOutOfRangeException("normalDay");
			if (fallback == DayOfWeek.Saturday) throw new ArgumentOutOfRangeException("fallback", "Fallback cannot be שבת");

			Month = month;
			NormalDay = normalDay;
			Fallback = fallback;
		}

		///<summary>Gets the month that this event occurs on.</summary>
		public HebrewMonth Month { get; private set; }

		///<summary>Gets the day of the month that this event normally occurs on.</summary>
		public int NormalDay { get; private set; }

		///<summary>Gets the day of the week that this event occurs on instead of שבת.</summary>
		public DayOfWeek Fallback { get; private set; }

		#region Equality

		///<summary>Checks whether this NonשבתEvent is equal to an object.</summary>
		///<param name="obj">The NonשבתEvent to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public override bool Equals(object obj) { return obj is NonשבתEvent && Equals((NonשבתEvent)obj); }
		///<summary>Checks whether this NonשבתEvent is equal to another Hebrew event.</summary>
		///<param name="other">The HebrewEvent to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(IHebrewEvent other) { return other is NonשבתEvent && Equals(other); }
		///<summary>Checks whether this NonשבתEvent is equal to another Hebrew date.</summary>
		///<param name="other">The NonשבתEvent to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(NonשבתEvent other) { return other.Month == Month && other.NormalDay == NormalDay && other.Fallback == Fallback; }
		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return Month.GetHashCode() ^ NormalDay.GetHashCode() ^ Fallback.GetHashCode(); }



		///<summary>Checks whether two NonשבתEvents are equal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two NonשבתEvents are equal.</returns>
		public static bool operator ==(NonשבתEvent first, NonשבתEvent second) { return first.Equals(second); }
		///<summary>Checks whether two NonשבתEvents are unequal.</summary><param name="first">The first date.</param><param name="second">The second date.</param><returns>True if the two NonשבתEvents are unequal.</returns>
		public static bool operator !=(NonשבתEvent first, NonשבתEvent second) { return !(first == second); }
		#endregion

		///<summary>Checks whether this value describes a date.</summary>
		///<param name="date">The date to check.</param>
		///<returns>True if this value describes the date.</returns>
		public bool Is(HebrewDate date) {
			if (date.HebrewMonth != Month) return false;
			return GetDate(date.HebrewYear) == date;
		}

		///<summary>Gets this date in the given year.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<returns>The HebrewDate of the given year and this instance's month and day.</returns>
		public HebrewDate GetDate(int hebrewYear) {
			var retVal = new HebrewDate(hebrewYear, Month, NormalDay);
			if (retVal.DayOfWeek != DayOfWeek.Saturday)
				return retVal;
			if (Fallback < DayOfWeek.Wednesday)
				return retVal + (1 + (int)Fallback);
			return retVal - (6 - (int)Fallback);
		}
	}
}
