using ShomreiTorah.Common.Calendar.Zmanim;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for FixedZmanimProviderTest and is intended
	///to contain all FixedZmanimProviderTest Unit Tests
	///</summary>
	[TestClass()]
	public class FixedZmanimProviderTest {

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


		/// <summary>
		///A test for GetZmanim
		///</summary>
		[TestMethod()]
		public void GetZmanimTest() {
			for (int year = 2009; year < 2038; year++) {
				for (var date = new DateTime(year, 1, 1); date.Year == year; date = date.AddDays(1)) {
					var e = FastCsvZmanimProvider.Default.GetZmanim(date);
					var a = FixedZmanimProvider.Default.GetZmanim(date);

					Assert.AreEqual(e.Date, a.Date);
					Assert.AreEqual(e.Times.Count, a.Times.Count);
					foreach (var key in e.Times.Keys) {
						Assert.AreEqual(a[key], e[key]);
					}
				}
			}
		}
	}
}
