using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using ShomreiTorah.Common.Calendar.Zmanim.Temp;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

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
		static readonly JavaScriptSerializer jsonDecoder = new JavaScriptSerializer();

		static readonly Dictionary<string, string> keyMappings = new Dictionary<string, string> {
			{ "sunrise",			Zman.Sunrise.ToZmanName()				},
			{ "sunset",				Zman.Sunset.ToZmanName()				},
			{ "sof_zman_shema_ma",	Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א.ToZmanName()	},
			{ "sof_zman_shema_gra",	Zman.סוף٠זמן٠קריאת٠שמע٠גרא.ToZmanName()	},
			{ "sof_zman_tefila_ma",	Zman.סוף٠זמן٠תפילה.ToZmanName()			},
			{ "alos_ma",			Zman.עלות.ToZmanName()					},    
			{ "talis_ma",			Zman.טלית.ToZmanName()					},
			{ "chatzos",			Zman.חצות.ToZmanName()					},
			{ "mincha_gedola_ma",	Zman.מנחה٠גדולה.ToZmanName()				},
			{ "mincha_ketana_ma",	Zman.מנחה٠קטנה.ToZmanName()				},
			{ "plag_mincha_ma",		Zman.פלג٠המנחה.ToZmanName()				},
			{ "tzeis_595_degrees",	Zman.צאת72.ToZmanName()					},  
			{ "tzeis_850_degrees",	Zman.צאת42.ToZmanName()					},
			{ "tzeis_72_minutes",	Zman.צאת595.ToZmanName()				},
			{ "tzeis_42_minutes",	Zman.צאת850.ToZmanName()				},
		};

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

			var days = jsonDecoder.DeserializeObject(jsonString).GetKey<object[]>("days");
			return new YearlyZmanimDictionary(year, days.Select(d =>
				new ZmanimInfo(DateTime.ParseExact(d.GetKey<string>("engDateString"), "M/d/yyyy", CultureInfo.InvariantCulture),
						d.GetKey<Dictionary<string, object>>("zmanim").ToDictionary(kvp => keyMappings[kvp.Key], kvp => TimeSpan.Parse(kvp.Value.ToString()))
					)
				).ToArray());
		}

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public override DateTime MinDate { get { return DateTime.MinValue; } }

		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public override DateTime MaxDate { get { return DateTime.MaxValue; } }
	}
	namespace Temp {
		static class Extensions {
			public static T GetKey<T>(this object obj, string key) {
				return (T)((Dictionary<string, object>)obj)[key];
			}
		}
	}
}
