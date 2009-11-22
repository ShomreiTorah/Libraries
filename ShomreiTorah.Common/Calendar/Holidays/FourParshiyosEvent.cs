using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Common.Calendar.Holidays {
	struct FourParshiyosEvent : IHebrewEvent {
		static readonly Dictionary<DayOfWeek, HebrewDayOfYear[]> gaps = new Dictionary<DayOfWeek, HebrewDayOfYear[]> {
			{ DayOfWeek.Saturday,	new [] { new HebrewDayOfYear(HebrewMonth.אדר2, 15) } },
			{ DayOfWeek.Monday,		new [] { new HebrewDayOfYear(HebrewMonth.אדר2, 06) } },
			{ DayOfWeek.Wednesday,	new [] { new HebrewDayOfYear(HebrewMonth.אדר2, 04) } },
			{ DayOfWeek.Friday,		new [] { new HebrewDayOfYear(HebrewMonth.אדר2, 02), 
											 new HebrewDayOfYear(HebrewMonth.אדר2, 16) } }
		};

		public int Index { get; private set; }
		public FourParshiyosEvent(int index)
			: this() {
			if (index < 0 || index > 3) throw new ArgumentOutOfRangeException("index");
			Index = index;
		}

		public HebrewDate GetDate(int hebrewYear) {
			//Find ראש חודש
			var rc = new HebrewDate(hebrewYear, HebrewMonth.אדר2, 1);
			//Start at פרשת שקלים
			var retVal = rc.Last(DayOfWeek.Saturday);
			int weekIndex = 0;

			//Skip preceding ארבע פרשיות
			while (weekIndex < Index) {
				retVal += 7;

				//If the שבת we're up to isn't a gap,
				//move forward.
				if (!gaps[rc.DayOfWeek].Any(d => d.Is(retVal)))
					weekIndex++;
			}
			return retVal;
		}

		public bool Is(HebrewDate date) { return date == GetDate(date.HebrewYear); }

		public bool Equals(IHebrewEvent other) { return other is FourParshiyosEvent && ((FourParshiyosEvent)other).Index == Index; }
	}
}
