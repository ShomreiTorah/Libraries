using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ShomreiTorah.Common;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity {
	///<summary>A column in a Singularity table.</summary>
	public abstract partial class Column : INamedObject, ISchemaItem {
		string name;
		object defaultValue;

		internal Column(TableSchema schema, string name) {
			Schema = schema;
			Schema.ValidateName(name);
			this.name = name;   //Don't call the property setter to avoid a redundant SchemaChanged
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

		///<summary>Indicates whether this column's values are indexed for O(1) lookup.  Not supported for calculated columns.</summary>
		public bool HasIndex { get; protected set; }

		internal virtual void OnRemove() { Schema = null; }
		internal virtual void OnRowAdded(Row row) { }
		internal virtual void OnRowRemoved(Row row, Table table) { }

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public abstract string ValidateValue(Row row, object value);
		///<summary>Checks whether a value's type is valid for this column.  This method will only validate the basic datatype.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public abstract string ValidateValueType(object value);

		internal virtual void OnValueChanged(Row row, object oldValue, object newValue) { }

		internal virtual bool CanValidate => true;

		///<summary>Gets or sets the default value of the column.</summary>
		public virtual object DefaultValue {
			get { return defaultValue; }
			set {
				if (CanValidate) {
					var error = ValidateValueType(value);
					if (!String.IsNullOrEmpty(error))
						throw new ArgumentException(error, "value");
				}
				defaultValue = value;
			}
		}

		///<summary>Gets the data-type of the column, or null if the column can hold any datatype.</summary>
		public Type DataType { get; protected set; }
		///<summary>Indicates whether the column's values can be changed.</summary>
		public bool ReadOnly { get; protected set; }

		///<summary>Coerces a value to the datatype for this column.</summary>
		public virtual object CoerceValue(object value, IFormatProvider provider) {
			if (value == null || value == DBNull.Value) return null;
			if (DataType == null) return value;

			if ((Nullable.GetUnderlyingType(DataType) ?? DataType) == typeof(Guid)) {
				var str = value as string;
				if (str != null)
					return new Guid(str);
				var bytes = value as byte[];
				if (bytes != null)
					return new Guid(bytes);
			}
			return Convert.ChangeType(value, DataType, provider);
		}
	}
	///<summary>Provides data for column events.</summary>
	public partial class ColumnEventArgs : EventArgs {
		///<summary>Creates a new ColumnEventArgs instance.</summary>
		public ColumnEventArgs(Column column) { Column = column; }

		///<summary>Gets the row.</summary>
		public Column Column { get; private set; }
	}
	///<summary>A column containing simple values.</summary>
	public partial class ValueColumn : Column {
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "DefaultValue is not overridden by this branch")]
		internal ValueColumn(TableSchema schema, string name, Type dataType, object defaultValue)
			: base(schema, name) {
			DataType = dataType;
			DefaultValue = defaultValue;

			allowNulls = DataType == null || !DataType.IsValueType;     //Don't set the property so as not to trigger validation
			if (!allowNulls && DataType.IsNullable()) {
				allowNulls = true;
				DataType = Nullable.GetUnderlyingType(DataType);
			}
		}


		///<summary>Creates an index for this column's values in all tables.  Unique columns are automatically indexed.</summary>
		public void AddIndex() {
			if (HasIndex) throw new InvalidOperationException($"Column {Schema.Name}.{Name} already has an index.");
			HasIndex = true;
			Schema.EachRow(row => row.Table.AddToIndex(row, this));
		}
		internal override void OnValueChanged(Row row, object oldValue, object newValue) {
			base.OnValueChanged(row, oldValue, newValue);

			if (row.Table == null) return;
			ExtroduceValue(row, row.Table, oldValue);
			IntroduceValue(row);
		}


		internal override void OnRowAdded(Row row) {
			base.OnRowAdded(row);
			IntroduceValue(row);
		}
		///<summary>Called when a row is added or a row's value is changed.</summary>
		protected virtual void IntroduceValue(Row row) {
			if (HasIndex) row.Table.AddToIndex(row, this);
		}
		///<summary>Called when a row is removed or a row's value is changed.</summary>
		protected virtual void ExtroduceValue(Row row, Table table, object oldValue) {
			if (HasIndex) table.GetIndex(this)[oldValue].Remove(row);
		}

		internal override void OnRowRemoved(Row row, Table table) {
			base.OnRowRemoved(row, table);
			ExtroduceValue(row, table, row[this]);
		}

		bool allowNulls, unique;

		///<summary>Gets or sets value indicating whether the column can contain null values.</summary>
		public bool AllowNulls {
			get { return allowNulls; }
			set {
				if (AllowNulls == value) return;

				if (!value && Schema.Rows.Where(r => r.Table != null).Any(r => r[this] == null))
					throw new InvalidOperationException("The " + Name + " column contains null values, and cannot disallow nulls.");

				allowNulls = value;
			}
		}
		///<summary>Gets or sets a value indicating whether this column can contain duplicate values.</summary>
		public bool Unique {
			get { return unique; }
			set {
				if (unique == value) return;
				if (!value) // This would create horrible messes for index-backed ChildRowCollections.
					throw new InvalidOperationException("Cannot set Unique back to false.");

				if (value) {
					// This is not expected to run when there are many rows, so
					// I don't bother trying to use the index.
					if (Schema.Rows.GroupBy(r => new { r.Table, Val = r[this] }).Any(g => g.Has(2)))
						throw new InvalidOperationException("The " + Name + " column contains duplicate values, and cannot enforce uniqueness.");
				}

				unique = value;
				if (!HasIndex) AddIndex();
			}
		}

		///<summary>Checks whether a value is valid for this column.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public override string ValidateValue(Row row, object value) {
			if (!AllowNulls && value == null)
				return "The " + Name + " column cannot contain nulls";

			//TODO: Conversions (numeric, Nullable<T>, DBNull, etc) (abstract ConvertValue?)
			if (value != null && DataType != null && !DataType.IsInstanceOfType(value))
				return "The " + Name + " column cannot hold a " + value.GetType().Name + " value.";

			if (Unique) {
				if (row.Table.GetIndex(this).TryGetValue(value, out var collection)
				 && collection.Count > 0 && collection.First() != row)
					return "The " + Name + " column cannot contain duplicate values";
			}

			return null;
		}
		///<summary>Checks whether a value's type is valid for this column.  This method will only validate the basic datatype.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public override string ValidateValueType(object value) {
			if (value == null || DataType == null)
				return null;
			if (!DataType.IsInstanceOfType(value))
				return "The " + Name + " column cannot hold a " + value.GetType().Name + " value.";
			return null;
		}
	}
	///<summary>A column containing parent rows from a different table.</summary>
	public sealed partial class ForeignKeyColumn : ValueColumn {
		internal ForeignKeyColumn(TableSchema schema, string name, TableSchema foreignSchema, string foreignName)
			: base(schema, name, foreignSchema.RowType, null) {
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
		public override string ValidateValue(Row row, object value) {
			var error = ValidateValueType(value);   //Validate the schema.  Everything else is handled correctly by the base class.
			if (error != null)
				return error;

			return base.ValidateValue(row, value);
		}
		///<summary>Checks whether a value's type is valid for this column.  This method will only validate the basic datatype.</summary>
		///<returns>An error message, or null if the value is valid.</returns>
		public override string ValidateValueType(object value) {
			if (value == null) return null;
			var foreignRow = value as Row;

			if (foreignRow == null)
				return "The " + Name + " column can only hold rows";
			if (foreignRow.Schema != ForeignSchema)
				return "The " + Name + " column cannot hold " + foreignRow.Schema.Name + " rows.";

			return null;
		}
		///<summary>Adds a row to this column's child row collections.</summary>
		protected override void IntroduceValue(Row row) {
			base.IntroduceValue(row);
			var newParent = (Row)row[this];
			if (row.Table.Context != null && newParent?.Table?.Context != null)
				newParent.ChildRows(ChildRelation, false)?.AddRow(row);
		}
		///<summary>Removes a row from this column's child row collections.</summary>
		protected override void ExtroduceValue(Row row, Table table, object oldValue) {
			base.ExtroduceValue(row, table, oldValue);
			var oldParent = (Row)oldValue;
			// If the parent is not in a DataContext, it has no collection to update.
			if (oldParent?.Table?.Context != null)
				oldParent.ChildRows(ChildRelation, false)?.RemoveRow(row);
		}

		internal override void OnRemove() {
			base.OnRemove();
			ForeignSchema.ChildRelations.RemoveRelation(ChildRelation);
			ForeignSchema.EachRow(r => r.OnRelationRemoved(ChildRelation));
			ForeignSchema.OnSchemaChanged();
		}
	}


	///<summary>A collection of columns in a schema.</summary>
	public sealed partial class ColumnCollection : ReadOnlyCollection<Column> {
		internal ColumnCollection(TableSchema schema) : base(new List<Column>()) { Schema = schema; }

		///<summary>Gets the schema containing the columns.</summary>
		public TableSchema Schema { get; private set; }

		//Parameters are validated by the Column base class

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
			Schema.OnSchemaChanged();
			return column;
		}

		///<summary>Checks whether the collection contains a column with the specified name.</summary>
		public bool Contains(string name) { return this[name] != null; }

		///<summary>Removes a column from the schema.</summary>
		public void RemoveColumn(string name) { RemoveColumn(this[name]); }
		///<summary>Removes a column from the schema.</summary>
		public void RemoveColumn(Column column) {
			if (column == null) throw new ArgumentNullException("column");
			if (column.Schema != Schema) throw new ArgumentException("Cannot remove column from different schema", "column");

			Items.Remove(column);
			Schema.EachRow(r => r.OnColumnRemoved(column)); //Will not raise events

			column.OnRemove();  //This will raise an event for ForeignKeyColumn, so we must be in a consistent state.
			Schema.OnColumnRemoved(new ColumnEventArgs(column));
			Schema.OnSchemaChanged();
		}

		///<summary>Gets the column with the given name, or null if there is no column with that name.</summary>
		public Column this[string name] { get { return this.FirstOrDefault(c => c.Name == name); } }
	}

	///<summary>An object with a unique name.</summary>
	public interface INamedObject {
		///<summary>Gets the object's name.</summary>
		string Name { get; }
	}
}
