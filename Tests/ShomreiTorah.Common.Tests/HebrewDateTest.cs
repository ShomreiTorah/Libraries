using ShomreiTorah.Common.Calendar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for HebrewDateTest and is intended
	///to contain all HebrewDateTest Unit Tests
	///</summary>
	[TestClass()]
	public class HebrewDateTest {
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


		/// <summary>Tests basic functionality.</summary>
		[TestMethod()]
		public void MainTest() {
			var start = new DateTime(1584, 1, 1);
			for (var d = start; d.Year < 2239; d += TimeSpan.FromDays(1)) {
				var hd = new HebrewDate(d);
				var hd2 = new HebrewDate(hd.HebrewYear, hd.HebrewMonth, hd.HebrewDay);
				Assert.AreEqual(hd, hd2);
				Assert.IsTrue(d == hd, d.ToLongDateString() + " == " + hd);
			}
		}
		/// <summary>
		///A test for Findפרשה
		///</summary>
		[TestMethod()]
		public void FindפרשהTest() {
			for (int hebrewYear = 5344; hebrewYear < 6000; hebrewYear++) {
				for (int parshaIndex = 0; parshaIndex < 53; parshaIndex++) {
					var expectedParsha = HebrewDate.Parshiyos[parshaIndex];
					var date = HebrewDate.Findפרשה(hebrewYear, parshaIndex);

					Assert.AreEqual(DayOfWeek.Saturday, date.DayOfWeek);					//We should get a שבת
					//Assert.AreEqual(hebrewYear, date.HebrewYear, date + " (" + expectedParsha + ")");			//We should get the year we asked for
					Assert.AreEqual(date, HebrewDate.Findפרשה(hebrewYear, expectedParsha));	//We should get the same date by index & by name

					var actualParsha = date.Parsha;
					Assert.IsTrue(actualParsha == expectedParsha
							   || actualParsha.StartsWith(expectedParsha + "־")
							   || actualParsha.EndsWith("־" + expectedParsha)
					);			  //We should get the פרשה that we asked for.
				}
			}
			Assert.Inconclusive("I must figure out how to handle years without וילך.");
		}
		///// <summary>
		/////A test for Parsha
		/////</summary>
		//[TestMethod()]
		//public void ParshaTest() {
		//    var start = new DateTime(1584, 1, 1);
		//    start += TimeSpan.FromDays(6 - (int)start.DayOfWeek);
		//    for (var d = start; d.Year < 2239; d += TimeSpan.FromDays(7)) {
		//        Assert.AreEqual(Parshiyos.Getפרשה(d), new HebrewDate(d).Parsha ?? "No פרשה");
		//    }
		//}

		/// <summary>
		///A test for GetYearType
		///</summary>
		[TestMethod()]
		public void GetYearTypeTest() {
			Assert.AreEqual(HebrewYearType.חסר, HebrewDate.GetYearType(5765));
			Assert.AreEqual(HebrewYearType.כסדרן, HebrewDate.GetYearType(5766));
			Assert.AreEqual(HebrewYearType.מלא, HebrewDate.GetYearType(5767));
			Assert.AreEqual(HebrewYearType.חסר, HebrewDate.GetYearType(5768));
			Assert.AreEqual(HebrewYearType.כסדרן, HebrewDate.GetYearType(5769));
			Assert.AreEqual(HebrewYearType.מלא, HebrewDate.GetYearType(5770));
			Assert.AreEqual(HebrewYearType.מלא, HebrewDate.GetYearType(5771));
		}
	}
}
