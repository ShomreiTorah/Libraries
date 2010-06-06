using ShomreiTorah.Singularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

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

		[Description("Tests XML persistence of empty schemas (without any columns)")]
		[TestMethod]
		public void XmlEmptyTest() {
			var schema = new TableSchema("XmlEmptyTest");
			Assert.AreEqual(new XElement("TableSchema", new XAttribute("Name", "XmlEmptyTest")).ToString(), schema.ToXml().ToString());
			var newSchema = TableSchema.FromXml(schema.ToXml());

			AssertSchemasEqual(schema, newSchema);
		}
		[Description("Tests XML persistence of simple schemas (without relations or calculated columns)")]
		[TestMethod]
		public void XmlSimpleTest() {
			var schema = new TableSchema("XmlSimpleTest");

			schema.Columns.AddValueColumn("Col1", typeof(DateTimeOffset?), null).Unique = true;
			schema.Columns.AddValueColumn("Col2", typeof(int?), 57);
			schema.Columns.AddValueColumn("Col3", typeof(int), null);
			schema.Columns.AddValueColumn("Col4", typeof(string), null);
			schema.Columns.AddValueColumn("Col5", typeof(string), "ABC").AllowNulls = false;
			schema.Columns.AddValueColumn("Col6", typeof(double), Math.PI).Unique = true;
			schema.Columns.AddValueColumn("Col7", typeof(DateTime), DateTime.Now).Unique = true;
			schema.PrimaryKey = schema.Columns.AddValueColumn("Col8", typeof(byte?), (byte)7);


			var newSchema = TableSchema.FromXml(schema.ToXml());
			AssertSchemasEqual(schema, newSchema);
		}
		[Description("Tests XML persistence of schemas with relations")]
		[TestMethod]
		public void XmlRelationsTest() {
			var parentSchema = new TableSchema("XmlRelationsTest - Parent");
			parentSchema.Columns.AddValueColumn("ABC", typeof(string), "DEF");

			var childSchema = new TableSchema("XmlRelationsTest - Child 1");
			childSchema.Columns.AddValueColumn("Col1", typeof(uint), 35u);

			childSchema.Columns.AddForeignKey("Parent1", parentSchema, "Child1");
			childSchema.Columns.AddForeignKey("Parent2", parentSchema, "Child2");

			var pLogsSchema = new TableSchema("XmlRelationsTest - Parent Logs");
			pLogsSchema.Columns.AddForeignKey("Row", parentSchema, "History");
			pLogsSchema.Columns.AddValueColumn("Timestamp", typeof(DateTime), null);
			pLogsSchema.Columns.AddValueColumn("ABC", typeof(string), "DEF");

			var cLogsSchema = new TableSchema("XmlRelationsTest - Child Logs");
			cLogsSchema.Columns.AddForeignKey("Row", childSchema, "History");
			cLogsSchema.Columns.AddValueColumn("Timestamp", typeof(DateTime), null);
			cLogsSchema.Columns.AddValueColumn("Col1", typeof(uint), 35u);


			var newSchemas = TableSchema.FromXml(parentSchema.ToXml(), childSchema.ToXml(), pLogsSchema.ToXml(), cLogsSchema.ToXml());

			Func<TableSchema, TableSchema> NewSchema = oldSchema => newSchemas.Single(ts => ts.Name == oldSchema.Name);

			AssertSchemasEqual(parentSchema, NewSchema(parentSchema));
			AssertSchemasEqual(childSchema, NewSchema(childSchema));
			AssertSchemasEqual(pLogsSchema, NewSchema(pLogsSchema));
			AssertSchemasEqual(cLogsSchema, NewSchema(cLogsSchema));
		}
		static void AssertSchemasEqual(TableSchema expected, TableSchema actual) {
			Assert.AreEqual(expected.Name, actual.Name);
			Assert.AreEqual(expected.Columns.Count, actual.Columns.Count);
			Assert.AreEqual(expected.ChildRelations.Count, actual.ChildRelations.Count);

			if (expected.PrimaryKey == null)
				Assert.IsNull(actual.PrimaryKey);
			else
				Assert.AreEqual(expected.PrimaryKey.Name, actual.PrimaryKey.Name);

			for (int i = 0; i < expected.Columns.Count; i++) {
				Column e = expected.Columns[i], a = actual.Columns[i];

				Assert.AreEqual(e.GetType(), a.GetType());
				Assert.AreEqual(e.DataType, a.DataType);
				Assert.AreEqual(e.DefaultValue, a.DefaultValue);
				Assert.AreEqual(e.Name, a.Name);

				if (e is ValueColumn) {
					ValueColumn te = (ValueColumn)e, ta = (ValueColumn)a;

					Assert.AreEqual(te.Unique, ta.Unique);
					Assert.AreEqual(te.AllowNulls, ta.AllowNulls);
				}
				if (e is ForeignKeyColumn) {	//Already checked ValueColumn properties
					ForeignKeyColumn te = (ForeignKeyColumn)e, ta = (ForeignKeyColumn)a;
					Assert.AreEqual(te.ChildRelation.Name, ta.ChildRelation.Name);
					AssertSchemasEqual(te.ForeignSchema, ta.ForeignSchema);
				}
				//if (e is CalculatedColumn) {
				//    CalculatedColumn te = (CalculatedColumn)e, ta = (CalculatedColumn)a;
				//}
			}
		}
	}
}
