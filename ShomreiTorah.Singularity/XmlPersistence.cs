using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity {
	partial class DataContext {
		///<summary>Saves the tables in this DataContext to an XML document.</summary>
		public XDocument ToXml() {
			return new XDocument(new XElement("DataContext", Tables.SortDependencies(t => t.Schema).Select(t => t.ToXml())));
		}

		///<summary>Reads data into this DataContext's tables from an XML element.</summary>
		public void ReadXml(XContainer element) {
			if (element == null) throw new ArgumentNullException("element");
			if (element.Elements().Count() == 1 && element.Elements().Single().Name == "DataContext")
				element = element.Elements().Single();

			foreach (var tableElement in element.Elements()) {
				Tables[XmlConvert.DecodeName(tableElement.Name.LocalName)].ReadXml(tableElement);
			}
		}
	}

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
			readonly string primaryKeyName;
			public XmlTablePopulator(Table table, XElement element)
				: base(table) {
				tableElement = element;

				if (table.Schema.PrimaryKey != null)
					primaryKeyName = XmlConvert.EncodeLocalName(table.Schema.PrimaryKey.Name);
			}

			protected override IEnumerable<XElement> GetRows() { return tableElement.Elements("Row"); }

			protected override IEnumerable<KeyValuePair<Column, object>> GetValues(XElement values) {
				return Columns.Select(c => new KeyValuePair<Column, object>(c, values.Element(XmlConvert.EncodeLocalName(c.Name)).Value));
			}

			[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
			protected override object GetPrimaryKey(XElement values) { return values.Element(primaryKeyName).Value; }
		}
	}

	internal abstract class TablePopulator<TValueSource> {
		protected TablePopulator(Table table) { Table = table; }

		///<summary>Gets the table that this instance populates.</summary>
		public Table Table { get; private set; }

		public void FillTable() {
			//Maps each of this table's foreign keys to 
			//a dictionary mapping primary keys to rows.
			var foreignKeyMap = Columns
				.OfType<ForeignKeyColumn>()
				.ToDictionary(
					col => col,
					col => Table.Context.Tables[col.ForeignSchema].Rows.ToDictionary(
						foreignRow => foreignRow[foreignRow.Table.Schema.PrimaryKey]
					)
				);

			Dictionary<object, Row> keyMap = null;
			if (Table.Schema.PrimaryKey != null) {
				keyMap = Table.Rows.ToDictionary(row => row[Table.Schema.PrimaryKey]);
			}

			foreach (var rowSource in GetRows()) {
				Row row = null;

				if (keyMap != null) {
					if (keyMap.TryGetValue(CoercePrimaryKey(rowSource), out row))
						keyMap.Remove(row[Table.Schema.PrimaryKey]);
				}

				if (row == null)
					row = Table.Rows.CreateRow();

				foreach (var field in GetValues(rowSource)) {
					var foreignKey = field.Key as ForeignKeyColumn;

					if (foreignKey == null)
						row[field.Key] = field.Key.CoerceValue(field.Value, CultureInfo.InvariantCulture);
					else
						row[field.Key] = foreignKeyMap[foreignKey][foreignKey.ForeignSchema.PrimaryKey.CoerceValue(field.Value, CultureInfo.InvariantCulture)];
				}

				if (row.Table == null)		//If we created this row, as opposed to getting it out of the keyMap
					Table.Rows.Add(row);
			}

			//Next, remove all rows that weren't in the source.
			//(All rows that were in the source would have been
			//removed from keyMap when they were loaded)
			if (keyMap != null) {
				foreach (var oldRow in Table.Rows.Intersect(keyMap.Values).ToArray()) {
					oldRow.RemoveRow();
				}
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

		private object CoercePrimaryKey(TValueSource rowSource) {
			return Table.Schema.PrimaryKey.CoerceValue(GetPrimaryKey(rowSource), CultureInfo.InvariantCulture);
		}


		///<summary>Gets the primary key for a row.</summary>
		///<param name="values">A TValueSource object from GetRows.</param>
		protected abstract object GetPrimaryKey(TValueSource values);
	}
}
