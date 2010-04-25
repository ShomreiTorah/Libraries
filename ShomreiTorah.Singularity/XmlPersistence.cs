using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;

namespace ShomreiTorah.Singularity {
	partial class Table {
		///<summary>Saves the contents of this table to an XML element.</summary>
		public XElement ToXml() {
			return new XElement(XmlConvert.EncodeLocalName(Schema.Name),
				Rows.Select(row => new XElement("Row",
					Schema.Columns.Select(column =>
						new XElement(XmlConvert.EncodeLocalName(column.Name), GetColumnValue(row, column))
					)
				))
			);
		}

		static object GetColumnValue(Row row, Column column) {
			var foreignKey = column as ForeignKeyColumn;

			if (foreignKey == null)
				return row[column];

			if (foreignKey.ForeignSchema.PrimaryKey == null)
				throw new InvalidOperationException("A foreign key column that references a table without a primary key cannot be saved to XML");

			return row.Field<Row>(column)[foreignKey.ForeignSchema.PrimaryKey];
		}

		///<summary>Reads data from an XML element into this table.</summary>
		public void ReadXml(XElement element) {
			if (element == null) throw new ArgumentNullException("element");

			new XmlTablePopulator(this, element).FillTable();
		}
		sealed class XmlTablePopulator : TablePopulator<XElement> {
			readonly XElement tableElement;
			public XmlTablePopulator(Table table, XElement element) : base(table) { tableElement = element; }

			protected override IEnumerable<XElement> GetRows() { return tableElement.Elements("Row"); }

			protected override IEnumerable<KeyValuePair<Column, object>> GetValues(XElement values) {
				return Columns.Select(c => new KeyValuePair<Column, object>(c, values.Element(XmlConvert.EncodeLocalName(c.Name)).Value));
			}
		}
	}

	internal abstract class TablePopulator<TValueSource> {
		protected TablePopulator(Table table) { Table = table; }

		///<summary>Gets the table that this instance populates.</summary>
		public Table Table { get; private set; }

		public void FillTable() {
			//Maps each of this table's foreign keys to 
			//a dictionary mapping primary keys to rows.
			var keyMap = Columns
				.OfType<ForeignKeyColumn>()
				.ToDictionary(
					col => col,
					col => Table.Context.Tables[col.ForeignSchema].Rows.ToDictionary(
						foreignRow => foreignRow[foreignRow.Table.Schema.PrimaryKey]
					)
				);

			//TODO: Apply changes to existing rows.

			foreach (var rowSource in GetRows()) {
				var row = new Row(Table.Schema);

				foreach (var field in GetValues(rowSource)) {
					var foreignKey = field.Key as ForeignKeyColumn;

					if (foreignKey == null)
						row[field.Key] = field.Key.CoerceValue(field.Value, CultureInfo.InvariantCulture);
					else
						row[field.Key] = keyMap[foreignKey][foreignKey.ForeignSchema.PrimaryKey.CoerceValue(field.Value, CultureInfo.InvariantCulture)];
				}

				Table.Rows.Add(row);
			}
		}
		///<summary>Gets the columns that this instance fills.</summary>
		protected virtual IEnumerable<Column> Columns { get { return Table.Schema.Columns; } }
		///<summary>Runs the loop that returns rows.</summary>
		///<returns>A set of TValueSource objects that can be passed to GetValues to return the values for each row.</returns>
		protected abstract IEnumerable<TValueSource> GetRows();
		///<summary>Gets the column values for a row.</summary>
		///<param name="values">A TValueSource object from GetRows.</param>
		protected abstract IEnumerable<KeyValuePair<Column, object>> GetValues(TValueSource values);
	}
}
