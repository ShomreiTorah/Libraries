using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>An event that occurs on the שבת between two dates.</summary>
	public struct SpecialשבתEvent : IHebrewEvent, IEquatable<SpecialשבתEvent> {
		///<summary>Creates a SpecialשבתEvent from a month and day.</summary>
		///<param name="month">The month of the earliest date that this שבת can occur.</param>
		///<param name="day">The date of the earliest date that this שבת can occur.</param>
		public SpecialשבתEvent(HebrewMonth month, int day) : this(new HebrewDayOfYear(month, day)) { }
		///<summary>Creates a SpecialשבתEvent.</summary>
		///<param name="earliest">The earliest date that this שבת can occur.</param>
		public SpecialשבתEvent(IHebrewEvent earliest) : this() { Earliest = earliest; }

		///<summary>Gets the earliest date that this שבת can occur.</summary>
		public IHebrewEvent Earliest { get; private set; }


		///<summary>Gets this date in the given year.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<returns>The HebrewDate of the given year described by this instance.</returns>
		public HebrewDate GetDate(int hebrewYear) { return Earliest.GetDate(hebrewYear).Next(DayOfWeek.Saturday); }

		///<summary>Checks whether this value describes a date.</summary>
		///<param name="date">The date to check.</param>
		///<returns>True if this value describes the date.</returns>
		public bool Is(HebrewDate date) { return date.DayOfWeek == DayOfWeek.Saturday && date == GetDate(date.HebrewYear); }

		#region Equality
		///<summary>Checks whether this instance is equal to an object.</summary>
		///<param name="obj">The object to check.</param>
		///<returns>True if this instance holds the same value as obj.</returns>
		public override bool Equals(object obj) { return obj is SpecialשבתEvent && Equals((SpecialשבתEvent)obj); }
		///<summary>Checks whether this instance is equal to an IHebrewEvent.</summary>
		///<param name="other">The IHebrewEvent to check.</param>
		///<returns>True if this instance holds the same value as other.</returns>
		public bool Equals(IHebrewEvent other) { return other is SpecialשבתEvent && Equals((SpecialשבתEvent)other); }

		///<summary>Checks whether this instance is equal to an SpecialשבתEvent.</summary>
		///<param name="other">The SpecialשבתEvent to check.</param>
		///<returns>True if this instance holds the same value as other.</returns>
		public bool Equals(SpecialשבתEvent other) { return Earliest == other.Earliest; }

		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return Earliest.GetHashCode(); }

		///<summary>Checks whether two SpecialשבתEvents are equal.</summary><param name="first">The first event.</param><param name="second">The second event.</param><returns>True if the two SpecialשבתEvents are equal.</returns>
		public static bool operator ==(SpecialשבתEvent first, SpecialשבתEvent second) { return first.Equals(second); }
		///<summary>Checks whether two SpecialשבתEvents are unequal.</summary><param name="first">The first event.</param><param name="second">The second event.</param><returns>True if the two SpecialשבתEvents are unequal.</returns>
		public static bool operator !=(SpecialשבתEvent first, SpecialשבתEvent second) { return !(first == second); }
		#endregion
	}
}
