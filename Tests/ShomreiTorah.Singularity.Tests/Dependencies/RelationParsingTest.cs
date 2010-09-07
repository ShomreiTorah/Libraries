using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using ShomreiTorah.Singularity.Dependencies;

namespace ShomreiTorah.Singularity.Tests.Dependencies {
	/// <summary>
	/// Summary description for RelationParsingTest
	/// </summary>
	[TestClass]
	public class RelationParsingTest {
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

		static RelationParsingTest() {
			NumbersSchema = new TableSchema("Numbers");
			PowersSchema = new TableSchema("Powers");

			NumbersSchema.Columns.AddValueColumn("Number", typeof(int), 0);
			NumbersSchema.Columns.AddValueColumn("IsEven", typeof(bool), false);

			PowersKeyColumn = PowersSchema.Columns.AddForeignKey("Number", NumbersSchema, "Powers");

			PowersSchema.Columns.AddValueColumn("Exponent", typeof(int), 0);
			PowersSchema.Columns.AddValueColumn("Value", typeof(int), 0);
		}

		static readonly TableSchema NumbersSchema, PowersSchema;
		static readonly ForeignKeyColumn PowersKeyColumn;

		[Description("Ensures that accessing a ForeignKeyColumn but not the columns in the foreign row doesn't add a ParentRowDependency.")]
		[TestMethod]
		public void ForeignKeyColumnTest() {
			Expression<Func<Row, object>> func = r =>
				r.Field<Row>("Number") != null;

			var dep = DependencyParser.GetDependencyTree(PowersSchema, func);

			Assert.IsFalse(dep.RequiresDataContext);
			var srd = (SameRowDependency)dep;

			Assert.AreEqual(0, srd.NestedDependencies.Count);
			Assert.AreEqual(1, srd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn, srd.DependentColumns[0]);
		}
		[Description("Ensures that accessing columns in a parent row adds a ParentRowDependency.")]
		[TestMethod]
		public void ParentRowColumnTest() {
			Expression<Func<Row, object>> func = r =>
				((Row)r["Number"])["Number"].ToString() + (r.Field<Row>("Number").Field<bool>("IsEven") ? "/" : "-");

			var dep = DependencyParser.GetDependencyTree(PowersSchema, func);

			Assert.IsTrue(dep.RequiresDataContext);
			var srd = (SameRowDependency)dep;

			Assert.AreEqual(1, srd.NestedDependencies.Count);
			Assert.AreEqual(1, srd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn, srd.DependentColumns[0]);

			var prd = (ParentRowDependency)srd.NestedDependencies[0];
			Assert.AreEqual(0, prd.NestedDependencies.Count);
			Assert.AreEqual(PowersKeyColumn, prd.ParentColumn);

			Assert.IsTrue(NumbersSchema.Columns.SequenceEqual(prd.DependentColumns));
		}
		[Description("Ensures that accessing a child row collection (without any columns) adds a ChildRowDependency.")]
		[TestMethod]
		public void ChildRowsTest() {
			Expression<Func<Row, object>> func = r =>
				r.ChildRows("Powers");

			var dep = DependencyParser.GetDependencyTree(NumbersSchema, func);

			Assert.IsTrue(dep.RequiresDataContext);
			var ad = (AggregateDependency)dep;

			Assert.AreEqual(1, ad.Dependencies.Count);

			var prd = (ChildRowDependency)ad.Dependencies[0];
			Assert.AreEqual(0, prd.NestedDependencies.Count);
			Assert.AreEqual(0, prd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn.ChildRelation, prd.ChildRelation);
		}
		[Description("Ensures that aggregating a child row collection (without any columns) adds a ChildRowDependency.")]
		[TestMethod]
		public void ChildRowsLINQTest() {
			Expression<Func<Row, object>> func = r =>
				r.ChildRows("Powers").Distinct().Count();

			var dep = DependencyParser.GetDependencyTree(NumbersSchema, func);

			Assert.IsTrue(dep.RequiresDataContext);
			var ad = (AggregateDependency)dep;

			Assert.AreEqual(1, ad.Dependencies.Count);

			var crd = (ChildRowDependency)ad.Dependencies[0];
			Assert.AreEqual(0, crd.NestedDependencies.Count);
			Assert.AreEqual(0, crd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn.ChildRelation, crd.ChildRelation);
		}

		[Description("Ensures that accessing columns in child row adds a ChildRowDependency.")]
		[TestMethod]
		public void ChildRowsColumnTest() {
			Expression<Func<Row, object>> func = r =>
			-(int)r["Number"] + r.ChildRows("Powers").Last().Field<int>("Exponent") * r.ChildRows("Powers").Sum(p => (int)p["Value"] / p.Field<long>("Number"));

			var dep = DependencyParser.GetDependencyTree(NumbersSchema, func);

			Assert.IsTrue(dep.RequiresDataContext);
			var srd = (SameRowDependency)dep;

			Assert.AreEqual(1, srd.NestedDependencies.Count);
			Assert.AreEqual(1, srd.DependentColumns.Count);
			Assert.AreEqual(NumbersSchema.Columns[0], srd.DependentColumns[0]);

			var crd = (ChildRowDependency)srd.NestedDependencies[0];
			Assert.AreEqual(0, crd.NestedDependencies.Count);
			Assert.AreEqual(PowersKeyColumn.ChildRelation, crd.ChildRelation);

			Assert.IsTrue(PowersSchema.Columns.OrderBy(c => c.Name).SequenceEqual(crd.DependentColumns.OrderBy(c => c.Name)));
		}

		[Description("Ensures that chained relations in lambdas are handled correctly.")]
		[TestMethod]
		public void NestedRelationTest() {
			Expression<Func<Row, object>> func = r =>
				from power in ((Row)r["Number"]).ChildRows("Powers")
				where (int)power["Exponent"] % 2 == 0
				select power.Field<int>("Value")
					 / power.Field<Row>("Number").ChildRows("Powers").Sum(p => p.Field<int>("Value"));

			var dep = DependencyParser.GetDependencyTree(PowersSchema, func);

			Assert.IsTrue(dep.RequiresDataContext);

			var srd = (SameRowDependency)dep;
			Assert.AreEqual(1, srd.NestedDependencies.Count);
			Assert.AreEqual(1, srd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn, srd.DependentColumns[0]);

			var prd = (ParentRowDependency)srd.NestedDependencies[0];	// ((Row)r["Number"]).
			Assert.AreEqual(0, prd.DependentColumns.Count);
			Assert.AreEqual(1, prd.NestedDependencies.Count);
			Assert.AreEqual(PowersKeyColumn, prd.ParentColumn);

			var crd = (ChildRowDependency)prd.NestedDependencies[0];	// ((Row)r["Number"]).ChildRows("Powers") (In from clause)
			Assert.AreEqual(1, crd.NestedDependencies.Count);
			Assert.AreEqual(PowersKeyColumn.ChildRelation, crd.ChildRelation);

			Assert.IsTrue(PowersSchema.Columns.OrderBy(c => c.Name).SequenceEqual(crd.DependentColumns.OrderBy(c => c.Name)));

			prd = (ParentRowDependency)crd.NestedDependencies[0];	// / power.Field<Row>("Number").
			Assert.AreEqual(0, prd.DependentColumns.Count);
			Assert.AreEqual(1, prd.NestedDependencies.Count);
			Assert.AreEqual(PowersKeyColumn, prd.ParentColumn);


			crd = (ChildRowDependency)prd.NestedDependencies[0];	// Field<Row>("Number").ChildRows("Powers") (in select clause)
			Assert.AreEqual(0, crd.NestedDependencies.Count);
			Assert.AreEqual(1, crd.DependentColumns.Count);
			Assert.AreEqual(PowersKeyColumn.ChildRelation, crd.ChildRelation);

			Assert.AreEqual(PowersSchema.Columns["Value"], crd.DependentColumns[0]);
		}
	}
}
