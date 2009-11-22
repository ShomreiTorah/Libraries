using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Common.Calendar {
	public partial class HebrewDayInfo {
		static readonly DateTime CycleEnd = new DateTime(2005, 3, 1);

		///<summary>Gets the string for the date's דף.</summary>
		public string DafYomiString {
			get {
				var offset = (Date.EnglishDate - CycleEnd).Days;

				if (offset < 0)
					offset = ShasLength - (-offset % ShasLength);
				else
					offset %= ShasLength;

				int i = 0;
				while (offset > Masechtos[i].Value) {
					offset -= Masechtos[i].Value;
					i++;
					if (i == Masechtos.Count) i = 0;
				}
				return Masechtos[i].Key + " דף " + (offset + 1).ToHebrewString(HebrewNumberFormat.Letter);

			}
		}

		#region Lengths
		static readonly ReadOnlyCollection<KeyValuePair<string, int>> Masechtos = new ReadOnlyCollection<KeyValuePair<string, int>>(new KeyValuePair<string, int>[] {
			new KeyValuePair<string, int>("ברכות",		 63),
			new KeyValuePair<string, int>("שבת",		156),
			new KeyValuePair<string, int>("עירובין",		104),
			new KeyValuePair<string, int>("פסחים",		120),
			new KeyValuePair<string, int>("שקלים",		 21),
			new KeyValuePair<string, int>("יומא",		 87),
			new KeyValuePair<string, int>("סוכה",		 55),
			new KeyValuePair<string, int>("ביצה",		 39),
			new KeyValuePair<string, int>("ראש השנה",	 34),
			new KeyValuePair<string, int>("תענית",		 30),
			new KeyValuePair<string, int>("מגילה",		 31),
			new KeyValuePair<string, int>("מועד קטן",	 28),
			new KeyValuePair<string, int>("חגיגה",		 26),
			new KeyValuePair<string, int>("יבמות",		121),
			new KeyValuePair<string, int>("כתובות",		111),
			new KeyValuePair<string, int>("נדרים",		 90),
			new KeyValuePair<string, int>("נזיר",			 65),
			new KeyValuePair<string, int>("סוטה",		 48),
			new KeyValuePair<string, int>("גיטין",		 89),
			new KeyValuePair<string, int>("קידושין",		 81),
			new KeyValuePair<string, int>("בבא קמא",	118),
			new KeyValuePair<string, int>("בבא מציעא",	118),
			new KeyValuePair<string, int>("בבא בתרא",	175),
			new KeyValuePair<string, int>("סנהדרין",		112),
			new KeyValuePair<string, int>("מכות",		 23),
			new KeyValuePair<string, int>("שבועות",		 48),
			new KeyValuePair<string, int>("עבודה זרה",	 75),
			new KeyValuePair<string, int>("הוריות",		 13),
			new KeyValuePair<string, int>("זבחים",		119),
			new KeyValuePair<string, int>("מנחות",		109),
			new KeyValuePair<string, int>("חולין",		141),
			new KeyValuePair<string, int>("בכורות",		 60),
			new KeyValuePair<string, int>("ערכין",		 33),
			new KeyValuePair<string, int>("תמורה",		 33),
			new KeyValuePair<string, int>("כריתות",		 27),
			new KeyValuePair<string, int>("מעילה",		 36),
			new KeyValuePair<string, int>("נידה",		 72),
		});
		#endregion
		static readonly int ShasLength = Masechtos.Sum(kvp => kvp.Value);
	}
}
