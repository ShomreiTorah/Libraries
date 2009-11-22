using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common.Calendar {
	partial struct HebrewDate {
		///<summary>Gets the פרשה of the שבת following this date, or null if it is שבת יום טוב or שבת חול המעוד.</summary>
		public string Parsha {
			get {
				var shabbos = DayOfWeek == DayOfWeek.Saturday ? this : this + (6 - (int)DayOfWeek);
				var roshHashana = new HebrewDate(shabbos.HebrewYear, HebrewMonth.תשרי, 1);
				return parshaIndices[roshHashana, (shabbos - roshHashana).Days / 7];
			}
		}
		///<summary>Finds the שבת of the given פרשה in this date's year.</summary>
		///<param name="parsha">The name of the single פרשה to find.</param>
		///<returns>The שבת of the פרשה.</returns>
		public HebrewDate Findפרשה(string parsha) { return Findפרשה(HebrewYear, parsha); }
		///<summary>Finds the שבת of the given פרשה in this date's year.</summary>
		///<param name="parshaIndex">The index within the Parshiyos collection of the single פרשה to find.</param>
		///<returns>The שבת of the פרשה.</returns>
		public HebrewDate Findפרשה(int parshaIndex) { return Findפרשה(HebrewYear, parshaIndex); }

		///<summary>Finds the שבת of the given פרשה.</summary>
		///<param name="hebrewYear">The Hebrew year to look in.</param>
		///<param name="parsha">The name of the single פרשה to find.</param>
		///<returns>The שבת of the פרשה.</returns>
		public static HebrewDate Findפרשה(int hebrewYear, string parsha) {
			var index = Parshiyos.IndexOf(parsha);
			if (index < 0) throw new ArgumentException("Parsha '" + parsha + "' does not exist", "parsha");
			return Findפרשה(hebrewYear, index);
		}
		///<summary>Finds the שבת of the given פרשה.</summary>
		///<param name="hebrewYear">The Hebrew year to look in.</param>
		///<param name="parshaIndex">The index within the Parshiyos collection of the single פרשה to find.</param>
		///<returns>The שבת of the פרשה.</returns>
		public static HebrewDate Findפרשה(int hebrewYear, int parshaIndex) {
			if (parshaIndex < 0 || parshaIndex > Parshiyos.Count) throw new ArgumentOutOfRangeException("parshaIndex");
			if (parshaIndex > 52) throw new ArgumentOutOfRangeException("parshaIndex", "Cannot search for double פרשה.");

			var roshHashana = new HebrewDate(hebrewYear, HebrewMonth.תשרי, 1);
			var array = parshaIndices[roshHashana];
			var weekNumber = Array.IndexOf(array, parshaIndex);

			//If this year doesn't have the פרשה separately, find the double פרשה.
			//First, find the index in Parshiyos of the double פרשה's name.
			//Then, locate that index in the year's array.
			if (weekNumber == -1)
				weekNumber = Array.IndexOf(array, Enumerable.Range(53, 7).First(i => Parshiyos[i].Contains(Parshiyos[parshaIndex])));
			//TODO: Years without וילך
			//Four  year types don't have פרשת וילך. (See below)
			//Right now, I return פרשת וילך of the previous (!) year
			//(weekNumber becomes -1 because the double פרשה wasn't 
			//found, and, coincidentally, the last week of the previous
			//year has וילך for all of these ParshaArrayKeys)

			//Maybe I should throw, maybe I should return next year,
			//maybe I should leave as is.
			

			//These are the troublesome ParshaArrayKeys:
			//True,		חסר,	Thursday 
			//False,	כסדרן,	Thursday 
			//False,	חסר,	Saturday 
			//False,	מלא,	Thursday 


			//Round ראש השנה to the next שבת before adding weeks.
			return roshHashana + (6 - (int)roshHashana.DayOfWeek + weekNumber * 7);
		}

		#region Data
		///<summary>Gets the Hebrew names of the פרשיות.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "ReadOnlyCollection is immutable")]
		public static readonly ReadOnlyCollection<string> Parshiyos = new ReadOnlyCollection<string>(new[]{
			"בראשית", "נח", "לך לך", "וירא", "חיי שרה", "תולדות", "ויצא", "וישלח", "וישב", "מקץ",
			"ויגש", "ויחי", "שמות", "וארא", "בא", "בשלח", "יתרו", "משפטים", "תרומה", "תצוה", 
			"כי תשא", "ויקהל", "פקודי", "ויקרא", "צו", "שמיני", "תזריע", "מצורע", "אחרי מות", "קדושים",
			"אמור", "בהר", "בחוקותי", "במדבר", "נשא", "בהעלותך", "שלח", "קרח", "חוקת", "בלק", 
			"פינחס", "מטות", "מסעי", "דברים", "ואתחנן", "עקב", "ראה", "שופטים", "כי תצא",
			"כי תבוא", "נצבים", "וילך", "האזינו", "ויקהל־פקודי", "תזריע־מצורע", "אחרי מות־קדושים",
			"בהר־בחוקותי", "חוקת־בלק", "מטות־מסעי", "נצבים־וילך"
		});
		struct ParshaArrayKey {
			public ParshaArrayKey(bool leapYear, HebrewYearType yearType, DayOfWeek rhDay) : this() { LeapYear = leapYear; YearType = yearType; RoshHashanah = rhDay; }
			public override bool Equals(object obj) {
				if (!(obj is ParshaArrayKey)) return false;
				var other = (ParshaArrayKey)obj;
				return other.LeapYear == LeapYear && other.YearType == YearType && other.RoshHashanah == RoshHashanah;
			}
			//public override int GetHashCode() { return LeapYear.GetHashCode() ^ YearType.GetHashCode() ^ RoshHashanah.GetHashCode(); }
			public override int GetHashCode() { return LeapYear ? 1 : 0 + 8 * (int)YearType + 32 * (int)RoshHashanah; }
			public static bool operator ==(ParshaArrayKey first, ParshaArrayKey second) { return first.Equals(second); }
			public static bool operator !=(ParshaArrayKey first, ParshaArrayKey second) { return !(first == second); }

			public bool LeapYear { get; private set; }
			public HebrewYearType YearType { get; private set; }
			public DayOfWeek RoshHashanah { get; private set; }
		}
		class ParshaArrayDict : Dictionary<ParshaArrayKey, int[]> {
			public void Add(bool leapYear, HebrewYearType yearType, DayOfWeek rhDay, int[] values) {
				Add(new ParshaArrayKey(leapYear, yearType, rhDay), values);
			}

			public int[] this[HebrewDate roshHashana] { get { return this[roshHashana.IsLeapYear, roshHashana.YearType, roshHashana.DayOfWeek]; } }
			public int[] this[bool leapYear, HebrewYearType yearType, DayOfWeek rhDay] {
				get { return this[new ParshaArrayKey(leapYear, yearType, rhDay)]; }
			}

			public string this[HebrewDate roshHashana, int weekOfYear] { get { return this[roshHashana.IsLeapYear, roshHashana.YearType, roshHashana.DayOfWeek, weekOfYear]; } }
			public string this[bool leapYear, HebrewYearType yearType, DayOfWeek rhDay, int weekOfYear] {
				get {
					var index = this[leapYear, yearType, rhDay][weekOfYear];
					return index < 0 ? null : Parshiyos[index];
				}
			}
		}

		static readonly ParshaArrayDict parshaIndices = new Func<ParshaArrayDict>(() => {
			var d = new ParshaArrayDict {
				//		חסר Normal		———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
				{ false, HebrewYearType.חסר,	DayOfWeek.Monday,   new [] { 51, 52, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 53, 
																			 23, 24, -1, 25, 54, 55, 30, 56, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59} },
				{ false, HebrewYearType.חסר,	DayOfWeek.Saturday, new [] { -1, 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 
																			 53, 23, 24, -1, 25, 54, 55, 30, 56, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 50} },

				//		כסדרן Normal	———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
				{ false, HebrewYearType.כסדרן,	DayOfWeek.Thursday, new [] { 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 53,
																			 23, 24, -1, -1, 25, 54, 55, 30, 56, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 50} },

				//		מלא Normal		———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
				{ false, HebrewYearType.מלא,	DayOfWeek.Monday,	new [] { 51, 52, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 53, 
																			 23, 24, -1, 25, 54, 55, 30, 56, 33, -1, 34, 35, 36, 37, 57, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59 } },
				{ false, HebrewYearType.מלא,	DayOfWeek.Thursday, new [] { 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
																			 22, 23, 24, -1, 25, 54, 55, 30, 56, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 50 } },
				{ false, HebrewYearType.מלא,	DayOfWeek.Saturday, new [] { -1, 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
																			 53, 23, 24, -1, 25, 54, 55, 30, 56, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59 } },

				//		חסר Leap		———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
				{ true,  HebrewYearType.חסר,	DayOfWeek.Monday,   new [] { 51, 52, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
																			 24, 25, 26, 27, -1, 28, 29, 30, 31, 32, 33, -1, 34, 35, 36, 37, 57, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59 } },
				{ true,  HebrewYearType.חסר,	DayOfWeek.Thursday, new [] { 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
																			 24, 25, 26, 27, 28, -1, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50 } },
				{ true,  HebrewYearType.חסר,	DayOfWeek.Saturday, new [] { -1, 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
																			 23, 24, 25, 26, 27, -1, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59 } },

				//		מלא Leap		———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
				{ true,  HebrewYearType.מלא,	DayOfWeek.Monday,   new [] { 51, 52, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
																			 24, 25, 26, 27, -1, -1, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 58, 43, 44, 45, 46, 47, 48, 49, 50 } },
				{ true,  HebrewYearType.מלא,	DayOfWeek.Thursday, new [] { 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 
																			 24, 25, 26, 27, 28, -1, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 59 } },
				{ true,  HebrewYearType.מלא,	DayOfWeek.Saturday, new [] { -1, 52, -1, -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 
																			 23, 24, 25, 26, 27, -1, 28, 29, 30, 31, 32, 33, -1, 34, 35, 36, 37, 57, 40, 58, 43, 44, 45, 46, 47, 48, 49, 59 } }
			};
			d.Add(false, HebrewYearType.כסדרן, DayOfWeek.Tuesday, d[false, HebrewYearType.מלא, DayOfWeek.Monday]);
			d.Add(true, HebrewYearType.כסדרן, DayOfWeek.Tuesday, d[true, HebrewYearType.מלא, DayOfWeek.Monday]);
			return d;
		})();
		#endregion
	}
}
