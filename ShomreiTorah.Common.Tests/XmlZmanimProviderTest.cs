using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common.Calendar.Zmanim;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for XmlZmanimProviderTest and is intended
	///to contain all XmlZmanimProviderTest Unit Tests
	///</summary>
	[TestClass()]
	public class XmlZmanimProviderTest {

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

		static readonly DataTable table = CreateTable();
		static DataTable CreateTable() {
			var table = new DataTable();
			table.Columns.Add("Zman1", typeof(DateTime));
			table.Columns.Add("Date", typeof(DateTime));
			table.Columns.Add("Zman2", typeof(DateTime));

			table.PrimaryKey = new[] { table.Columns["Date"] };

			table.Rows.Add(new DateTime(new TimeSpan(1, 1, 41).Ticks), new DateTime(2000, 1, 1), new DateTime(new TimeSpan(5, 10, 35).Ticks));
			table.Rows.Add(new DateTime(new TimeSpan(2, 2, 42).Ticks), new DateTime(2000, 1, 2), new DateTime(new TimeSpan(6, 20, 36).Ticks));
			table.Rows.Add(new DateTime(new TimeSpan(3, 3, 43).Ticks), new DateTime(2000, 1, 4), new DateTime(new TimeSpan(7, 30, 37).Ticks));
			table.Rows.Add(new DateTime(new TimeSpan(4, 4, 44).Ticks), new DateTime(2000, 1, 5), new DateTime(new TimeSpan(8, 40, 38).Ticks));

			return table;
		}
		static readonly XmlZmanimProvider provider = new XmlZmanimProvider(table);

		/// <summary>
		///A test for GetZmanim
		///</summary>
		[TestMethod()]
		public void GetZmanimTest() {
			Assert.IsNull(provider.GetZmanim(DateTime.MinValue));
			Assert.IsNull(provider.GetZmanim(DateTime.MaxValue));
			Assert.IsNull(provider.GetZmanim(new DateTime(2000, 1, 3)));

			Assert.AreEqual(2, provider.GetZmanim(new DateTime(2000, 1, 2)).Times.Count);
			Assert.AreEqual(new DateTime(2000, 1, 4), provider.GetZmanim(new DateTime(2000, 1, 4)).Date);
			Assert.AreEqual(new TimeSpan(01, 01, 41), provider.GetZmanim(new DateTime(2000, 1, 1))["Zman1"]);
			Assert.AreEqual(new TimeSpan(08, 40, 38), provider.GetZmanim(new DateTime(2000, 1, 5))["Zman2"]);

			var range = provider.GetZmanim(new DateTime(2000, 1, 2), new DateTime(2000, 1, 6)).ToArray();
			Assert.AreEqual(range.Length, 5);
			Assert.IsNull(range[1]);
			Assert.IsNull(range[4]);
			Assert.AreEqual(new DateTime(2000, 1, 4), range[2].Date);
			Assert.AreEqual(new TimeSpan(02, 02, 42), range[0]["Zman1"]);
			Assert.AreEqual(new TimeSpan(08, 40, 38), range[3]["Zman2"]);
		}

		/// <summary>
		///A test for DateColumn
		///</summary>
		[TestMethod()]
		[DeploymentItem("ShomreiTorah.Common.dll")]
		public void DateColumnTest() {
			Assert.AreEqual(table.Columns["Date"], provider.DateColumn);
		}

		/// <summary>
		///A test for MinDate
		///</summary>
		[TestMethod()]
		public void MinDateTest() {
			Assert.AreEqual(new DateTime(2000, 1, 1), provider.MinDate);
		}

		/// <summary>
		///A test for MaxDate
		///</summary>
		[TestMethod()]
		public void MaxDateTest() {
			Assert.AreEqual(new DateTime(2000, 1, 5), provider.MaxDate);
		}
	}
}
