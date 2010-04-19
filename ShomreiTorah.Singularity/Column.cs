using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity {
	///<summary>A column in a Singularity table.</summary>
	public abstract class Column {
		string name;
		object defaultValue;

		internal Column(TableSchema schema, string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (Schema.Columns[name] != null)
				throw new ArgumentException("A column named " + name + " already exists", "name");

			this.name = name;	//Don't call the property setter to avoid a redundant SchemaChanged
			Schema = schema;
		}

		///<summary>Gets the schema containing this column.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets or sets the name of the column.</summary>
		public string Name {
			get { return name; }
			set {
				if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
				if (value == Name) return;
				if (Schema.Columns[value] != null)
					throw new ArgumentException("A column named " + name + " already exists", "value");
				name = value;
				Schema.OnSchemaChanged();
			}
		}

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public abstract string ValidateValue(object value);

		///<summary>Gets or sets the default value of the column.</summary>
		public virtual object DefaultValue {
			get { return defaultValue; }
			set {
				var error = ValidateValue(value);
				if (!String.IsNullOrEmpty(error))
					throw new ArgumentException(error, "value");
				defaultValue = value;
			}
		}
	}
	///<summary>Provides data for column events.</summary>
	public class ColumnEventArgs : EventArgs {
		///<summary>Creates a new ColumnEventArgs instance.</summary>
		public ColumnEventArgs(Column column) { Column = column; }

		///<summary>Gets the row.</summary>
		public Column Column { get; private set; }
	}
	///<summary>A column containing simple values.</summary>
	public sealed class ValueColumn : Column {
		internal ValueColumn(TableSchema schema, string name, Type dataType, object defaultValue)
			: base(schema, name) {
			Name = name;
			DataType = dataType;
			DefaultValue = defaultValue;
		}

		///<summary>Gets or sets the data-type of the column, or null if the column can hold any datatype.</summary>
		public Type DataType { get; set; }

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public override string ValidateValue(object value) {
			if (DataType == null) return null;

			//TODO: Conversions (numeric, Nullable<T>, DBNull, etc) (abstract ConvertValue?)
			if (!DataType.IsInstanceOfType(value))
				return "The " + Name + " column cannot hold a " + (value == null ? "null" : value.GetType().Name) + " value.";
			return null;
		}
	}

	///<summary>A collection of Column objects.</summary>
	public class ColumnCollection : ReadOnlyCollection<Column> {
		internal ColumnCollection(TableSchema schema) : base(new List<Column>()) { Schema = schema; }

		///<summary>Gets the schema containing the columns.</summary>
		public TableSchema Schema { get; private set; }

		//Name uniqueness is enforced by the Column base class

		///<summary>Adds a column containing simple values.</summary>
		public ValueColumn AddValueColumn(string name, Type dataType, object defaultValue) {
			return AddColumn(new ValueColumn(Schema, name, dataType, defaultValue));
		}

		TColumn AddColumn<TColumn>(TColumn column) where TColumn : Column {
			Items.Add(column);
			Schema.EachTable(t => t.ProcessColumnAdded(column));
			Schema.OnColumnAdded(new ColumnEventArgs(column));
			return column;
		}

		///<summary>Removes a column from the schema.</summary>
		public void RemoveColumn(Column column) {
			if (column == null) throw new ArgumentNullException("column");
			if (column.Schema != Schema) throw new ArgumentException("Cannot remove column from different schema", "column");
			Items.Remove(column);
			Schema.EachTable(t => t.ProcessColumnRemoved(column));
			Schema.OnColumnRemoved(new ColumnEventArgs(column));
		}

		///<summary>Gets the column with the given name, or null if there is no column with that name.</summary>
		public Column this[string name] { get { return this.FirstOrDefault(c => c.Name == name); } }
	}
}
