using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ShomreiTorah.Common;
using ShomreiTorah.Common.Collections;

namespace ShomreiTorah.Data {
	///<summary>Contains strings used by the data classes.</summary>
	public static class Names {
		static ReadOnlyCollection<string> Strings(params string[] values) { return new ReadOnlyCollection<string>(values); }
		static ReadOnlyCollection<string> Strings(IEnumerable<string> firstValues, IEnumerable<string> moreValues) { return new ReadOnlyCollection<string>(firstValues.Concat(moreValues).ToList()); }

		#region Melave Malka
		///<summary>Gets the values for the Melave Malka's Source field.</summary>
		public static readonly ReadOnlyCollection<string> MelaveMalkaSources =
			Config.GetElement("Journal", "Invitations")
				  .Elements("Source")
				  .Select(e => e.Value)
				  .ToList()
				  .AsReadOnly();

		///<summary>Gets the pre-defined ad types in the journal.</summary>
		public static ReadOnlyCollection<AdType> AdTypes { get { return AdType.All; } }
		#endregion

		#region Billing-related
		///<summary>Gets the names of the standard billing accounts.</summary>
		public static readonly ReadOnlyCollection<string> AccountNames =
			Config.GetElement("Billing", "Accounts")
				  .Elements("Account")
				  .Select(e => e.Value)
				  .ToList()
				  .AsReadOnly();

		///<summary>Gets the default account for payments and pledges.</summary>
		public static string DefaultAccount { get { return AccountNames[0]; } }


		///<summary>Gets the payment methods that cannot be deposited.</summary>
		public static readonly ReadOnlyCollection<string> UndepositedPayments = Strings("Goods and/or Services", "Credit Card");
		///<summary>Gets the all of the standard payment methods.</summary>
		public static readonly ReadOnlyCollection<string> PaymentMethods = Strings(new[] { "Cash", "Check" }, UndepositedPayments);
		#endregion

		#region Pledge Types
		///<summary>Gets the subtype that indicates pledges that are not tax deductible.</summary>
		///<remarks>These pledges are subtracted from receipts.</remarks>
		public static readonly string NonDeductibleSubType = "Nondeductible";
		///<summary>Gets the pledge type for journal ads.</summary>
		public static PledgeType JournalPledgeType { get; } = new PledgeType(
			Config.ReadAttribute("Journal", "PledgeType"),
			AdTypes.Select(a => a.PledgeSubType).ToList()
		);


		///<summary>Gets the standard pledge types.</summary>
		public static readonly ReadOnlyCollection<PledgeType> PledgeTypes =
			Config.GetElement("Billing", "PledgeTypes")
				  .Elements("PledgeType")
				  .Select(PledgeType.FromXml)
				  .Concat(new[] { JournalPledgeType })
				  .ToList()
				  .AsReadOnly();
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

		internal static PledgeType FromXml(XElement element) {
			return new PledgeType(element.Attribute("Name").Value,
				element.Elements("SubType").Attributes("Name").Select(a => a.Value).ToList());
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
		///<summary>Gets the ordinal position of this ad type, used for the default ordering of ads within the journal.</summary>
		public int Index { get; private set; }
		///<summary>Gets the name of the ad type.</summary>
		public string Name { get; private set; }
		///<summary>Gets the price of the ad type.</summary>
		public decimal DefaultPrice { get; private set; }
		///<summary>Gets the price of the ad type.</summary>
		public int AdsPerPage { get; private set; }

		///<summary>Gets the name with price of the ad type.</summary>
		///<remarks>This is used in drop-down lists.</remarks>
		public string DisplayAs { get { return Name + " (" + DefaultPrice.ToString("c0", CultureInfo.CurrentCulture) + ")"; } }

		///<summary>Gets the subtype used for a pledge of this ad type.</summary>
		public string PledgeSubType { get; private set; }

		private AdType(int index, string name, decimal defaultPrice, int adsPerPage, string displayName) {
			Index = index;
			Name = name;
			DefaultPrice = defaultPrice;
			AdsPerPage = adsPerPage;
			PledgeSubType = displayName ?? (name + " ad");
		}

		///<summary>Returns a string representation of this ad type.</summary>
		public override string ToString() { return DisplayAs; }
		//Exposed by Names for consistency
		internal readonly static ReadOnlyCollection<AdType> All =
			Config.GetElement("Journal")
				.Elements("AdType")
				.Select((elem, index) => new AdType(index,
					elem.Attribute("Name").Value,
					decimal.Parse(elem.Attribute("DefaultPrice").Value, NumberStyles.Currency, CultureInfo.CurrentCulture),
					(int)elem.Attribute("AdsPerPage"),
					(string)elem.Attribute("DisplayName")	// Optional
				))
				.ToList()
				.AsReadOnly();
	}
}
