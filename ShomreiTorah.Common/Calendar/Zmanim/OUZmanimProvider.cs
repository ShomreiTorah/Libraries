using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>Reads Zmanim from the OU's AJAX web service.</summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "WebClient doesn't override Dispose")]
	public class OUZmanimProvider : YearlyZmanimProvider {
		///<summary>Creates a new OUZmanimProvider instance.</summary>
		protected OUZmanimProvider() { }
		[ThreadStatic]
		static OUZmanimProvider defaultInstance;
		///<summary>Gets the OUZmanimProvider instance.</summary>
		///<remarks>Since OUZmanimProvider is not thread-safe, this will create a different instance for each thread.</remarks>
		public new static OUZmanimProvider Default {
			get {
				if (defaultInstance == null) defaultInstance = new OUZmanimProvider();	//Since it's ThreadStatic, I don't need to lock
				return defaultInstance;
			}
		}

		const string UrlFormat = @"http://www.ou.org/ou_services/getCalendarData.php?mode=grid&startDate={2:MM/dd/yyyy}&numberOfResults={3}&lat={0}&long={1}&timezone=America/New_York";
		readonly WebClient web = new WebClient();

		[DataContract]
		class OUDataObject {
			[DataMember(Name = "days")]
			public IList<OUDate> Days { get; set; }
		}
		[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "For deserialization")]
		[DataContract]
		class OUDate {
			public ZmanimInfo CreateZmanimInfo() { return new ZmanimInfo(Date, Zmanim.Values); }

			public DateTime Date { get; set; }

			[DataMember(Name = "engDateString")]
			public string DateString {
				get { return Date.ToString(); }
				set { Date = DateTime.ParseExact(value, "M/d/yyyy", CultureInfo.InvariantCulture); }
			}

			[DataMember(Name = "zmanim")]
			public OUZmanimSet Zmanim { get; set; }
		}
		[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "For deserialization")]
		[DataContract]
		class OUZmanimSet {
			Dictionary<Zman, TimeSpan> values;	//Deserialization will not run the ctor
			public Dictionary<Zman, TimeSpan> Values {
				get { return values = values ?? new Dictionary<Zman, TimeSpan>(); }
			}

			#region Zmanim properties
			[DataMember]
			public string sunrise {
				get { return Values[Zman.Sunrise].ToString(); }
				set { Values[Zman.Sunrise] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string sunset {
				get { return Values[Zman.Sunset].ToString(); }
				set { Values[Zman.Sunset] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string sof_zman_shema_ma {
				get { return Values[Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א].ToString(); }
				set { Values[Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string sof_zman_shema_gra {
				get { return Values[Zman.סוף٠זמן٠קריאת٠שמע٠גרא].ToString(); }
				set { Values[Zman.סוף٠זמן٠קריאת٠שמע٠גרא] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string sof_zman_tefila_ma {
				get { return Values[Zman.סוף٠זמן٠תפילה].ToString(); }
				set { Values[Zman.סוף٠זמן٠תפילה] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string alos_ma {
				get { return Values[Zman.עלות].ToString(); }
				set { Values[Zman.עלות] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string talis_ma {
				get { return Values[Zman.טלית].ToString(); }
				set { Values[Zman.טלית] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string chatzos {
				get { return Values[Zman.חצות].ToString(); }
				set { Values[Zman.חצות] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string mincha_gedola_ma {
				get { return Values[Zman.מנחה٠גדולה].ToString(); }
				set { Values[Zman.מנחה٠גדולה] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string mincha_ketana_ma {
				get { return Values[Zman.מנחה٠קטנה].ToString(); }
				set { Values[Zman.מנחה٠קטנה] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string plag_mincha_ma {
				get { return Values[Zman.פלג٠המנחה].ToString(); }
				set { Values[Zman.פלג٠המנחה] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string tzeis_595_degrees {
				get { return Values[Zman.צאת72].ToString(); }
				set { Values[Zman.צאת72] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string tzeis_850_degrees {
				get { return Values[Zman.צאת42].ToString(); }
				set { Values[Zman.צאת42] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string tzeis_72_minutes {
				get { return Values[Zman.צאת595].ToString(); }
				set { Values[Zman.צאת595] = TimeSpan.Parse(value); }
			}
			[DataMember]
			public string tzeis_42_minutes {
				get { return Values[Zman.צאת850].ToString(); }
				set { Values[Zman.צאת850] = TimeSpan.Parse(value); }
			}
			#endregion

			//sunset: Zman.Sunset
			//sof_zman_shema_ma: Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א
			//sof_zman_shema_gra: Zman.סוף٠זמן٠קריאת٠שמע٠גרא
			//sof_zman_tefila_ma: Zman.סוף٠זמן٠תפילה
			//alos_ma: Zman.עלות
			//talis_ma: Zman.טלית
			//chatzos: Zman.חצות
			//mincha_gedola_ma: Zman.מנחה٠גדולה
			//mincha_ketana_ma: Zman.מנחה٠קטנה
			//plag_mincha_ma: Zman.פלג٠המנחה
			//tzeis_595_degrees: Zman.צאת72	
			//tzeis_850_degrees: Zman.צאת42	
			//tzeis_72_minutes: Zman.צאת595	
			//tzeis_42_minutes: Zman.צאת850	

			//Replace 
			//{[a-z0-9_]+}"\::b+{.+}
			//with
			//[DataMember]\n public string \1 {\n get { return values[\2].ToString(); }\n set { values[\2] = TimeSpan.Parse(value); }\n }
		}

		static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OUDataObject));

		///<summary>Creates a YearlyZmanimDictionary for the given year.</summary>
		///<returns>The Zmanim for that year, or null if they could not be loaded.</returns>
		protected override YearlyZmanimDictionary CreateYear(int year) {
			//jsCall = "processJSONGridData(Giant JSON object)"
			var jsCall = web.DownloadString(new Uri(String.Format(CultureInfo.InvariantCulture, UrlFormat,
				Config.ReadAttribute("Zmanim", "Latitude"), Config.ReadAttribute("Zmanim", "Longitude"),
				new DateTime(year, 1, 1), DateTime.IsLeapYear(year) ? 366 : 365)));
			var start = jsCall.IndexOf('(') + 1;
			//jsonString = "Giant JSON object"
			var jsonString = jsCall.Substring(start, jsCall.Length - start - 1);

			OUDataObject data;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString), writable: false))
				data = (OUDataObject)serializer.ReadObject(stream);

			return new YearlyZmanimDictionary(year, data.Days.Select(day => day.CreateZmanimInfo()).ToArray());
		}

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public override DateTime MinDate { get { return DateTime.MinValue; } }

		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public override DateTime MaxDate { get { return DateTime.MaxValue; } }
	}
}
