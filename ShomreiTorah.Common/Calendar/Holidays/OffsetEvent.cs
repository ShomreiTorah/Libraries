using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>An event that occurs a given number of days from another event.</summary>
	///<remarks>This event is used for חנוכה to handle the variable length of כסלו.</remarks>
	public struct OffsetEvent : IHebrewEvent, IEquatable<OffsetEvent> {
		///<summary>Creates a SpecialשבתEvent.</summary>
		///<param name="start">The date that this event is offset from.</param>
		///<param name="offset">The number of days after start that this event occurs.</param>
		public OffsetEvent(IHebrewEvent start, int offset) : this() { Start = start; Offset = offset; }

		///<summary>Gets the date that this event is offset from.</summary>
		public IHebrewEvent Start { get; private set; }
		///<summary>Gets the number of days after Start that this event occurs.</summary>
		public int Offset { get; private set; }

		///<summary>Gets this date in the given year.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<returns>The HebrewDate of the given year described by this instance.</returns>
		public HebrewDate GetDate(int hebrewYear) { return Start.GetDate(hebrewYear) + Offset; }

		///<summary>Checks whether this value describes a date.</summary>
		///<param name="date">The date to check.</param>
		///<returns>True if this value describes the date.</returns>
		public bool Is(HebrewDate date) { return date == GetDate(date.HebrewYear); }

		#region Equality
		///<summary>Checks whether this instance is equal to an object.</summary>
		///<param name="obj">The object to check.</param>
		///<returns>True if this instance holds the same value as obj.</returns>
		public override bool Equals(object obj) { return obj is OffsetEvent && Equals((OffsetEvent)obj); }
		///<summary>Checks whether this instance is equal to an IHebrewEvent.</summary>
		///<param name="other">The IHebrewEvent to check.</param>
		///<returns>True if this instance holds the same value as other.</returns>
		public bool Equals(IHebrewEvent other) { return other is OffsetEvent && Equals((OffsetEvent)other); }

		///<summary>Checks whether this instance is equal to an OffsetEvent.</summary>
		///<param name="other">The OffsetEvent to check.</param>
		///<returns>True if this instance holds the same value as other.</returns>
		public bool Equals(OffsetEvent other) { return Offset == other.Offset && Start == other.Start; }

		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return Start.GetHashCode() ^ Offset.GetHashCode(); }

		///<summary>Checks whether two SpecialOffsetEvents are equal.</summary><param name="first">The first event.</param><param name="second">The second event.</param><returns>True if the two SpecialOffsetEvents are equal.</returns>
		public static bool operator ==(OffsetEvent first, OffsetEvent second) { return first.Equals(second); }
		///<summary>Checks whether two SpecialOffsetEvents are unequal.</summary><param name="first">The first event.</param><param name="second">The second event.</param><returns>True if the two SpecialOffsetEvents are unequal.</returns>
		public static bool operator !=(OffsetEvent first, OffsetEvent second) { return !(first == second); }
		#endregion
	}
}
