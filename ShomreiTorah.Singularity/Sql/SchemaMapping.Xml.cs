using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ShomreiTorah.Singularity.Sql {
	partial class SchemaMapping {
		///<summary>Saves this SchemaMapping to an XML element.</summary>
		public XElement ToXml() {
			return new XElement("SchemaMapping",
				new XAttribute("SchemaName", Schema.Name),
				new XAttribute("SqlName", SqlName),

				SqlSchemaName == null ? null : new XAttribute("SqlSchemaName", SqlSchemaName),

				Columns.Select(c =>
					new XElement("ColumnMapping",
						new XAttribute("ColumnName", c.Column.Name),
						new XAttribute("SqlName", c.SqlName)
					)
				)
			);
		}

		///<summary>Reads SchemaMappings from XML elements.</summary>
		///<param name="elements">A set of XML elements created by <see cref="SchemaMapping.ToXml"/>.</param>
		///<param name="schemas">The schemas mapped by the mappings.</param>
		public static IEnumerable<SchemaMapping> FromXml(IEnumerable<XElement> elements, IEnumerable<TableSchema> schemas) {
			return elements.Select(xml => SchemaMapping.FromXml(xml, schemas));
		}

		///<summary>Reads a SchemaMapping from an XML element.</summary>
		///<param name="xml">An XML element created by <see cref="SchemaMapping.ToXml"/>.</param>
		///<param name="schemas">The schema mapped by the mapping.</param>
		public static SchemaMapping FromXml(XElement xml, params TableSchema[] schemas) { return FromXml(xml, (IEnumerable<TableSchema>)schemas); }
		///<summary>Reads a SchemaMapping from an XML element.</summary>
		///<param name="xml">An XML element created by <see cref="SchemaMapping.ToXml"/>.</param>
		///<param name="schemas">The schema mapped by the mapping.</param>
		public static SchemaMapping FromXml(XElement xml, IEnumerable<TableSchema> schemas) {
			if (xml == null) throw new ArgumentNullException("xml");
			if (schemas == null) throw new ArgumentNullException("schemas");

			var schemaName = xml.Attribute("SchemaName").Value;
			var schema = schemas.SingleOrDefault(ts => ts.Name == schemaName);
			if (schema == null)
				throw new ArgumentException("Schema '" + schemaName + "' not found", "schemas");

			var retVal = new SchemaMapping(schema, false);

			retVal.SqlName = xml.Attribute("SqlName").Value;

			if (xml.Attribute("SqlSchemaName") != null)
				retVal.SqlSchemaName = xml.Attribute("SqlSchemaName").Value;

			foreach (var cm in xml.Elements("ColumnMapping")) {
				retVal.Columns.AddMapping(
					schema.Columns[cm.Attribute("ColumnName").Value],
					cm.Attribute("SqlName").Value
				);
			}
			return retVal;
		}
	}
}
