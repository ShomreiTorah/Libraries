using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>A ZmanimProvider that calculates Zmanim for a latitude and longitude.</summary>
	public class CalculatingZmanimProvider : YearlyZmanimProvider {
		///<summary>Creates a new CalculatingZmanimProvider instance for the location specified in ShomreiTorahConfig.</summary>
		private CalculatingZmanimProvider()
			: this(double.Parse(Config.ReadAttribute("Zmanim", "Latitude"), CultureInfo.InvariantCulture),
				   double.Parse(Config.ReadAttribute("Zmanim", "Longitude"), CultureInfo.InvariantCulture)) {
		}
		///<summary>Creates a CalculatingZmanimProvider for a given location.</summary>
		public CalculatingZmanimProvider(double latitude, double longitude) { Latitude = latitude; Longitude = longitude; }
		[ThreadStatic]
		static CalculatingZmanimProvider defaultInstance;
		///<summary>Gets the CalculatingZmanimProvider instance for the location specified in ShomreiTorahConfig.</summary>
		///<remarks>Since CalculatingZmanimProvider is not thread-safe, this will create a different instance for each thread.</remarks>
		public static new CalculatingZmanimProvider Default {
			get {
				if (defaultInstance == null) defaultInstance = new CalculatingZmanimProvider(); //Since it's ThreadStatic, I don't need to lock
				return defaultInstance;
			}
		}

		///<summary>Gets the latitude that the Zmanim are calculated for.</summary>
		public double Latitude { get; private set; }
		///<summary>Gets the longitude that the Zmanim are calculated for.</summary>
		public double Longitude { get; private set; }


		///<summary>Creates a YearlyZmanimDictionary for the given year.</summary>
		///<returns>The Zmanim for that year, or null if they could not be loaded.</returns>
		protected override YearlyZmanimDictionary CreateYear(int year) {
			var days = new ZmanimInfo[DateTime.IsLeapYear(year) ? 366 : 365];

			for (int i = 0; i < days.Length; i++) {
				var date = new DateTime(year, 1, 1).AddDays(i);
				days[i] = new ZmanimInfo(date, CalculateZmanim(date));
			}
			return new YearlyZmanimDictionary(year, days);
		}

		Dictionary<Zman, TimeSpan> CalculateZmanim(DateTime date) {
			var retVal = new Dictionary<Zman, TimeSpan>(16);

			retVal.Add(Zman.Sunrise, CalcSunTime(date, 545 / 6.0, true));
			retVal.Add(Zman.Sunset, CalcSunTime(date, 545 / 6.0, false));

			var hour1 = (retVal[Zman.Sunset] - retVal[Zman.Sunrise]).Ticks / 12;
			var hour2 = (TimeSpan.FromHours(2.4) + retVal[Zman.Sunset] - retVal[Zman.Sunrise]).Ticks / 12;

			retVal.Add(Zman.עלות, retVal[Zman.Sunrise] - TimeSpan.FromHours(1.2));

			retVal.Add(Zman.סוף٠זמן٠קריאת٠שמע٠גרא, retVal[Zman.Sunrise] + TimeSpan.FromTicks(hour1 * 3));
			retVal.Add(Zman.סוף٠זמן٠קריאת٠שמע٠מ٠א, retVal[Zman.עלות] + TimeSpan.FromTicks(hour2 * 3));

			retVal.Add(Zman.חצות, retVal[Zman.Sunrise] + TimeSpan.FromTicks(hour1 * 6));

			//TODO: Other זמנים

			return retVal;
		}

		TimeSpan CalcSunTime(DateTime date, double zenith, bool sunrise) {
			zenith = Cos(zenith);
			var N1 = Math.Floor(275 * date.Month / 9.0);
			var N2 = Math.Floor((date.Month + 9) / 12.0);
			var N3 = (1 + Math.Floor((date.Year - 4 * Math.Floor(date.Year / 4.0) + 2) / 3.0));
			var N = N1 - (N2 * N3) + date.Day - 30;
			var lngHour = Longitude / 15;

			double t;
			if (sunrise)
				t = N + ((6 - lngHour) / 24);
			else
				t = N + ((18 - lngHour) / 24);

			var M = (.9856 * t) - 3.289;
			var L = M + (1.916 * Sin(M)) + (.020 * Sin(2 * M)) + 282.634;
			if (sunrise) {
				L -= 360;
			}
			var RA = Atan(.91746 * Tan(L));
			var lQuadrant = (Math.Floor(L / 90.0)) * 90;
			var RAQuadrant = (Math.Floor(RA / 90.0)) * 90;
			RA += (lQuadrant - RAQuadrant);
			RA /= 15.0;
			var sinDec = .39782 * Sin(L);
			var cosDec = Cos(Asin(sinDec));
			var cosH = (zenith - (sinDec * Sin(Latitude))) / (cosDec * Cos(Latitude));
			var H = Acos(cosH);

			if (sunrise)
				H = 360 - H;
			H /= 15.0;

			var T = H + RA - (.06571 * t) - 6.622;
			var UT = T - lngHour;

			//Get the offset after the DST change.
			//Otherwise, the Sunday that the clock
			//changes gets pre-change values, since
			//the UtcOffset only changes at 2:00 AM

			var utcValue = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc).AddHours(UT);
			var timeZone = (string)Config.GetElement("Zmanim").Attribute("TimeZone") ?? TimeZoneInfo.Local.Id;

			return TimeZoneInfo.ConvertTimeFromUtc(utcValue, TimeZoneInfo.FindSystemTimeZoneById(timeZone)).TimeOfDay;
		}

		static double Sin(double degrees) { return Math.Sin(degrees * Math.PI / 180); }
		static double Cos(double degrees) { return Math.Cos(degrees * Math.PI / 180); }
		static double Tan(double degrees) { return Math.Tan(degrees * Math.PI / 180); }

		static double Asin(double number) { return Math.Asin(number) / (Math.PI / 180); }
		static double Acos(double number) { return Math.Acos(number) / (Math.PI / 180); }
		static double Atan(double number) { return Math.Atan(number) / (Math.PI / 180); }

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public override DateTime MinDate { get { return DateTime.MinValue; } }

		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public override DateTime MaxDate { get { return DateTime.MaxValue; } }
	}
}
