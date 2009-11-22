using ShomreiTorah.Common.Calendar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for CalendarExtensionsTest and is intended
	///to contain all CalendarExtensionsTest Unit Tests
	///</summary>
	[TestClass]
	public class CalendarExtensionsTest {
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

		//5768 is a leap year

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod]
		public void ToStringTest() {
			Assert.AreEqual("ניסן", HebrewMonth.ניסן.ToString(0));
			Assert.AreEqual("תשרי", HebrewMonth.תשרי.ToString(9999));

			Assert.AreEqual("אדר", HebrewMonth.אדר1.ToString(5555));
			Assert.AreEqual("אדר", HebrewMonth.אדר2.ToString(5555));

			Assert.AreEqual("אדר א", HebrewMonth.אדר1.ToString(5768));
			Assert.AreEqual("אדר ב", HebrewMonth.אדר2.ToString(5768));
		}

		/// <summary>
		///A test for ToHebrewMonth
		///</summary>
		[TestMethod()]
		public void ToHebrewMonthTest() {
			Assert.AreEqual(HebrewMonth.תשרי, CalendarExtensions.ToHebrewMonth(5555, 1));
			Assert.AreEqual(HebrewMonth.תשרי, CalendarExtensions.ToHebrewMonth(5768, 1));

			Assert.AreEqual(HebrewMonth.אלול, CalendarExtensions.ToHebrewMonth(5555, 12));
			Assert.AreEqual(HebrewMonth.אלול, CalendarExtensions.ToHebrewMonth(5768, 13));

			Assert.AreEqual(HebrewMonth.שבט, CalendarExtensions.ToHebrewMonth(5555, 5));
			Assert.AreEqual(HebrewMonth.שבט, CalendarExtensions.ToHebrewMonth(5768, 5));

			Assert.AreEqual(HebrewMonth.ניסן, CalendarExtensions.ToHebrewMonth(5555, 7));
			Assert.AreEqual(HebrewMonth.ניסן, CalendarExtensions.ToHebrewMonth(5768, 8));

			Assert.AreEqual(HebrewMonth.אדר2, CalendarExtensions.ToHebrewMonth(5555, 6));
			Assert.AreEqual(HebrewMonth.אדר2, CalendarExtensions.ToHebrewMonth(5555, 6));

			Assert.AreEqual(HebrewMonth.אדר1, CalendarExtensions.ToHebrewMonth(5768, 6));
			Assert.AreEqual(HebrewMonth.אדר2, CalendarExtensions.ToHebrewMonth(5768, 7));
		}

		/// <summary>
		///A test for Length
		///</summary>
		[TestMethod()]
		public void LengthTest() {
			Assert.AreEqual(30, HebrewMonth.תשרי.Length(5555));
			Assert.AreEqual(30, HebrewMonth.תשרי.Length(5768));

			Assert.AreEqual(29, HebrewMonth.טבת.Length(5555));
			Assert.AreEqual(29, HebrewMonth.טבת.Length(5768));

			//אדר א have 30 days; אדר ב & אדר סתם has 29
			//אדר1 for non-leap should be אדר סתם
			Assert.AreEqual(29, HebrewMonth.אדר2.Length(5555));
			Assert.AreEqual(29, HebrewMonth.אדר1.Length(5555));
			Assert.AreEqual(30, HebrewMonth.אדר1.Length(5768));
			Assert.AreEqual(29, HebrewMonth.אדר2.Length(5768));

			Assert.AreEqual(30, HebrewMonth.אב.Length(5555));
			Assert.AreEqual(30, HebrewMonth.אב.Length(5768));

			Assert.AreEqual(29, HebrewMonth.אלול.Length(5555));
			Assert.AreEqual(29, HebrewMonth.אלול.Length(5768));
		}
		/// <summary>
		///A test for Index
		///</summary>
		[TestMethod()]
		public void IndexTest() {
			Assert.AreEqual(1, HebrewMonth.תשרי.Index(5555));
			Assert.AreEqual(1, HebrewMonth.תשרי.Index(5768));

			Assert.AreEqual(12, HebrewMonth.אלול.Index(5555));
			Assert.AreEqual(13, HebrewMonth.אלול.Index(5768));

			Assert.AreEqual(5, HebrewMonth.שבט.Index(5555));
			Assert.AreEqual(5, HebrewMonth.שבט.Index(5768));

			Assert.AreEqual(7, HebrewMonth.ניסן.Index(5555));
			Assert.AreEqual(8, HebrewMonth.ניסן.Index(5768));

			Assert.AreEqual(6, HebrewMonth.אדר1.Index(5555));
			Assert.AreEqual(6, HebrewMonth.אדר2.Index(5555));

			Assert.AreEqual(6, HebrewMonth.אדר1.Index(5768));
			Assert.AreEqual(7, HebrewMonth.אדר2.Index(5768));
		}

		/// <summary>
		///A test for Next
		///</summary>
		[TestMethod()]
		public void NextTest() {
			Assert.AreEqual(new DateTime(2009, 4, 11), new DateTime(2009, 4, 11).Next(DayOfWeek.Saturday));
			Assert.AreEqual(new DateTime(2009, 4, 18), new DateTime(2009, 4, 15).Next(DayOfWeek.Saturday));

			Assert.AreEqual(new DateTime(2009, 4, 15), new DateTime(2009, 4, 15).Next(DayOfWeek.Wednesday));
			Assert.AreEqual(new DateTime(2009, 4, 15), new DateTime(2009, 4, 10).Next(DayOfWeek.Wednesday));
			Assert.AreEqual(new DateTime(2009, 4, 15), new DateTime(2009, 4, 14).Next(DayOfWeek.Wednesday));

		}

		/// <summary>
		///A test for Last
		///</summary>
		[TestMethod()]
		public void LastTest() {
			Assert.AreEqual(new DateTime(2009, 4, 11), new DateTime(2009, 4, 11).Last(DayOfWeek.Saturday));
			Assert.AreEqual(new DateTime(2009, 4, 11), new DateTime(2009, 4, 14).Last(DayOfWeek.Saturday));

			Assert.AreEqual(new DateTime(2009, 4, 8), new DateTime(2009, 4, 8).Last(DayOfWeek.Wednesday));
			Assert.AreEqual(new DateTime(2009, 4, 8), new DateTime(2009, 4, 10).Last(DayOfWeek.Wednesday));
			Assert.AreEqual(new DateTime(2009, 4, 8), new DateTime(2009, 4, 14).Last(DayOfWeek.Wednesday));
		}
	}
}
