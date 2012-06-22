using ShomreiTorah.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ShomreiTorah.Data.Tests {


	/// <summary>
	///This is a test class for AliyahNoteTest and is intended
	///to contain all AliyahNoteTest Unit Tests
	///</summary>
	[TestClass()]
	public class AliyahNoteTest {
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


		///<summary>Tests that malformed strings are normalized (this tests the parser).</summary>
		[TestMethod]
		public void NormalizationTest() {
			Assert.AreEqual("Son-in-law", new AliyahNote("  son  in --law   ").Text);
			Assert.AreEqual("מתנה", new AliyahNote("  mAtana ").Text);
			Assert.AreEqual("Father, מתנה", new AliyahNote("  fAtHeR  ;  mAtana ").Text);
			Assert.AreEqual("Brother-in-law.  He wants to do something", new AliyahNote("  Brother - - in Law ;  He wants to do something ").Text);
			Assert.AreEqual("מתנה.  Hopefully a big one!", new AliyahNote("  מתנה ;  Hopefully a big one!").Text);
		}

		///<summary>Tests the generation of the Text property.</summary>
		[TestMethod]
		public void TextTest() {
			Assert.AreEqual("", new AliyahNote().Text);
			Assert.AreEqual("Son-in-law", new AliyahNote { Relative = "  son  in --law  " }.Text);
			Assert.AreEqual("Father, מתנה", new AliyahNote { Relative = "  fAtHeR  ", Isמתנה = true }.Text);
			Assert.AreEqual("Grandfather.  We might like him!", new AliyahNote { Relative = "  GrandFather  ", Isמתנה = false, ExtraNote = "We might like him!" }.Text);
		}
	}
}
