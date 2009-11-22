using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>Contains the Zmanim for a specific date.</summary>
	public class ZmanimInfo {
		///<summary>Creates a ZmanimInfo instance for the given date with the given times.</summary>
		public ZmanimInfo(DateTime date, IDictionary<Zman, TimeSpan> times) { Date = date; Times = times.ToDictionary(kvp => ToZmanName(kvp.Key), kvp => kvp.Value); }
		///<summary>Creates a ZmanimInfo instance for the given date with the given times.</summary>
		public ZmanimInfo(DateTime date, IDictionary<string, TimeSpan> times) { Date = date; Times = times; }

		///<summary>Gets the date described by this instance.</summary>
		public DateTime Date { get; private set; }

		///<summary>Gets the Zmanim for the date.</summary>
		public IDictionary<string, TimeSpan> Times { get; private set; }

		///<summary>Gets a Zman.</summary>
		public TimeSpan this[string name] { get { return Times[name]; } }
		///<summary>Gets a Zman.</summary>
		public TimeSpan this[Zman name] { get { return Times[name.ToZmanName()]; } }

		///<summary>Converts this ZmanimInfo instance to its string representation.</summary>
		public override string ToString() {
			return "Zmanim for " + Date.ToLongDateString();
		}

		///<summary>The character used instead of a space in Zmanim identifiers.</summary>
		public const char SpaceChar = '٠';
		#region Names
		TimeSpan GetZman(Zman name) {
			TimeSpan retVal;
			Times.TryGetValue(name.ToZmanName(), out retVal);
			return retVal;
		}

		///<summary>Gets the זמן for sunrise, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan Sunrise { get { return GetZman(Zman.Sunrise); } }
		///<summary>Gets the זמן for sunset, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan Sunset { get { return GetZman(Zman.Sunset); } }
		///<summary>Gets the זמן for סוף זמן קריאת שמע according to the מגן אברהם, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan סוף٠זמן٠קריאת٠שמע٠מ٠א { get { return GetZman(Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א); } }
		///<summary>Gets the זמן for סוף זמן קריאת שמע according to the גרא, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan סוף٠זמן٠קריאת٠שמע٠גרא { get { return GetZman(Zman.סוף٠זמן٠קריאת٠שמע٠גרא); } }
		///<summary>Gets the זמן for סוף זמן תפילה, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan סוף٠זמן٠תפילה { get { return GetZman(Zman.סוף٠זמן٠תפילה); } }
		///<summary>Gets the זמן for עלות השחר, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan עלות { get { return GetZman(Zman.עלות); } }
		///<summary>Gets the זמן for טלית, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan טלית { get { return GetZman(Zman.טלית); } }
		///<summary>Gets the זמן for חצות, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan חצות { get { return GetZman(Zman.חצות); } }
		///<summary>Gets the זמן for מנחה גדולה, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan מנחה٠גדולה { get { return GetZman(Zman.מנחה٠גדולה); } }
		///<summary>Gets the זמן for מנחה קטנה, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan מנחה٠קטנה { get { return GetZman(Zman.מנחה٠קטנה); } }
		///<summary>Gets the זמן for פלג המנחה, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan פלג٠המנחה { get { return GetZman(Zman.פלג٠המנחה); } }
		///<summary>Gets the זמן for צאת הכוכבים at 72 minutes, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan צאת72 { get { return GetZman(Zman.צאת72); } }
		///<summary>Gets the זמן for צאת הכוכבים at 42 minutes, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan צאת42 { get { return GetZman(Zman.צאת42); } }
		///<summary>Gets the זמן for צאת הכוכבים at 5.95°, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan צאת595 { get { return GetZman(Zman.צאת595); } }
		///<summary>Gets the זמן for צאת הכוכבים at 8.50°, or TimeSpan.Zero if it is not defined in this instance.</summary>
		public TimeSpan צאת850 { get { return GetZman(Zman.צאת850); } }

		///<summary>Gets the human-readable name of a Zman enum value.</summary>
		public static string ToZmanName(Zman zman) { return ZmanNames[zman]; }
		///<summary>Gets a Zman enum value from its human-readable name.</summary>
		public static Zman FromZmanName(string zmanName) { return ZmanNames.First(kvp => kvp.Value == zmanName).Key; }

		static readonly Dictionary<Zman, string> ZmanNames = new Dictionary<Zman, string> {
			{ Zman.Sunrise,					"Sunrise"			},
			{ Zman.Sunset,					"Sunset"			},
			{ Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א,	"סוף זמן קריאת שמע according to the מגן אברהם"	},
			{ Zman.סוף٠זמן٠קריאת٠שמע٠גרא,	"סוף זמן קריאת שמע according to the גרא"			},
			{ Zman.סוף٠זמן٠תפילה,			"סוף זמן תפילה"		},
			{ Zman.עלות,					"עלות השחר"			},
			{ Zman.טלית,					"טלית"				},
			{ Zman.חצות,					"חצות"				},
			{ Zman.מנחה٠גדולה,				"מנחה גדולה"			},
			{ Zman.מנחה٠קטנה,				"מנחה קטנה"			},
			{ Zman.פלג٠המנחה,				"פלג המנחה"			},
			{ Zman.צאת72,					"צאת הכוכבים at 72 minutes"	},
			{ Zman.צאת42,					"צאת הכוכבים at 42 minutes"	},
			{ Zman.צאת595,					"צאת הכוכבים at 5.95°"		},
			{ Zman.צאת850,					"צאת הכוכבים at 8.50°"		}
		};
		#endregion
	}

	///<summary>The standard set of Zmanim.</summary>
	public enum Zman {
		///<summary>The זמן for sunrise.</summary>
		Sunrise,
		///<summary>The זמן for sunset.</summary>
		Sunset,
		///<summary>The זמן for סוף זמן קריאת שמע according to the מגן אברהם.</summary>
		סוף٠זמן٠קריאת٠שמע٠מ٠א,
		///<summary>The זמן for סוף זמן קריאת שמע according to the גרא.</summary>
		סוף٠זמן٠קריאת٠שמע٠גרא,
		///<summary>The זמן for סוף זמן תפילה.</summary>
		סוף٠זמן٠תפילה,
		///<summary>The זמן for עלות השחר.</summary>
		עלות,
		///<summary>The זמן for טלית.</summary>
		טלית,
		///<summary>The זמן for חצות.</summary>
		חצות,
		///<summary>The זמן for מנחה גדולה.</summary>
		מנחה٠גדולה,
		///<summary>The זמן for מנחה קטנה.</summary>
		מנחה٠קטנה,
		///<summary>The זמן for פלג המנחה.</summary>
		פלג٠המנחה,
		///<summary>The זמן for צאת הכוכבים at 72 minutes.</summary>
		צאת72,
		///<summary>The זמן for צאת הכוכבים at 42 minutes.</summary>
		צאת42,
		///<summary>The זמן for צאת הכוכבים at 5.95°.</summary>
		צאת595,
		///<summary>The זמן for צאת הכוכבים at 8.50°.</summary>
		צאת850
	}
}
