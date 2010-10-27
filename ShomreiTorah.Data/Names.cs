using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using ShomreiTorah.Common;

namespace ShomreiTorah.Data {
	///<summary>Contains strings used by the data classes.</summary>
	public static class Names {
		static ReadOnlyCollection<string> Strings(params string[] values) { return new ReadOnlyCollection<string>(values); }

		#region Billing-related
		///<summary>Gets the names of the standard billing accounts.</summary>
		public static readonly ReadOnlyCollection<string> AccountNames = Strings("Operating Fund", "Building Fund");

		///<summary>Gets the default account for payments and pledges.</summary>
		public static string DefaultAccount { get { return AccountNames[0]; } }

		///<summary>Gets the standard payment methods.</summary>
		public static readonly ReadOnlyCollection<string> PaymentMethods = Strings("Cash", "Check");
		#endregion

		#region Pledge Types
		///<summary>Gets the standard pledge types.</summary>
		public static readonly ReadOnlyCollection<PledgeType> PledgeTypes = new ReadOnlyCollection<PledgeType>(new[]{
			new PledgeType("Donation"),
			new PledgeType("Membership"),
			new PledgeType("סעודה שלישית"),
			new PledgeType("Kiddush",		subTypes: new[]{ "ראש השנה", "שמחת תורה", "חתנים", "שבועות" }),
			new PledgeType("Seforim"),
			new PledgeType("Building Fund"),
			new PledgeType("ימים נוראים Seats"),

			new PledgeType("Auction",	
							subTypes: new[] {	"כהן", "לוי", "שלישי", "רביעי", "חמישי", "שישי", "שביעי", "מפטיר",
												"פתיחה", "הגבהה",
												"מפטיר יונה", "פתיחה דנעילה", 
												"אתה הראית", "חתן תורה", "חתן בראשית" }),
			new PledgeType("מי שברך",	
							subTypes: new[] {	"כהן", "לוי", "שלישי", "רביעי", "חמישי", "שישי", "שביעי", "מפטיר",
												"מפטיר יונה", "אתה הראית", "חתן תורה", "חתן בראשית" }),
			new PledgeType("Melave Malka Journal", 
							subTypes: new[]{	"Diamond ad",	"Platinum ad",		"Gold ad",
												"Silver ad",	"Bronze ad",		"Full page ad", 
												"Half page ad", "Quarter page ad",	"Greeting ad" }),
			new PledgeType("Melave Malka Raffle"),
			new PledgeType("Shalach Manos"),
		});
		#endregion

		#region General
		///<summary>Gets a dictionary mapping state abbreviations to their full names.</summary>
		public static readonly ReadOnlyDictionary<string, string> States = new ReadOnlyDictionary<string, string>(new SortedDictionary<string, string>{
			#region State names
			{ "AL",	"Alabama" },
			{ "AK",	"Alaska" },
			{ "AZ",	"Arizona" },
			{ "AR",	"Arkansas" },
			{ "CA",	"California" },
			{ "CO",	"Colorado" },
			{ "CT",	"Connecticut" },
			{ "DE",	"Delaware" },
			{ "FL",	"Florida" },
			{ "GA",	"Georgia" },
			{ "HI",	"Hawaii" },
			{ "ID",	"Idaho" },
			{ "IL",	"Illinois" },
			{ "IN",	"Indiana" },
			{ "IA",	"Iowa" },
			{ "KS",	"Kansas" },
			{ "KY",	"Kentucky" },
			{ "LA",	"Louisiana" },
			{ "ME",	"Maine" },
			{ "MD",	"Maryland" },
			{ "MA",	"Massachusetts" },
			{ "MI",	"Michigan" },
			{ "MN",	"Minnesota" },
			{ "MS",	"Mississippi" },
			{ "MO",	"Missouri" },
			{ "MT",	"Montana" },
			{ "NE",	"Nebraska" },
			{ "NV",	"Nevada" },
			{ "NH",	"New Hampshire" },
			{ "NJ",	"New Jersey" },
			{ "NM",	"New Mexico" },
			{ "NY",	"New York" },
			{ "NC",	"North Carolina" },
			{ "ND",	"North Dakota" },
			{ "OH",	"Ohio" },
			{ "OK",	"Oklahoma" },
			{ "OR",	"Oregon" },
			{ "PA",	"Pennsylvania" },
			{ "RI",	"Rhode Island" },
			{ "SC",	"South Carolina" },
			{ "SD",	"South Dakota" },
			{ "TN",	"Tennessee" },
			{ "TX",	"Texas" },
			{ "UT",	"Utah" },
			{ "VT",	"Vermont" },
			{ "VA",	"Virginia" },
			{ "WA",	"Washington" },
			{ "WV",	"West Virginia" },
			{ "WI",	"Wisconsin" },
			{ "WY",	"Wyoming" }		
			#endregion
		});

		///<summary>Gets the abbreviations of the US states.</summary>
		public static readonly ReadOnlyCollection<string> StateAbbreviations = new ReadOnlyCollection<string>(States.Keys.ToArray());

		///<summary>Gets the abbreviations of the states commonly found in the data.</summary>
		public static readonly ReadOnlyCollection<string> CommonStates = Strings("NJ", "NY");
		#endregion
	}
	///<summary>Contains information about a standard pledge type.</summary>
	public sealed class PledgeType {
		///<summary>Creates a PledgeType instance.</summary>
		internal PledgeType(string name, IList<string> subTypes = null) {
			if (name == null) throw new ArgumentNullException("name");
			Name = name;
			Subtypes = new ReadOnlyCollection<string>(subTypes ?? new string[0]);
		}

		///<summary>Gets the name of the pledge type.</summary>
		public string Name { get; private set; }
		///<summary>Gets the standard subtypes of this pledge type, if any.</summary>
		public ReadOnlyCollection<String> Subtypes { get; private set; }

		///<summary>Returns the name of the pledge type.</summary>
		public override string ToString() { return Name; }
	}
}
