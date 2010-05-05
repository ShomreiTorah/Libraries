using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Singularity.Dependencies;
using System.Linq.Expressions;

namespace ShomreiTorah.Singularity.Tests.Dependencies {
	/// <summary>
	/// Summary description for BasicParsingTest
	/// </summary>
	[TestClass]
	public class BasicParsingTest {

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void NoDependenciesTest() {
			Expression<Func<Row, object>> func = r =>
				Environment.TickCount + DateTime.Now.Second % 3 * 2 + (Enumerable.Range(0, 5).Select(i => i * i).Sum() / 67).ToString();

			var dep = DependencyParser.GetDependencyTree(new TableSchema("NoDependenciesTest"), func);
			Assert.IsFalse(dep.RequiresDataContext);
			Assert.IsInstanceOfType(dep, typeof(AggregateDependency));
			var ad = (AggregateDependency)dep;
			Assert.AreEqual(0, ad.Dependencies.Count);
		}

		[TestMethod]
		public void SimpleColumnTest() {
			var schema = new TableSchema("SimpleColumnTest");

			schema.Columns.AddValueColumn("Col1", typeof(int), 5);
			schema.Columns.AddValueColumn("Col2", typeof(int), 5);
			schema.Columns.AddValueColumn("Col3", typeof(int), 5);
			schema.Columns.AddValueColumn("Col4", typeof(int), 5);

			Expression<Func<Row, object>> func = r =>
				r["Col1"].ToString() + ((int)r["Col2"] % r.Field<int?>("Col3")) + r["Col4"];
			var dep = DependencyParser.GetDependencyTree(schema, func);
			Assert.IsFalse(dep.RequiresDataContext);
			Assert.IsInstanceOfType(dep, typeof(SameRowDependency));

			var srd = (SameRowDependency)dep;
			Assert.IsTrue(schema.Columns.SequenceEqual(srd.DependantColumns.OrderBy(c => c.Name)));
		}
	}
}
