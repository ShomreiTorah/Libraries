using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ShomreiTorah.Common.Collections;

namespace ShomreiTorah.Data {
	///<summary>Contains strings used by the data classes.</summary>
	public static class Names {
		static ReadOnlyCollection<string> Strings(params string[] values) { return new ReadOnlyCollection<string>(values); }
		static ReadOnlyCollection<string> Strings(IEnumerable<string> firstValues, IEnumerable<string> moreValues) { return new ReadOnlyCollection<string>(firstValues.Concat(moreValues).ToList()); }

		#region Melave Malka
		///<summary>Gets the values for the Melave Malka's Source field.</summary>
		public static readonly ReadOnlyCollection<string> MelaveMalkaSources = Strings("Shul", "Rav", "Honoree");
		///<summary>Gets the pre-defined ad types in the journal.</summary>
		public static ReadOnlyCollection<AdType> AdTypes { get { return AdType.All; } }
		#endregion

		#region Billing-related
		///<summary>Gets the names of the standard billing accounts.</summary>
		public static readonly ReadOnlyCollection<string> AccountNames = Strings("Operating Fund", "Building Fund");

		///<summary>Gets the default account for payments and pledges.</summary>
		public static string DefaultAccount { get { return AccountNames[0]; } }


		///<summary>Gets the payment methods that cannot be deposited.</summary>
		public static readonly ReadOnlyCollection<string> UndepositedPayments = Strings("Goods and/or Services");
		///<summary>Gets the all of the standard payment methods.</summary>
		public static readonly ReadOnlyCollection<string> PaymentMethods = Strings(new[] { "Cash", "Check" }, UndepositedPayments);
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

			new PledgeType("Auction",		  //This is the sort order for rows in the auction grid, and for subtypes
							subTypes: new[] { "פתיחה", "אתה הראית",  
											  "כהן", "לוי", "שלישי", "רביעי", "חמישי", "שישי", "שביעי", 

											  "כל הנערים", "חתן תורה", "חתן בראשית",

											  "מפטיר יונה", "מפטיר", "הגבהה", 
											  "פתיחה דנעילה", "פתיחה דגשם",
											}),
			new PledgeType("מי שברך",	
							subTypes: new[] {	"כהן", "לוי", "שלישי", "רביעי", "חמישי", "שישי", "שביעי", "מפטיר",
												"מפטיר יונה", "חתן תורה", "חתן בראשית" }),
			new PledgeType("Melave Malka Journal", AdTypes.Select(a => a.PledgeSubType).ToArray()),
			new PledgeType("Melave Malka Raffle"),
			new PledgeType("Shalach Manos"),
		});
		#endregion

		#region General
		///<summary>Gets the standard types of relatives.</summary>
		public static readonly ReadOnlyCollection<string> RelationNames = Strings(
			"Brother", "Brother-in-law",
			"Father", "Father-in-law",
			"Son", "Son-in-law",
			"Grandfather", "Uncle",
			"Guest"
		);

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

	///<summary>Represents a type of ad in the journal.</summary>
	public class AdType {
		///<summary>Gets the ID of the ad type.</summary>
		public int Id { get; private set; }
		///<summary>Gets the name of the ad type.</summary>
		public string Name { get; private set; }
		///<summary>Gets the price of the ad type.</summary>
		public int DefaultPrice { get; private set; }
		///<summary>Gets the price of the ad type.</summary>
		public int AdsPerPage { get; private set; }

		///<summary>Gets the name with price of the ad type.</summary>
		///<remarks>This is used in drop-down lists.</remarks>
		public string DisplayAs { get { return Name + " (" + DefaultPrice.ToString("c0", CultureInfo.CurrentCulture) + ")"; } }

		///<summary>Gets the subtype used for a pledge of this ad type.</summary>
		public string PledgeSubType {
			get {
				if (Name == "Greeting")
					return "Greeting ad";
				if (DefaultPrice > 180)
					return Name + " ad";
				else
					return Name + " page ad";
			}
		}

		private AdType(int id, string name, int defaultPrice, int adsPerPage) { Id = id; Name = name; DefaultPrice = defaultPrice; AdsPerPage = adsPerPage; }

		///<summary>Returns a string representation of this ad type.</summary>
		public override string ToString() { return DisplayAs; }
		//Exposed by Names for consistency
		internal readonly static ReadOnlyCollection<AdType> All = new ReadOnlyCollection<AdType>(new AdType[] { 
			new AdType(1,	"Diamond",	1000,	1),
			new AdType(2,	"Platinum",	750,	1),
			new AdType(3,	"Gold",		500,	1),
			new AdType(4,	"Silver",	360,	1),
			new AdType(5,	"Bronze",	250,	1),
			new AdType(6,	"Full",		180,	1),
			new AdType(7,	"Half",		100,	2),
			new AdType(8,	"Quarter",	72,		4),
			new AdType(9,	"Greeting",	36,		10)
		});
	}
}
