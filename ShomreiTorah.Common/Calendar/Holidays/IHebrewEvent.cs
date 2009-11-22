using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>Describes a specific event on the Hebrew Calendar.</summary>
	public interface IHebrewEvent : IEquatable<IHebrewEvent> {
		///<summary>Gets this date in the given year.</summary>
		///<param name="hebrewYear">The Hebrew year.</param>
		///<returns>The HebrewDate described by this value in the given year.</returns>
		HebrewDate GetDate(int hebrewYear);

		///<summary>Checks whether this value describes a date.</summary>
		///<param name="date">The date to check.</param>
		///<returns>True if this instance describes the date.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Not intended for external implementation")]
		bool Is(HebrewDate date);
	}
}
