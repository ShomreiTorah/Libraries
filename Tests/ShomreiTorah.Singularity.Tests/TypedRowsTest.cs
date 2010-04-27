using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShomreiTorah.Singularity.Tests {
	[TestClass]
	public class TypedRowsTest {
		public TestContext TestContext { get; set; }

		class PowersOfTwoRow : Row {
			public static ValueColumn ExponentColumn { get; private set; }
			public static ValueColumn StringValueColumn { get; private set; }

			public static new readonly TypedSchema<PowersOfTwoRow> Schema = TypedSchema<PowersOfTwoRow>.Create("Powers of 2", schema => {
				ExponentColumn = schema.Columns.AddValueColumn("Exponent", typeof(int), null);
				StringValueColumn = schema.Columns.AddValueColumn("StringValue", typeof(string), null);
				StringValueColumn.AllowNulls = false;
			});

			public static TypedTable<PowersOfTwoRow> CreateTable() { return new TypedTable<PowersOfTwoRow>(Schema, () => new PowersOfTwoRow()); }

			public PowersOfTwoRow() : base(Schema) { }
			public int Exponent {
				get { return Field<int>(ExponentColumn); }
				set { base[ExponentColumn] = value; }
			}
			public string StringValue {
				get { return Field<string>(StringValueColumn); }
				set { base[StringValueColumn] = value; }
			}
		}

		[TestMethod]
		public void BasicTypedRowsTest() {
			var table = PowersOfTwoRow.CreateTable();

			int rowCount = 0;
			table.RowAdded += (s, e) => {
				Assert.AreEqual(e.Row, table.Rows.Last());
				rowCount++;
				Assert.AreEqual(rowCount, table.Rows.Count);
			};
			table.RowRemoved += (s, e) => {
				Assert.IsFalse(table.Rows.Contains(e.Row));
				rowCount--;
				Assert.AreEqual(rowCount, table.Rows.Count);
			};

			for (int i = 0; i <= 10; i++) {
				table.Rows.Add(new PowersOfTwoRow { Exponent = i, StringValue = Math.Pow(2, i).ToString("n0") });
			}

			Assert.AreEqual("1,024", table.Rows.Last().StringValue);
			table.Rows.Remove(table.Rows.Last());
			Assert.AreEqual(9, table.Rows.Last().Exponent);
			Assert.AreEqual(10, table.Rows.Count);

			table.Rows.Clear();
			Assert.AreEqual(0, table.Rows.Count);
		}
		class Number : Row {
			public static ValueColumn NumberColumn { get; private set; }
			public static ValueColumn XRepsColumn { get; private set; }

			public static new readonly TypedSchema<Number> Schema = TypedSchema<Number>.Create("Numbers", schema => {
				schema.PrimaryKey = NumberColumn = schema.Columns.AddValueColumn("Number", typeof(int), null);

				XRepsColumn = schema.Columns.AddValueColumn("XReps", typeof(string), null);
				XRepsColumn.AllowNulls = false;
			});

			public static TypedTable<Number> CreateTable() { return new TypedTable<Number>(Schema, () => new Number()); }

			public Number() : base(Schema) { }

			public int Value {
				get { return Field<int>(NumberColumn); }
				set { base[NumberColumn] = value; }
			}
			public string XReps {
				get { return Field<string>(XRepsColumn); }
				set { base[XRepsColumn] = value; }
			}

			public IChildRowCollection<PowersRow> Powers { get { return TypedChildRows<PowersRow>(PowersRow.BaseColumn); } }
		}
		class PowersRow : Row {
			public static ForeignKeyColumn BaseColumn { get; private set; }
			public static ValueColumn ExponentColumn { get; private set; }
			public static ValueColumn StringValueColumn { get; private set; }

			public static new readonly TypedSchema<PowersRow> Schema = TypedSchema<PowersRow>.Create("Powers", schema => {
				BaseColumn = schema.Columns.AddForeignKey("Base", Number.Schema, "Powers");
				ExponentColumn = schema.Columns.AddValueColumn("Exponent", typeof(int), null);
				StringValueColumn = schema.Columns.AddValueColumn("StringValue", typeof(string), null);
				StringValueColumn.AllowNulls = false;
			});

			public static TypedTable<PowersRow> CreateTable() { return new TypedTable<PowersRow>(Schema, () => new PowersRow()); }

			public PowersRow() : base(Schema) { }

			public Number Base {
				get { return Field<Number>(BaseColumn); }
				set { base[BaseColumn] = value; }
			}
			public int Exponent {
				get { return Field<int>(ExponentColumn); }
				set { base[ExponentColumn] = value; }
			}
			public string StringValue {
				get { return Field<string>(StringValueColumn); }
				private set { base[StringValueColumn] = value; }
			}

			protected override void OnValueChanged(Column column, object oldValue, object newValue) {
				if (column == ExponentColumn)
					OnExponentChanged(oldValue, newValue);

				base.OnValueChanged(column, oldValue, newValue);
			}
			void OnExponentChanged(object oldValue, object newValue) {
				StringValue = Math.Pow(2, Exponent).ToString("n0");
			}
		}
		class NumbersPowersContext : DataContext {
			public NumbersPowersContext() {
				Tables.AddTable(Numbers = Number.CreateTable());
				Tables.AddTable(Powers = PowersRow.CreateTable());
			}

			public TypedTable<Number> Numbers { get; private set; }
			public TypedTable<PowersRow> Powers { get; private set; }
		}

		[TestMethod]
		public void RelatedTypedRowsTest() {
			var context = new NumbersPowersContext();

			for (int i = 0; i < 10; i++) {
				var number = new Number { Value = i, XReps = new string('x', i) };
				context.Numbers.Rows.Add(number);

				if (i % 3 == 0) {
					var children = number.Powers;

					int rowCount = 0;
					children.RowAdded += (s, e) => {
						Assert.AreEqual(e.Row, children.Last());
						rowCount++;
						Assert.AreEqual(rowCount, children.Count);
					};
					children.RowRemoved += (s, e) => {
						Assert.IsFalse(children.Contains(e.Row));
						rowCount--;
						Assert.AreEqual(rowCount, children.Count);
					};
				}

				for (int j = 0; j < 11; j++) {
					context.Powers.Rows.Add(new PowersRow { Base = number, Exponent = j });
				}
				context.Powers.Rows.Remove(context.Powers.Rows.Last());
				number.Powers.Last().RemoveRow();

				context.Powers.Rows.AddFromValues(number, 9);
				Assert.AreEqual(10, number.Powers.Count);
			}
			Assert.AreEqual(10, context.Numbers.Rows.Count);
			Assert.AreEqual(100, context.Powers.Rows.Count);

			context.Powers.Rows.Clear();
			Assert.AreEqual(0, context.Powers.Rows.Count);
			Assert.AreEqual(0, context.Numbers.Rows[0].Powers.Count);

			context.Powers.Rows.AddFromValues(context.Numbers.Rows[0], 4);
			Assert.AreEqual(1, context.Numbers.Rows[0].Powers.Count);
		}
		[TestMethod]
		public void XmlPersistenceTest() {
			var context = new NumbersPowersContext();

			for (int i = 0; i < 10; i++) {
				var number = new Number { Value = i, XReps = new string('x', i) };
				context.Numbers.Rows.Add(number);

				for (int j = 0; j < 10; j++) {
					context.Powers.Rows.Add(new PowersRow { Base = number, Exponent = j });
				}
			}

			var newContext = new NumbersPowersContext();
			newContext.ReadXml(context.ToXml());

			Assert.AreEqual(10, newContext.Numbers.Rows.Count);
			Assert.AreEqual(100, newContext.Powers.Rows.Count);
		}
	}
}
