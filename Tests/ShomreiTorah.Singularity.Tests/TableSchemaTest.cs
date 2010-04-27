using ShomreiTorah.Singularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ShomreiTorah.Singularity.Tests {
	[TestClass]
	public class TableSchemaTest {
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
		public void SchemaNameTest() {
			var schema = new TableSchema("SchemaNameTest");
			Assert.AreEqual(schema.Name, "SchemaNameTest");
			var table = new Table("SchemaNameTest 2");
			Assert.AreEqual(table.Schema.Name, "SchemaNameTest 2");
		}

		[TestMethod]
		public void ColumnsTest() {
			int columnCount = 0, schemaChangeCount = 0;
			var schema = new TableSchema("ColumnAddedTest");
			schema.ColumnAdded += (s, e) => {
				Assert.AreEqual(e.Column, schema.Columns.Last());
				columnCount++;
				Assert.AreEqual(columnCount, schema.Columns.Count);
			};
			schema.ColumnRemoved += (s, e) => {
				Assert.IsFalse(schema.Columns.Contains(e.Column));
				columnCount--;
				Assert.AreEqual(columnCount, schema.Columns.Count);
			};

			schema.SchemaChanged += delegate { schemaChangeCount++; };

			schema.Columns.AddValueColumn("Col1", typeof(int), 4);
			schema.Columns.AddValueColumn("Col2", typeof(string), "Hello!");
			schema.Columns.AddValueColumn("Col3", typeof(DateTime?), (DateTime?)DateTime.Now);

			schema.Columns.RemoveColumn("Col2");
			schema.Columns.RemoveColumn(schema.Columns.Last());
			schema.Columns.AddValueColumn("Col2", typeof(TimeSpan), TimeSpan.Zero);
			Assert.AreEqual(2, schema.Columns.Count);
			Assert.AreEqual(6, schemaChangeCount);
		}

		[TestMethod]
		public void RelationTest() {
			var childSchema = new TableSchema("RelationTest Child");
			var parentSchema = new TableSchema("RelationTest Parent");

			var foreignColumn = childSchema.Columns.AddForeignKey("Parent", parentSchema, "Children");
			Assert.AreEqual(parentSchema, foreignColumn.ForeignSchema);

			Assert.AreEqual(1, parentSchema.ChildRelations.Count);
			Assert.AreEqual(foreignColumn.ChildRelation, parentSchema.ChildRelations[0]);
			Assert.AreEqual(foreignColumn, parentSchema.ChildRelations[0].ChildColumn);
			Assert.AreEqual(childSchema, parentSchema.ChildRelations[0].ChildSchema);
			Assert.AreEqual(parentSchema, parentSchema.ChildRelations[0].ParentSchema);

			childSchema.Columns.RemoveColumn(foreignColumn);
			Assert.AreEqual(0, childSchema.Columns.Count);
			Assert.AreEqual(0, parentSchema.Columns.Count);
			Assert.AreEqual(0, childSchema.ChildRelations.Count);
			Assert.AreEqual(0, parentSchema.ChildRelations.Count);
		}
	}
}
