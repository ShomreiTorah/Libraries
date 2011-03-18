using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShomreiTorah.Singularity.Tests {
	[TestClass]
	public class FilteredTableTest {

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
		public void SimpleTest() {
			TableSchema schema = new TableSchema("SimpleTest");
			schema.Columns.AddValueColumn("Index", typeof(int), null);
			schema.Columns.AddCalculatedColumn("FP", row => (int)Math.Pow(row.Field<int>("Index"), 1.5));

			var table = new Table(schema);
			for (int i = 0; i < 10; i++) {
				table.Rows.AddFromValues(i);
			}
			Assert.AreEqual(10, table.Rows.Count);
			var halfView = new VerifyableFilteredTable<Row>(table, row => row.Field<int>("Index") % 2 == 1);
			halfView.Verify();

			table.Rows[1]["Index"] = 0;
			Assert.AreEqual(4, halfView.Rows.Count);
			halfView.Verify();
			table.Rows[3].RemoveRow();
			Assert.AreEqual(3, halfView.Rows.Count);
			halfView.Verify();
			table.Rows.AddFromValues(13);
			Assert.AreEqual(4, halfView.Rows.Count);
			halfView.Verify();
		}
		static void VerifyFilteredTable<TRow>(FilteredTable<TRow> ft, Func<TRow, bool> filter) where TRow : Row {
			CollectionAssert.AreEqual((ICollection)ft.Rows, ft.Table.Rows.Where(r => filter((TRow)r)).ToArray());
		}
		class VerifyableFilteredTable<TRow> : FilteredTable<TRow> where TRow : Row {
			readonly Func<TRow, bool> filter;
			public VerifyableFilteredTable(ITable<TRow> table, Expression<Func<TRow, bool>> filter) : base(table, filter) { this.filter = filter.Compile(); }

			public void Verify() { VerifyFilteredTable(this, filter); }
		}
	}
}
