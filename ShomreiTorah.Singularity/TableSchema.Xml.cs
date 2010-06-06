using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ShomreiTorah.Common;
using System.Globalization;

namespace ShomreiTorah.Singularity {
	partial class TableSchema {
		///<summary>Saves this TableSchema to an XML element.</summary>
		public XElement ToXml() {
			var retVal = new XElement("TableSchema",
				new XAttribute("Name", Name),
				Columns.Where(c => c.SupportsXml).Select(c => c.ToXml())
			);
			if (PrimaryKey != null)
				retVal.Add(new XAttribute("PrimaryKey", PrimaryKey.Name));
			return retVal;
		}
		///<summary>Reads a set of TableSchemas from XML elements.</summary>
		///<param name="elements">A set of XML elements created by <see cref="TableSchema.ToXml"/>.</param>
		public static IEnumerable<TableSchema> FromXml(params	XElement[] elements) { return FromXml((IEnumerable<XElement>)elements); }
		///<summary>Reads a set of TableSchemas from XML elements.</summary>
		///<param name="elements">A set of XML elements created by <see cref="TableSchema.ToXml"/>.</param>
		public static IEnumerable<TableSchema> FromXml(IEnumerable<XElement> elements) {
			if (elements == null) throw new ArgumentNullException("elements");

			var retVal = new List<TableSchema>();

			foreach (var elem in elements) {
				retVal.Add(TableSchema.FromXml(elem, retVal));
			}

			return retVal;
		}

		///<summary>Reads a TableSchema from an XML element.</summary>
		///<param name="xml">An XML element created by <see cref="TableSchema.ToXml"/>.</param>
		///<param name="referencedSchemas">A set of schemas that may be referenced by the foreign keys in the schema being created.</param>
		public static TableSchema FromXml(XElement xml, params TableSchema[] referencedSchemas) { return FromXml(xml, (IEnumerable<TableSchema>)referencedSchemas); }
		///<summary>Reads a TableSchema from an XML element.</summary>
		///<param name="xml">An XML element created by <see cref="TableSchema.ToXml"/>.</param>
		///<param name="referencedSchemas">A set of schemas that may be referenced by the foreign keys in the schema being created.</param>
		public static TableSchema FromXml(XElement xml, IEnumerable<TableSchema> referencedSchemas) {
			if (xml == null) throw new ArgumentNullException("xml");

			var retVal = new TableSchema(xml.Attribute("Name").Value);

			foreach (var elem in xml.Elements()) {
				ColumnParsers[elem.Name.LocalName](retVal, elem, referencedSchemas);
			}

			var primaryKey = xml.Attribute("PrimaryKey");
			if (primaryKey != null)
				retVal.PrimaryKey = retVal.Columns[primaryKey.Value];

			return retVal;
		}

		static readonly Dictionary<string, ColumnParser> ColumnParsers = new Dictionary<string, ColumnParser> {
			{ "ValueColumn",		ValueColumn.FromXml			},
			{ "ForeignKeyColumn",	ForeignKeyColumn.FromXml	},
			//{ "CalculatedColumn",	CalculatedColumn.FromXml	},
		};
	}
	delegate void ColumnParser(TableSchema newSchema, XElement xml, IEnumerable<TableSchema> referencedSchemas);
	partial class Column {
		internal virtual bool SupportsXml { get { return true; } }
		internal abstract XElement ToXml();
	}
	partial class ValueColumn {
		internal override XElement ToXml() {
			return new XElement("ValueColumn",
				new XAttribute("Name", Name),
				new XAttribute("Type", DataType.AssemblyQualifiedName),
				new XAttribute("AllowNulls", AllowNulls),
				new XAttribute("Unique", Unique),

				new XElement("DefaultValue", DefaultValue ?? new XElement("Null"))
			);
		}

		internal static void FromXml(TableSchema newSchema, XElement xml, IEnumerable<TableSchema> referencedSchemas) {
			var column = newSchema.Columns.AddValueColumn(xml.Attribute("Name").Value, Type.GetType(xml.Attribute("Type").Value), null);

			var def = xml.Element("DefaultValue");
			if (def.Element("Null") == null)	//If it's not <Null />
				column.DefaultValue = column.CoerceValue(def.Value, CultureInfo.InvariantCulture);

			column.AllowNulls = Boolean.Parse(xml.Attribute("AllowNulls").Value);
			column.Unique = Boolean.Parse(xml.Attribute("Unique").Value);
		}
	}
	partial class ForeignKeyColumn {
		internal override XElement ToXml() {
			if (DefaultValue != null)
				throw new InvalidOperationException("Foreign key columns with non-null default values cannot be persisted to XML.");
			return new XElement("ForeignKeyColumn",
				new XAttribute("Name", Name),
				new XAttribute("ForeignSchemaName", ForeignSchema.Name),
				new XAttribute("RelationName", ChildRelation.Name),

				new XAttribute("AllowNulls", AllowNulls),
				new XAttribute("Unique", Unique)
			);
		}

		internal new static void FromXml(TableSchema newSchema, XElement xml, IEnumerable<TableSchema> referencedSchemas) {
			if (referencedSchemas == null) throw new ArgumentNullException("referencedSchemas", "referencedSchemas is required when schema XML has foreign key columns");

			var foreignSchemaName = xml.Attribute("ForeignSchemaName").Value;
			var foreignSchema = referencedSchemas.FirstOrDefault(ts => ts.Name == foreignSchemaName);
			if (foreignSchema == null)
				throw new ArgumentException("Referenced schema '" + foreignSchemaName + "' not found", "referencedSchemas");

			var column = newSchema.Columns.AddForeignKey(xml.Attribute("Name").Value, foreignSchema, xml.Attribute("RelationName").Value);

			column.AllowNulls = Boolean.Parse(xml.Attribute("AllowNulls").Value);
			column.Unique = Boolean.Parse(xml.Attribute("Unique").Value);
		}
	}

	partial class CalculatedColumn {
		internal override bool SupportsXml { get { return false; } }
		internal override XElement ToXml() {
			throw new NotSupportedException();
		}
		//internal static void FromXml(TableSchema newSchema, XElement xml, IEnumerable<TableSchema> referencedSchemas) {
		//    throw new NotImplementedException();
		//}

	}
}
