using ShomreiTorah.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System;
using System.Reflection;
using System.IO;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for ConfigTest and is intended
	///to contain all ConfigTest Unit Tests
	///</summary>
	[TestClass()]
	public class ConfigTest {
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

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		//The file should exist
		public void ResetPathTest1() { Config.FilePath = Assembly.GetCallingAssembly().Location; }
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		//The file should exist
		public void ResetPathTest2() { Config.FilePath = "***"; }

		/// <summary>
		///A test for Xml
		///</summary>
		[TestMethod()]
		public void XmlTest() {
			Assert.IsNotNull(Config.Xml);
		}

		/// <summary>
		///A test for GetElement
		///</summary>
		[TestMethod()]
		public void GetElementTest() {
			Assert.AreEqual(Config.GetElement("Schedules").Name, "Schedules");
			Assert.AreEqual(Config.GetElement("Journal", "CallListInfo").Name, "CallListInfo");
		}

		/// <summary>
		///A test for ReadAttribute
		///</summary>
		[TestMethod()]
		public void ReadAttributeTest() {
			Assert.AreEqual(@"L:\Community\Rabbi Weinberger's Shul\Membership\ListMaker\Data.lmdb.gz", Config.ReadAttribute("ListMaker", "DatabasePath"));
		}
		[TestMethod]
		public void LoadedTest() { Assert.IsTrue(Config.Loaded); }
	}
}
