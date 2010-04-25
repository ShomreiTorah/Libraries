using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Maps a Singularity table to a table in an SQL database.</summary>
	public class SchemaMapping {
		///<summary>Creates a mapping for a schema.</summary>
		public SchemaMapping(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");

			Schema = schema;
			SqlName = schema.Name;

			Columns = new ColumnMappingCollection(this);

			Schema.ColumnAdded += (s, e) => Columns.AddMapping(e.Column);
			Schema.ColumnRemoved += (s, e) => Columns.RemoveMapping(e.Column);
		}

		///<summary>Gets the schema that this mapping maps.</summary>
		public TableSchema Schema { get; private set; }

		///<summary>Gets or sets the name of the corresponding table in the SQL database.</summary>
		public string SqlName { get; set; }
		///<summary>Gets or sets the schema of the corresponding table in the SQL database.</summary>
		public string SqlSchemaName { get; set; }

		///<summary>Gets the mapping objects for the columns in the table.</summary>
		public ColumnMappingCollection Columns { get; private set; }
	}

	///<summary>Maps a column in a Singularity table to a column in an SQL database.</summary>
	public class ColumnMapping {
		///<summary>Creates a mapping for a column.</summary>
		internal ColumnMapping(Column column) {
			if (column == null) throw new ArgumentNullException("column");

			Column = column;
			SqlName = column.Name;
		}

		///<summary>Gets the column that this mapping maps.</summary>
		public Column Column { get; private set; }

		///<summary>Gets or sets the name of the corresponding column in the SQL database.</summary>
		public string SqlName { get; set; }
	}

	///<summary>A collection of ColumnMapping objects.</summary>
	public class ColumnMappingCollection : ReadOnlyCollection<ColumnMapping> {
		///<summary>Creates a ColumnMappingCollection that wraps a list of ColumnMapping objects.</summary>
		internal ColumnMappingCollection(SchemaMapping schema) : base(schema.Schema.Columns.Select(c => new ColumnMapping(c)).ToList()) { SchemaMapping = schema; }
		///<summary>Gets the schema mapping that contains these columns.</summary>
		public SchemaMapping SchemaMapping { get; private set; }

		///<summary>Gets the mapping for column with the given name, or null if the schema has no column with that name.</summary>
		public ColumnMapping this[string name] { get { return this.FirstOrDefault(c => c.Column.Name == name); } }
		///<summary>Gets the mapping for the given column, or null if there is no mapping for that column.</summary>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public ColumnMapping this[Column column] { get { return this.FirstOrDefault(c => c.Column == column); } }

		///<summary>Adds a synchronizer for a column.</summary>
		public void AddMapping(Column column) {
			if (column == null) throw new ArgumentNullException("column");
			if (column.Schema != SchemaMapping.Schema) throw new ArgumentException("Column must belong to parent schema", "column");
			Items.Add(new ColumnMapping(column));
		}
		///<summary>Removes the mapping for the given column, preventing the column from being synchronized to the database.</summary>
		public void RemoveMapping(Column column) { Items.Remove(this[column]); }
	}
}
