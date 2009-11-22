using ShomreiTorah.Common.Calendar.Holidays;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShomreiTorah.Common.Calendar;
namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for HebrewDayOfYearTest and is intended
	///to contain all HebrewDayOfYearTest Unit Tests
	///</summary>
	[TestClass()]
	public class HebrewDayOfYearTest {
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


		[TestMethod()]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void HebrewDayOfYearConstructorTest1() { new HebrewDayOfYear(HebrewMonth.None, 1); }
		[TestMethod()]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void HebrewDayOfYearConstructorTest2() { new HebrewDayOfYear(HebrewMonth.אב, -1); }
		[TestMethod()]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void HebrewDayOfYearConstructorTest3() { new HebrewDayOfYear(HebrewMonth.אלול, 30); }
		[TestMethod()]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void HebrewDayOfYearConstructorTest4() { new HebrewDayOfYear(HebrewMonth.אב, 31); }
		[TestMethod()]
		public void HebrewDayOfYearConstructorTest5() {
			new HebrewDayOfYear(HebrewMonth.חשון, 30);
			new HebrewDayOfYear(HebrewMonth.כסלו, 30);
		}

		/// <summary>
		///A test for GetDate
		///</summary>
		[TestMethod()]
		public void GetDateTest() {
			Assert.AreEqual(new HebrewDate(5678, HebrewMonth.טבת, 23), new HebrewDayOfYear(HebrewMonth.טבת, 23).GetDate(5678));
			Assert.AreEqual(new HebrewDate(5678, HebrewMonth.אדר1, 23), new HebrewDayOfYear(HebrewMonth.אדר1, 23).GetDate(5678));
			Assert.AreEqual(new HebrewDate(5678, HebrewMonth.אדר2, 23), new HebrewDayOfYear(HebrewMonth.אדר2, 23).GetDate(5678));

			Assert.AreEqual(new HebrewDate(5555, HebrewMonth.אדר1, 23), new HebrewDayOfYear(HebrewMonth.אדר1, 23).GetDate(5555));
			Assert.AreEqual(new HebrewDate(5555, HebrewMonth.אדר2, 23), new HebrewDayOfYear(HebrewMonth.אדר1, 23).GetDate(5555));
			Assert.AreEqual(new HebrewDate(5555, HebrewMonth.אדר1, 23), new HebrewDayOfYear(HebrewMonth.אדר2, 23).GetDate(5555));
			Assert.AreEqual(new HebrewDate(5555, HebrewMonth.אדר2, 23), new HebrewDayOfYear(HebrewMonth.אדר2, 23).GetDate(5555));

			Assert.AreEqual(new HebrewDate(5555, HebrewMonth.סיון, 23), new HebrewDayOfYear(HebrewMonth.סיון, 23).GetDate(5555));
		}

		/// <summary>
		///A test for op_Subtraction
		///</summary>
		[TestMethod()]
		public void op_SubtractionTest() {
			Assert.AreEqual(7, new HebrewDate(5678, HebrewMonth.תשרי, 28) - new HebrewDayOfYear(HebrewMonth.תשרי, 21));
			Assert.AreEqual(-7, new HebrewDate(5678, HebrewMonth.תשרי, 21) - new HebrewDayOfYear(HebrewMonth.תשרי, 28));
			Assert.AreEqual(37, new HebrewDate(5678, HebrewMonth.חשון, 28) - new HebrewDayOfYear(HebrewMonth.תשרי, 21));
		}

		[TestMethod()]
		public void ComparisonsTest() {
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.אדר1, 4) < new HebrewDayOfYear(HebrewMonth.אדר2, 23));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.כסלו, 4) < new HebrewDayOfYear(HebrewMonth.כסלו, 23));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.כסלו, 4) > new HebrewDayOfYear(HebrewMonth.תשרי, 23));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.כסלו, 4) >= new HebrewDayOfYear(HebrewMonth.תשרי, 23));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.כסלו, 26) >= new HebrewDayOfYear(HebrewMonth.כסלו, 23));

			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.כסלו, 26) != new HebrewDayOfYear(HebrewMonth.כסלו, 23));

			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.תשרי, 4) >= new HebrewDayOfYear(HebrewMonth.תשרי, 4));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.תשרי, 4) <= new HebrewDayOfYear(HebrewMonth.תשרי, 4));
			Assert.IsTrue(new HebrewDayOfYear(HebrewMonth.תשרי, 4) == new HebrewDayOfYear(HebrewMonth.תשרי, 4));
		}

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod()]
		public void ToStringTest() {
			Assert.AreEqual("ז' ניסן", new HebrewDayOfYear(HebrewMonth.ניסן, 7).ToString());
			Assert.AreEqual("כ\"ג אדר א", new HebrewDayOfYear(HebrewMonth.אדר1, 23).ToString());
			Assert.AreEqual("ט\"ז אדר ב", new HebrewDayOfYear(HebrewMonth.אדר2, 16).ToString());
		}
	}
}
