using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common.Calendar.Holidays {
	///<summary>Describes a יום טוב.</summary>
	public sealed class Holiday {
		#region ימים טובים
		///<summary>The days of ראש השנה.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly HolidayGroup ראש٠השנה = new HolidayGroup(new[]{
				new Holiday("א' ראש השנה",		HebrewMonth.תשרי,  1, HolidayCategory.דאריתא),
				new Holiday("ב' ראש השנה",		HebrewMonth.תשרי,  2, HolidayCategory.דאריתא)
		});

		///<summary>The יום טוב of יום כיפור.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Holiday is immutable")]
		public static readonly Holiday יום٠כיפור = new Holiday("יום כיפור", HebrewMonth.תשרי, 10, HolidayCategory.דאריתא);

		///<summary>The days of חנוכה.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly HolidayGroup חנוכה = new HolidayGroup(Enumerable.Range(1, 8).Select(d =>
			new Holiday(d.ToHebrewString(HebrewNumberFormat.LetterQuoted) + " חנוכה", new OffsetEvent(new HebrewDayOfYear(HebrewMonth.כסלו, 25), d - 1), HolidayCategory.דרבנן)
		));

		///<summary>The day of פורים.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Holiday is immutable")]
		public static readonly Holiday פורים = new Holiday("פורים", HebrewMonth.אדר2, 14, HolidayCategory.דרבנן);


		///<summary>The days of סוכות.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly HolidayGroup סוכות = new HolidayGroup(new[]{
				new Holiday("א' סוכות",			HebrewMonth.תשרי,	15, HolidayCategory.דאריתא),
				new Holiday("ב' סוכות",			HebrewMonth.תשרי,	16, HolidayCategory.דאריתא),
				new Holiday("א' חוה\"מ סוכות",	HebrewMonth.תשרי,	17, HolidayCategory.חולהמועד),
				new Holiday("ב' חוה\"מ סוכות",	HebrewMonth.תשרי,	18, HolidayCategory.חולהמועד),
				new Holiday("ג' חוה\"מ סוכות",	HebrewMonth.תשרי,	19, HolidayCategory.חולהמועד),
				new Holiday("ד' חוה\"מ סוכות",	HebrewMonth.תשרי,	20, HolidayCategory.חולהמועד),
				new Holiday("הושענא רבה",		HebrewMonth.תשרי,	21, HolidayCategory.חולהמועד),
				new Holiday("שמיני עצרת",		HebrewMonth.תשרי,	22, HolidayCategory.דאריתא),
				new Holiday("שמחת תורה",		HebrewMonth.תשרי,	23, HolidayCategory.דאריתא)
		});

		///<summary>The days of פסח.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly HolidayGroup פסח = new HolidayGroup(new[]{
				new Holiday("א' פסח",			HebrewMonth.ניסן,	15, HolidayCategory.דאריתא),
				new Holiday("ב' פסח",			HebrewMonth.ניסן,	16, HolidayCategory.דאריתא),
				new Holiday("א' חוה\"מ פסח",	HebrewMonth.ניסן,	17, HolidayCategory.חולהמועד),
				new Holiday("ב' חוה\"מ פסח",	HebrewMonth.ניסן,	18, HolidayCategory.חולהמועד),
				new Holiday("ג' חוה\"מ פסח",		HebrewMonth.ניסן,	19, HolidayCategory.חולהמועד),
				new Holiday("ד' חוה\"מ פסח",	HebrewMonth.ניסן,	20, HolidayCategory.חולהמועד),
				new Holiday("שביעי של פסח",		HebrewMonth.ניסן,	21, HolidayCategory.דאריתא),
				new Holiday("אחרון של פסח",		HebrewMonth.ניסן,	22, HolidayCategory.דאריתא)
		});

		///<summary>The days of שבועות.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly HolidayGroup שבועות = new HolidayGroup(new[]{
				new Holiday("א' שבועות",		HebrewMonth.סיון,		 6, HolidayCategory.דאריתא),
				new Holiday("ב' שבועות",		HebrewMonth.סיון,		 7, HolidayCategory.דאריתא),
		});

		///<summary>The תענית of תשעה באב.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayGroup is immutable")]
		public static readonly Holiday תשעה٠באב = new Holiday("תשעה באב", new NonשבתEvent(HebrewMonth.אב, 9, DayOfWeek.Sunday), HolidayCategory.תענית);
		#endregion

		class Holidays : HolidayCollection {
			static Holiday[] values = new[] { 
				#region ימים טובים
				ראש٠השנה[1], ראש٠השנה[2],

				new Holiday("צום גדליה",		new NonשבתEvent	(HebrewMonth.תשרי,	3, DayOfWeek.Sunday),	HolidayCategory.תענית),
				new Holiday("שבת שובה",		new SpecialשבתEvent(HebrewMonth.תשרי,	3),					HolidayCategory.Specialשבת),

				יום٠כיפור,

				סוכות[1], סוכות[2], סוכות[3], סוכות[4], סוכות[5], סוכות[6], סוכות[7], סוכות[8], סוכות[9],

				חנוכה[1], חנוכה[2], חנוכה[3], חנוכה[4], חנוכה[5], חנוכה[6], חנוכה[7], חנוכה[8],

				new Holiday("עשרה בטבת",		HebrewMonth.טבת,	10, HolidayCategory.תענית),	//עשרה בטבת cannot fall out on שבת, and can be fasted on Friday
				new Holiday("ט\"ו בשבט",		HebrewMonth.שבט,	15, HolidayCategory.Minor),

				new Holiday("פרשת שקלים",		new FourParshiyosEvent(0),	HolidayCategory.Fourפרשיות),
				new Holiday("פרשת זכור",		new FourParshiyosEvent(1),	HolidayCategory.Fourפרשיות),
				new Holiday("פרשת פרה",		new FourParshiyosEvent(2),	HolidayCategory.Fourפרשיות),
				new Holiday("פרשת החודש",		new FourParshiyosEvent(3),	HolidayCategory.Fourפרשיות),

				new Holiday("פורים קטן",		HebrewMonth.אדר1,	14, HolidayCategory.Minor),
				new Holiday("שושן פורים קטן",	HebrewMonth.אדר1,	15, HolidayCategory.Minor),
				new Holiday("תענית אסתר",		new NonשבתEvent	(HebrewMonth.אדר2,	13, DayOfWeek.Thursday), HolidayCategory.תענית),
				פורים,
				new Holiday("שושן פורים",		HebrewMonth.אדר2,	15, HolidayCategory.Minor),

				new Holiday("שבת הגדול",		new SpecialשבתEvent(HebrewMonth.ניסן,	8),						 HolidayCategory.Specialשבת),
				new Holiday("תענית בכורות",	new NonשבתEvent	(HebrewMonth.ניסן,	14, DayOfWeek.Thursday), HolidayCategory.Minor),

				פסח[1], פסח[2], פסח[3], פסח[4], פסח[5], פסח[6], פסח[7], פסח[8],

				new Holiday("פסח שני",			HebrewMonth.אייר,	14, HolidayCategory.Minor),
				new Holiday("ל\"ג בעומר",		HebrewMonth.אייר,	18, HolidayCategory.Minor),

				שבועות[1], שבועות[2],

				new Holiday("י\"ז בתמוז",		new NonשבתEvent	(HebrewMonth.תמוז,	17, DayOfWeek.Sunday),	HolidayCategory.תענית),
				new Holiday("שבת חזון",		new SpecialשבתEvent(HebrewMonth.אב,	 2),					HolidayCategory.Specialשבת),
				תשעה٠באב,
				new Holiday("שבת נחמו",		new SpecialשבתEvent(HebrewMonth.אב,	 10),					HolidayCategory.Specialשבת),

				//Can conflict with שבת נחמו
				//new Holiday("ט\"ו באב",		HebrewMonth.אב,		15, HolidayCategory.Minor),
				#endregion
			};

			public Holidays() : base(new KeyedHolidayCollection()) { }
			private class KeyedHolidayCollection : KeyedCollection<string, Holiday> {
				public KeyedHolidayCollection() { Array.ForEach(values, Add); }
				protected override string GetKeyForItem(Holiday item) {
					if (item == null) throw new ArgumentNullException("item");
					return item.Name;
				}
			}
		}
		///<summary>Gets all of the ימים טובים.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "HolidayCollection is immutable")]
		public static readonly HolidayCollection All = new Holidays();

		private Holiday(string name, HebrewMonth month, int day, HolidayCategory category) : this(name, new HebrewDayOfYear(month, day), category) { }
		private Holiday(string name, IHebrewEvent date, HolidayCategory category) {
			Name = name;
			Date = date;
			Category = category;
		}

		///<summary>Gets the name of this יום טוב.</summary>
		public string Name { get; private set; }
		///<summary>Gets a HebrewEvent that describes this יום טוב.</summary>
		public IHebrewEvent Date { get; private set; }
		///<summary>Gets the category of this יום טוב.</summary>
		public HolidayCategory Category { get; private set; }

		///<summary>Returns the name of this יום טוב.</summary>
		///<returns>The name of this יום טוב.</returns>
		public override string ToString() { return Name; }
	}
	///<summary>A collection of Holiday objects.</summary>
	public abstract class HolidayCollection : ReadOnlyCollection<Holiday> {
		KeyedCollection<string, Holiday> list;

		internal HolidayCollection(KeyedCollection<string, Holiday> values) : base(values) { list = values; }

		///<summary>Gets the holiday with the given name.</summary>
		///<param name="name">The name of the יום טוב to get.  (eg, "ד' חוה\"מ סוכות")</param>
		///<returns>A Holiday object.</returns>
		public Holiday this[string name] { get { return list[name]; } }
	}
	///<summary>Contains a group of Holiday objects, such as ראש השנה or חנוכה.</summary>
	public class HolidayGroup {
		Holiday[] holidays;

		internal HolidayGroup(IEnumerable<Holiday> days) : this(days.ToArray()) { }
		internal HolidayGroup(Holiday[] days) { holidays = days; Days = new ReadOnlyCollection<Holiday>(holidays); }

		///<summary>Gets the number of days in this יומ טוב.</summary>
		public int Length { get { return holidays.Length; } }

		///<summary>Gets the Holiday instance for a specific day in this group.</summary>
		///<param name="day">The day to get (between 1 and Length).</param>
		public Holiday this[int day] {
			get {
				if (day < 1 || day > Length) throw new ArgumentOutOfRangeException("day");
				return holidays[day - 1];
			}
		}
		///<summary>Gets the days on this יום טוב.</summary>
		public ReadOnlyCollection<Holiday> Days { get; private set; }
	}

	///<summary>A type of יום טוב.</summary>
	public enum HolidayCategory {
		///<summary>Not a יום טוב.</summary>
		None,
		///<summary>A full יום טוב. (Specifically, ראש השנה, יום כיפור, סוכות, פסח, שבועות)</summary>
		דאריתא,
		///<summary>חול המועד.</summary>
		חולהמועד,
		///<summary>A יום טוב דרבנן. (Specifically, חנוכה and פורים)</summary>
		דרבנן,
		///<summary>A fast day.</summary>
		תענית,
		///<summary>A minor יום טוב. (Specifically, ט״ו בשבט, פורים קטן, שושן פורים, ל״ג בעומר, פסח שני)</summary>
		Minor,
		///<summary>A special שבת. (Specifically, שבת שובה, שבת הגדול, שבת חזון, שבת נחמו)</summary>
		Specialשבת,
		///<summary>The ארבע פרשיות.  (Specifically, פרשת שקלים, פרשת זכור, פרשת פרה, פרשת החודש)</summary>
		Fourפרשיות
	}
}
