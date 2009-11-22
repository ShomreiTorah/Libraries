using ShomreiTorah.Common.Calendar.Zmanim;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for FastCsvZmanimProviderTest and is intended
	///to contain all FastCsvZmanimProviderTest Unit Tests
	///</summary>
	[TestClass()]
	public class FastCsvZmanimProviderTest {

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

		static FastCsvZmanimProvider CreateProvider() {
			var tempPath = Path.Combine(Path.GetTempPath(), @"ShomreiTorah\Tests\Zmanim\FastCSV");
			if (Directory.Exists(tempPath))
				Directory.Delete(tempPath, true);
			Directory.CreateDirectory(tempPath);
			return new FastCsvZmanimProvider(tempPath);
		}

		/// <summary>
		///A test for the Write(ZmanimProvider) method
		///</summary>
		[TestMethod()]
		public void WriteProviderTestTest() {
			//var min = XmlZmanimProvider.Default.MinDate;
			//if (min.DayOfYear > 1) min = new DateTime(min.Year + 1, 1, 1);
			//var max = XmlZmanimProvider.Default.MaxDate;
			//if (max.Month != 12 || max.Day != 31) max = new DateTime(max.Year - 1, 12, 31);

			//var xmlZmanim = XmlZmanimProvider.Default.GetZmanim(min, max).ToArray();
			DateTime min = new DateTime(2010, 1, 1), max = new DateTime(2010, 12, 31);

			var provider = CreateProvider();

			Assert.AreEqual(DateTime.MinValue, provider.MinDate);
			Assert.AreEqual(DateTime.MinValue, provider.MaxDate);
			Assert.IsNull(provider.GetZmanim(DateTime.Now));

			provider.Write(OUZmanimProvider.Default, min.Year, max.Year, true);

			Assert.AreEqual(min, provider.MinDate);
			Assert.AreEqual(max, provider.MaxDate);


			var loadedZmanim = provider.GetZmanim(min, max).ToArray();
			Assert.AreEqual(min, loadedZmanim.First().Date);
			Assert.AreEqual(max, loadedZmanim.Last().Date);

		}
		/// <summary>
		///A test for the entire class using Write(IEnumerable)
		///</summary>
		[TestMethod()]
		public void WriteEnumerableTestTest() {
			//var min = XmlZmanimProvider.Default.MinDate;
			//if (min.DayOfYear > 1) min = new DateTime(min.Year + 1, 1, 1);
			//var max = XmlZmanimProvider.Default.MaxDate;
			//if (max.Month != 12 || max.Day != 31) max = new DateTime(max.Year - 1, 12, 31);

			DateTime min = new DateTime(2009, 1, 1), max = new DateTime(2012, 12, 31);
			var xmlZmanim = XmlZmanimProvider.Default.GetZmanim(min, max).ToArray();

			var provider = CreateProvider();

			Assert.AreEqual(DateTime.MinValue, provider.MinDate);
			Assert.AreEqual(DateTime.MinValue, provider.MaxDate);
			Assert.IsNull(provider.GetZmanim(DateTime.Now));

			provider.Write(xmlZmanim, true);

			Assert.AreEqual(min, provider.MinDate);
			Assert.AreEqual(max, provider.MaxDate);


			var loadedZmanim = provider.GetZmanim(min, max).ToArray();
			Assert.AreEqual(xmlZmanim.Length, loadedZmanim.Length);
			for (int i = 0; i < loadedZmanim.Length; i++) {
				var e = xmlZmanim[i];
				var a = loadedZmanim[i];
				Assert.AreEqual(e.Date, a.Date);
				Assert.AreEqual(e.Times.Count, a.Times.Count);
				foreach (var key in e.Times.Keys) {
					Assert.AreEqual(a[key], e[key]);
				}
			}
		}
	}
}
