using ShomreiTorah.Common.Calendar.Zmanim;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for YearlyZmanimDictionaryTest and is intended
	///to contain all YearlyZmanimDictionaryTest Unit Tests
	///</summary>
	[TestClass()]
	public class YearlyZmanimDictionaryTest {

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

		static readonly Random rand = new Random();
		static ZmanimInfo[] CreateZmanim(int year) {
			return Enumerable.Range(0, DateTime.IsLeapYear(year) ? 366 : 365)
				.Select(d => new ZmanimInfo(new DateTime(year, 1, 1).AddDays(d),
					new Dictionary<string, TimeSpan> {
						{ "Sunset",				TimeSpan.FromHours(rand.NextDouble() * 12) },
						{ "סוףזמןשמע_גרא",		TimeSpan.FromHours(rand.NextDouble() * 12) },
						{ "סוףזמןשמע_מגןאברהם",	TimeSpan.FromHours(rand.NextDouble() * 12) },
					}
				)).ToArray();
		}

		/// <summary>
		///A test for Year
		///</summary>
		[TestMethod()]
		public void YearTest() {
			for (int y = 1900; y < 2123; y++) {
				Assert.AreEqual(y, new YearlyZmanimDictionary(y, CreateZmanim(y)).Year);
			}
		}

		/// <summary>
		///A test for Values
		///</summary>
		[TestMethod()]
		public void ValuesTest() {
			for (int y = 1900; y < 2123; y++) {
				var values = CreateZmanim(y);
				var yzd = new YearlyZmanimDictionary(y, values);
				Assert.AreEqual(yzd.Values, ((IDictionary<DateTime, ZmanimInfo>)yzd).Values);
				Assert.IsTrue(values.SequenceEqual(yzd.Values));
			}
		}
		/// <summary>
		///A test for Keys
		///</summary>
		[TestMethod()]
		public void KeysTest() {
			for (int y = 1900; y < 2123; y++) {
				var yzd = new YearlyZmanimDictionary(y, CreateZmanim(y));
				var keys = yzd.Keys;
				Assert.AreEqual(yzd.Count, keys.Count);
				Assert.AreEqual(DateTime.IsLeapYear(y) ? 366 : 365, keys.Count);

				var expectedDate = new DateTime(y, 1, 1);
				foreach (var date in keys) {
					Assert.AreEqual(expectedDate, date);
					expectedDate = expectedDate.AddDays(1);
				}
			}
		}

		/// <summary>
		///A test for Item
		///</summary>
		[TestMethod()]
		public void ItemTest1() {
			for (int y = 1900; y < 2123; y++) {
				var values = CreateZmanim(y); 
				YearlyZmanimDictionary yzd = new YearlyZmanimDictionary(y, values); 
				for (int d = 0; d < values.Length; d++) {
					Assert.AreEqual(values[d], yzd[new DateTime(y, 1, 1).AddDays(d)]);
				}
			}
		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ItemTest2() { new YearlyZmanimDictionary(2000, CreateZmanim(2000))[new DateTime(1999, 12, 31)].ToString(); }

		/// <summary>
		///A test for Count
		///</summary>
		[TestMethod()]
		public void CountTest() {
			for (int y = 1900; y < 2123; y++) {
				var yzd = new YearlyZmanimDictionary(y, CreateZmanim(y));
				Assert.AreEqual(DateTime.IsLeapYear(y) ? 366 : 365, yzd.Count);
			}
		}

		/// <summary>
		///A test for TryGetValue
		///</summary>
		[TestMethod()]
		public void TryGetValueTest() {
			for (int y = 1900; y < 2123; y++) {
				var yzd = new YearlyZmanimDictionary(y, CreateZmanim(y));
				ZmanimInfo value;
				Assert.IsTrue(yzd.TryGetValue(new DateTime(y, 2, 28), out value));
				Assert.AreEqual(new DateTime(y, 2, 28), value.Date);
				Assert.IsFalse(yzd.TryGetValue(new DateTime(y - 1, 2, 28), out value));
				Assert.IsNull(value);
				Assert.IsTrue(yzd.TryGetValue(new DateTime(y, 10, 28), out value));
				Assert.AreEqual(new DateTime(y, 10, 28), value.Date);
			}
		}

		/// <summary>
		///A test for GetEnumerator
		///</summary>
		[TestMethod()]
		public void GetEnumeratorTest() {
			for (int y = 1900; y < 2123; y++) {
				var values = CreateZmanim(y); 
				YearlyZmanimDictionary yzd = new YearlyZmanimDictionary(y, values); 

				var date = new DateTime(y, 1, 1);
				foreach (var kvp in yzd) {
					Assert.AreEqual(date, kvp.Key);
					Assert.AreEqual(values[date.DayOfYear - 1], kvp.Value);
					date = date.AddDays(1);
				}
			}
		}

		/// <summary>
		///A test for CopyTo
		///</summary>
		[TestMethod()]
		public void CopyToTest() {
			for (int y = 1900; y < 2123; y++) {
				var values = CreateZmanim(y);
				var array = new KeyValuePair<DateTime, ZmanimInfo>[values.Length + 33];
				new YearlyZmanimDictionary(y, values).CopyTo(array, 33);
				Assert.IsTrue(array.Skip(33).Select(kvp => kvp.Value).SequenceEqual(values));
				Assert.IsTrue(array.Skip(33).All(kvp => kvp.Key == kvp.Value.Date));
			}
		}

		/// <summary>
		///A test for ContainsKey
		///</summary>
		[TestMethod()]
		public void ContainsKeyTest() {
			for (int y = 1900; y < 2123; y++) {
				var yzd = new YearlyZmanimDictionary(y, CreateZmanim(y));
				Assert.IsTrue(yzd.ContainsKey(new DateTime(y, 2, 28)));
				Assert.IsTrue(yzd.ContainsKey(new DateTime(y, 10, 28)));
				Assert.IsFalse(yzd.ContainsKey(new DateTime(y - 1, 2, 28)));
			}
		}

		/// <summary>
		///A test for Contains
		///</summary>
		[TestMethod()]
		public void ContainsTest() {
			for (int y = 1900; y < 2123; y++) {
				var yzd = new YearlyZmanimDictionary(y, CreateZmanim(y));
				Assert.IsTrue(yzd.Contains(yzd.ElementAt(321)));
				Assert.IsFalse(yzd.Contains(new KeyValuePair<DateTime, ZmanimInfo>(new DateTime(y - 1, 2, 28), null)));
				Assert.IsFalse(yzd.Contains(new KeyValuePair<DateTime, ZmanimInfo>(new DateTime(y, 2, 28), null)));
			}
		}
	}
}
