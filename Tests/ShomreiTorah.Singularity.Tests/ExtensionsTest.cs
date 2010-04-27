using ShomreiTorah.Singularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ShomreiTorah.Singularity.Tests {


	[TestClass()]
	public class ExtensionsTest {
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
		public void SortDependenciesTest() {
			// Columns indicate direct dependencies
			//  0 1 2 3 4
			//0 • •      
			//1   • • • •
			//2     • •  
			//3       • •
			//4         •

			var schemas = new List<TableSchema>();
			schemas.Add(new TableSchema("0"));
			schemas.Add(new TableSchema("1"));
			schemas.Add(new TableSchema("2"));
			schemas.Add(new TableSchema("3"));
			schemas.Add(new TableSchema("4"));

			schemas[4].Columns.AddForeignKey("4 -> 3", schemas[3], "4 -> 3");
			schemas[4].Columns.AddForeignKey("4 -> 1", schemas[1], "4 -> 1");
			schemas[3].Columns.AddForeignKey("3 -> 2", schemas[2], "3 -> 2");
			schemas[2].Columns.AddForeignKey("2 -> 1", schemas[1], "2 -> 1");

			schemas[3].Columns.AddForeignKey("3 -> 1", schemas[1], "3 -> 1");
			schemas[1].Columns.AddForeignKey("1 -> 0", schemas[0], "1 -> 0");

			var rand = new Random();

			for (int i = 0; i < 10; i++)	//Test different orderings
				Assert.IsTrue(schemas.OrderBy(j => rand.Next()).SortDependencies().ToArray().SequenceEqual(schemas));
		}
	}
}
