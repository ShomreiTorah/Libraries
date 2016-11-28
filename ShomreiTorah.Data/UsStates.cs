using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShomreiTorah.Data {
	///<summary>Tracks all known states in the US.</summary>
	public static class UsStates {
		///<summary>Returns a state abbreviation, if a string is a state.  If not, returns the original string.</summary>
		public static string Abbreviate(string name) {
			string retVal;
			if (name != null && Values.TryGetValue(name, out retVal))
				return retVal;
			return name;
		}

		#region Values
		///<summary>Maps states to their abbreviations.</summary>
		public static IReadOnlyDictionary<string, string> Values { get; } = new Dictionary<string, string>(50, StringComparer.OrdinalIgnoreCase) {
			{ "Alabama",        "AL" },
			{ "Alaska",         "AK" },
			{ "Arizona",        "AZ" },
			{ "Arkansas",       "AR" },
			{ "California",     "CA" },
			{ "Colorado",       "CO" },
			{ "Connecticut",    "CT" },
			{ "Delaware",       "DE" },
			{ "Florida",        "FL" },
			{ "Georgia",        "GA" },
			{ "Hawaii",         "HI" },
			{ "Idaho",          "ID" },
			{ "Illinois",       "IL" },
			{ "Indiana",        "IN" },
			{ "Iowa",           "IA" },
			{ "Kansas",         "KS" },
			{ "Kentucky",       "KY" },
			{ "Louisiana",      "LA" },
			{ "Maine",          "ME" },
			{ "Maryland",       "MD" },
			{ "Massachusetts",  "MA" },
			{ "Michigan",       "MI" },
			{ "Minnesota",      "MN" },
			{ "Mississippi",    "MS" },
			{ "Missouri",       "MO" },
			{ "Montana",        "MT" },
			{ "Nebraska",       "NE" },
			{ "Nevada",         "NV" },
			{ "New Hampshire",  "NH" },
			{ "New Jersey",     "NJ" },
			{ "New Mexico",     "NM" },
			{ "New York",       "NY" },
			{ "North Carolina", "NC" },
			{ "North Dakota",   "ND" },
			{ "Ohio",           "OH" },
			{ "Oklahoma",       "OK" },
			{ "Oregon",         "OR" },
			{ "Pennsylvania",   "PA" },
			{ "Rhode Island",   "RI" },
			{ "South Carolina", "SC" },
			{ "South Dakota",   "SD" },
			{ "Tennessee",      "TN" },
			{ "Texas",          "TX" },
			{ "Utah",           "UT" },
			{ "Vermont",        "VT" },
			{ "Virginia"    ,   "VA" },
			{ "Washingto	n", "WA" },
			{ "West Virginia",  "WV" },
			{ "Wisconsin",      "WI" },
			{ "Wyoming",        "WY" }
		};
		#endregion
	}
}
