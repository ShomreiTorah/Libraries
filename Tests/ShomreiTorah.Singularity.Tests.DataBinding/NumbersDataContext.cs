using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Singularity.DataBinding;

namespace ShomreiTorah.Singularity.Tests.DataBinding {
	public class NumbersDataContext : DataContext {
		public NumbersDataContext() {
			Tables.AddTable(Numbers = Number.CreateTable());
			Tables.AddTable(Powers = PowersRow.CreateTable());

			for (int i = 1; i <= 20; i++) {
				var number = new Number { Value = i };
				Numbers.Rows.Add(number);

				for (int j = 0; j < 10; j++) {
					Powers.Rows.Add(new PowersRow { Base = number, Exponent = j });
				}
			}
		}

		public TypedTable<Number> Numbers { get; private set; }
		public TypedTable<PowersRow> Powers { get; private set; }
	}

	public class NumbersDataSource : BindableDataContextBase {
		protected override DataContext FindDataContext() { return new NumbersDataContext(); }
	}

	public class Number : Row {
		public static ValueColumn NumberColumn { get; private set; }
		public static ValueColumn NoteColumn { get; private set; }

		public static new readonly TypedSchema<Number> Schema = TypedSchema<Number>.Create("Numbers", schema => {
			schema.PrimaryKey = NumberColumn = schema.Columns.AddValueColumn("Value", typeof(int), null);

			NoteColumn = schema.Columns.AddValueColumn("Note", typeof(string), null);
			NoteColumn.AllowNulls = true;
		});

		public static TypedTable<Number> CreateTable() { return new TypedTable<Number>(Schema, () => new Number()); }

		public Number() : base(Schema) { }

		public int Value {
			get { return Field<int>(NumberColumn); }
			set { base[NumberColumn] = value; }
		}
		public string Note {
			get { return Field<string>(NoteColumn); }
			set { base[NoteColumn] = value; }
		}
		public override string ToString() { return "{" + Value.ToString() + "}"; }
		public IChildRowCollection<PowersRow> Powers { get { return TypedChildRows<PowersRow>(PowersRow.BaseColumn); } }
	}
	public class PowersRow : Row {
		public static ForeignKeyColumn BaseColumn { get; private set; }
		public static ValueColumn ExponentColumn { get; private set; }
		public static CalculatedColumn StringValueColumn { get; private set; }

		public static new readonly TypedSchema<PowersRow> Schema = TypedSchema<PowersRow>.Create("Powers", schema => {
			BaseColumn = schema.Columns.AddForeignKey("Base", Number.Schema, "Powers");
			ExponentColumn = schema.Columns.AddValueColumn("Exponent", typeof(int), null);
			StringValueColumn = schema.Columns.AddCalculatedColumn<PowersRow, string>("StringValue", p => Math.Pow(p.Base.Value, p.Exponent).ToString("n0"));
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
		}
	}
}
