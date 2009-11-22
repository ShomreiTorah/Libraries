using ShomreiTorah.Common.Calendar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common.Calendar.Holidays;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for HebrewDayInfoTest and is intended
	///to contain all HebrewDayInfoTest Unit Tests
	///</summary>
	[TestClass()]
	public class HebrewDayInfoTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		//http://www.ou.org/holidays/calendar

		/// <summary>
		///A test for ראשחודשName
		///</summary>
		[TestMethod()]
		public void ראשחודשNameTest() {
			Assert.IsNull(new HebrewDate(5769, HebrewMonth.ניסן, 29).Info.ראשחודשMonth);
			Assert.AreEqual("אייר", new HebrewDate(5769, HebrewMonth.ניסן, 30).Info.ראשחודשMonth);
			Assert.AreEqual("אייר", new HebrewDate(5769, HebrewMonth.אייר, 1).Info.ראשחודשMonth);
			Assert.AreEqual("אדר", new HebrewDate(5769, HebrewMonth.אדר2, 1).Info.ראשחודשMonth);
			Assert.IsNull(new HebrewDate(5769, HebrewMonth.אייר, 2).Info.ראשחודשMonth);
		}

		#region ימים טובים
		/// <summary>
		///A test for Holiday
		///</summary>
		[TestMethod()]
		public void HolidayTest() {
			Assert.AreEqual("ב' חוה\"מ פסח", new HebrewDate(5768, HebrewMonth.ניסן, 18).Info.Holiday.Name);
			Assert.AreEqual("ב' חוה\"מ פסח", new HebrewDate(5555, HebrewMonth.ניסן, 18).Info.Holiday.Name);

			Assert.AreEqual("תענית אסתר", new HebrewDate(5767, HebrewMonth.אדר2, 11).Info.Holiday.Name);
			Assert.AreEqual("פורים", new HebrewDate(5767, HebrewMonth.אדר2, 14).Info.Holiday.Name);
			Assert.AreEqual("שושן פורים", new HebrewDate(5767, HebrewMonth.אדר2, 15).Info.Holiday.Name);

			Assert.AreEqual("פורים קטן", new HebrewDate(5768, HebrewMonth.אדר1, 14).Info.Holiday.Name);
			Assert.AreEqual("שושן פורים קטן", new HebrewDate(5768, HebrewMonth.אדר1, 15).Info.Holiday.Name);

			Assert.AreEqual("תענית אסתר", new HebrewDate(5768, HebrewMonth.אדר2, 13).Info.Holiday.Name);
			Assert.AreEqual("פורים", new HebrewDate(5768, HebrewMonth.אדר2, 14).Info.Holiday.Name);
			Assert.AreEqual("שושן פורים", new HebrewDate(5768, HebrewMonth.אדר2, 15).Info.Holiday.Name);

			Assert.AreEqual("תענית אסתר", new HebrewDate(5769, HebrewMonth.אדר2, 13).Info.Holiday.Name);
			Assert.AreEqual("פורים", new HebrewDate(5769, HebrewMonth.אדר1, 14).Info.Holiday.Name);
			Assert.AreEqual("פורים", new HebrewDate(5769, HebrewMonth.אדר2, 14).Info.Holiday.Name);
			Assert.AreEqual("שושן פורים", new HebrewDate(5769, HebrewMonth.אדר2, 15).Info.Holiday.Name);


			//ערב פסח in 5768 is שבת
			Assert.AreEqual("תענית בכורות", new HebrewDate(5768, HebrewMonth.ניסן, 12).Info.Holiday.Name);
			Assert.AreEqual("תענית בכורות", new HebrewDate(5769, HebrewMonth.ניסן, 14).Info.Holiday.Name);

			Assert.AreEqual("שבת הגדול", new HebrewDate(5768, HebrewMonth.ניסן, 14).Info.Holiday.Name);
			Assert.AreEqual("שבת הגדול", new HebrewDate(5769, HebrewMonth.ניסן, 10).Info.Holiday.Name);

			Assert.AreEqual("א' חנוכה", new HebrewDate(5768, HebrewMonth.כסלו, 25).Info.Holiday.Name);
			Assert.AreEqual("ב' חנוכה", new HebrewDate(5768, HebrewMonth.כסלו, 26).Info.Holiday.Name);
			Assert.AreEqual("ה' חנוכה", new HebrewDate(5768, HebrewMonth.כסלו, 29).Info.Holiday.Name);
			Assert.AreEqual("ו' חנוכה", new HebrewDate(5768, HebrewMonth.טבת, 1).Info.Holiday.Name);
			Assert.AreEqual("ח' חנוכה", new HebrewDate(5768, HebrewMonth.טבת, 3).Info.Holiday.Name);

			Assert.AreEqual("א' חנוכה", new HebrewDate(5767, HebrewMonth.כסלו, 25).Info.Holiday.Name);
			Assert.AreEqual("ב' חנוכה", new HebrewDate(5767, HebrewMonth.כסלו, 26).Info.Holiday.Name);
			Assert.AreEqual("ה' חנוכה", new HebrewDate(5767, HebrewMonth.כסלו, 29).Info.Holiday.Name);
			Assert.AreEqual("ו' חנוכה", new HebrewDate(5767, HebrewMonth.כסלו, 30).Info.Holiday.Name);
			Assert.AreEqual("ז' חנוכה", new HebrewDate(5767, HebrewMonth.טבת, 1).Info.Holiday.Name);
			Assert.AreEqual("ח' חנוכה", new HebrewDate(5767, HebrewMonth.טבת, 2).Info.Holiday.Name);

			Assert.AreEqual("שבת שובה", new HebrewDate(5768, HebrewMonth.תשרי, 3).Info.Holiday.Name);
			Assert.AreEqual("צום גדליה", new HebrewDate(5768, HebrewMonth.תשרי, 4).Info.Holiday.Name);
			Assert.AreEqual("יום כיפור", new HebrewDate(5768, HebrewMonth.תשרי, 10).Info.Holiday.Name);

			Assert.IsNull(new HebrewDate(5555, HebrewMonth.חשון, 23).Info.Holiday);
		}
		[TestMethod]
		public void BulkHolidayTest() {
			for (int hebrewYear = 5344; hebrewYear < 6000; hebrewYear++) {
				foreach (var holiday in Holiday.All) {
					if (!HebrewDate.IsHebrewLeapYear(hebrewYear) && holiday.Name.EndsWith("פורים קטן"))	//GetDate for פורים קטן returns regular פורים for non-leap years because of אדר equivalency
						continue;

					var date = holiday.Date.GetDate(hebrewYear);
					Assert.AreEqual(holiday, date.Info.Holiday);
					Assert.IsTrue(holiday.Date.Is(date));
				}
			}
		}
		#endregion

		/// <summary>
		///A test for OmerTextנקוד
		///</summary>
		[TestMethod()]
		public void OmerTextנקודTest() {
			Assert.IsNull(new HebrewDate(5555, HebrewMonth.ניסן, 1).Info.OmerTextנקוד);

			for (var date = new HebrewDate(5769, HebrewMonth.ניסן, 17); date.Info.OmerDay != -1; date++) {
				Assert.AreEqual(date.Info.OmerText, date.Info.OmerTextנקוד.Stripנקודות());
			}
		}

		/// <summary>
		///A test for OmerText
		///</summary>
		[TestMethod()]
		public void OmerTextTest() {
			Assert.IsNull(new HebrewDate(5555, HebrewMonth.ניסן, 1).Info.OmerText);

			Assert.AreEqual("היום יום אחד לעומר", Holiday.פסח[2].Date.GetDate(5769).Info.OmerText);
			Assert.AreEqual("היום שלשה ושלשים יום שהם ארבעה שבועות וחמשה ימים לעומר", Holiday.All["ל\"ג בעומר"].Date.GetDate(5769).Info.OmerText);
			Assert.AreEqual("היום תשעה ושלשים יום שהם חמשה שבועות וארבעה ימים לעומר", new HebrewDate(5769, HebrewMonth.אייר, 24).Info.OmerText);

		}

		/// <summary>
		///A test for OmerDay
		///</summary>
		[TestMethod()]
		public void OmerDayTest() {
			Assert.AreEqual(-1, new HebrewDate(5769, HebrewMonth.חשון, 13).Info.OmerDay);
			Assert.AreEqual(-1, new HebrewDate(5769, HebrewMonth.ניסן, 15).Info.OmerDay);
			Assert.AreEqual(-1, new HebrewDate(5769, HebrewMonth.סיון, 6).Info.OmerDay);

			Assert.AreEqual(1, new HebrewDate(5769, HebrewMonth.ניסן, 16).Info.OmerDay);
			//ל"ג בעומר is the daay before the night which we count 33
			Assert.AreEqual(33, Holiday.All["ל\"ג בעומר"].Date.GetDate(5769).Info.OmerDay);
			Assert.AreEqual(49, new HebrewDate(5769, HebrewMonth.סיון, 5).Info.OmerDay);
		}
	}
}
