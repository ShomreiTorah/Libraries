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
			Schema = schema;
			Schema.ValidateName(name);
			this.name = name;	//Don't call the property setter to avoid a redundant SchemaChanged
		}

		///<summary>Gets the schema containing this column.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets or sets the name of the column.</summary>
		public string Name {
			get { return name; }
			set {
				if (value == Name) return;
				Schema.ValidateName(value);
				name = value;
				Schema.OnSchemaChanged();
			}
		}
		///<summary>Returns a string representation of this instance.</summary>
		public override string ToString() { return Name; }


		internal virtual void OnRemove() { Schema = null; }

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public abstract string ValidateValue(object value);

		internal virtual void OnValueChanged(Row row, object oldValue, object newValue) { }

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
		public Type DataType { get; private set; }

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
	///<summary>A column containing parent rows from a different table.</summary>
	public sealed class ForeignKeyColumn : Column {
		internal ForeignKeyColumn(TableSchema schema, string name, TableSchema foreignSchema, string foreignName)
			: base(schema, name) {
			if (foreignSchema == null) throw new ArgumentNullException("foreignSchema");
			ForeignSchema = foreignSchema;

			ChildRelation = ForeignSchema.ChildRelations.AddRelation(new ChildRelation(this, foreignName));
			ForeignSchema.OnSchemaChanged();
		}

		///<summary>Gets the schema of the rows that this column contains.</summary>
		public TableSchema ForeignSchema { get; private set; }
		///<summary>Gets the child relation from the foreign schema.</summary>
		public ChildRelation ChildRelation { get; private set; }

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public override string ValidateValue(object value) {
			if (value == null)
				return null;	//TODO: Handle invalid defaults
			var row = value as Row;
			if (row == null)
				return "The " + Name + " column can only hold rows";
			if (row.Schema != ForeignSchema)
				return "The " + Name + " column cannot hold " + row.Schema.Name + " rows.";

			return null;
		}

		internal override void OnValueChanged(Row row, object oldValue, object newValue) {
			base.OnValueChanged(row, oldValue, newValue);

			if (row.Table == null) return;
			if (row.Table.Context == null) return;

			var oldParent = oldValue as Row;
			if (oldParent != null) {
				var c = oldParent.ChildRows(ChildRelation, false);
				if (c != null) c.RemoveRow(row);
			}
			var newParent = newValue as Row;
			if (newParent != null) {
				var c = newParent.ChildRows(ChildRelation, false);
				if (c != null) c.AddRow(row);
			}
		}

		internal override void OnRemove() {
			base.OnRemove();
			ForeignSchema.ChildRelations.RemoveRelation(ChildRelation);
			ForeignSchema.EachRow(r => r.OnRelationRemoved(ChildRelation));
			ForeignSchema.OnSchemaChanged();
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
		///<summary>Adds a column containing parent rows from a different table.</summary>
		///<param name="name">The name of the column that contains the parent rows.</param>
		///<param name="foreignSchema">The schema for the parent rows.</param>
		///<param name="foreignName">The name of the child relation in the foreign schema.</param>
		public ForeignKeyColumn AddForeignKey(string name, TableSchema foreignSchema, string foreignName) {
			return AddColumn(new ForeignKeyColumn(Schema, name, foreignSchema, foreignName));
		}

		TColumn AddColumn<TColumn>(TColumn column) where TColumn : Column {
			Items.Add(column);
			Schema.EachRow(r => r.OnColumnAdded(column));
			Schema.OnColumnAdded(new ColumnEventArgs(column));
			return column;
		}

		///<summary>Removes a column from the schema.</summary>
		public void RemoveColumn(Column column) {
			if (column == null) throw new ArgumentNullException("column");
			if (column.Schema != Schema) throw new ArgumentException("Cannot remove column from different schema", "column");

			Items.Remove(column);
			Schema.EachRow(r => r.OnColumnRemoved(column));	//Will not raise events

			column.OnRemove();	//This will raise an event for ForeignKeyColumn, so we must be in a consistent state.
			Schema.OnColumnRemoved(new ColumnEventArgs(column));
		}

		///<summary>Gets the column with the given name, or null if there is no column with that name.</summary>
		public Column this[string name] { get { return this.FirstOrDefault(c => c.Name == name); } }
	}
}
