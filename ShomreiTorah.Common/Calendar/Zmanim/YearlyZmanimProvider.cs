using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>A base class for ZmanimProviders that cache data using a list of YearlyZmanimDictionaries.</summary>
	public abstract class YearlyZmanimProvider : ZmanimProvider {
		///<summary>The year corresponding to the index 0 in the list of years.</summary>
		protected const int FirstYear = 2000;
		const char Separator = ',';
		readonly List<YearlyZmanimDictionary> years = new List<YearlyZmanimDictionary>(32);


		///<summary>Gets the Zmanim for the given date.</summary>
		public override ZmanimInfo GetZmanim(DateTime targetDate) {
			if (targetDate.Year < FirstYear)
				return null;

			var index = targetDate.Year - FirstYear;					//Find the index in years for the requested year.
			if (index >= years.Count || years[index] == null)	//If we haven't loaded that year yet, 
				if (!LoadYear(targetDate.Year))						//If we can't load it,
					return null;								//Give up.  We'll call LoadYear again next time, in case something changed.

			return years[index][targetDate];
		}

		///<summary>Loads the Zmanim for the given year, calling CreateYear to find the Zmanim.</summary>
		///<returns>True if the Zmanim were loaded successfully.</returns>
		protected bool LoadYear(int year) {
			if (year < FirstYear)
				throw new ArgumentOutOfRangeException("year", "Year must be on or after the year " + FirstYear);

			//Add the year to the array even if we don't load the file.
			var yearIndex = year - FirstYear;
			if (yearIndex >= years.Count)
				years.AddRange(new YearlyZmanimDictionary[1 + yearIndex - years.Count]);

			var values = CreateYear(year);
			if (values == null) return false;
			if (values.Year != year)
				throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "CreateYear returned {0} instead of {1}.", values.Year, year));

			years[yearIndex] = values;
			return true;
		}
		///<summary>Creates a YearlyZmanimDictionary for the given year.</summary>
		///<returns>The Zmanim for that year, or null if they could not be loaded.</returns>
		protected abstract YearlyZmanimDictionary CreateYear(int year);
	}
}
