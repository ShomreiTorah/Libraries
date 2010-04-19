using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity {
	///<summary>Contains the schema of a Singularity table.</summary>
	public class TableSchema {
		///<summary>Initializes a new instance of the <see cref="TableSchema"/> class.</summary>
		public TableSchema(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			Columns = new ColumnCollection(this);
			ChildRelations = new ChildRelationCollection(this);
			Name = name;
		}

		///<summary>Gets the columns in this schema.</summary>
		public ColumnCollection Columns { get; private set; }
		///<summary>Gets the child relations in this schema.</summary>
		public ChildRelationCollection ChildRelations { get; private set; }
		///<summary>Gets the name of the schema.</summary>
		public string Name { get; private set; }
		///<summary>Returns a string representation of this instance.</summary>
		public override string ToString() { return "Schema: " + Name; }

		#region Events
		///<summary>Occurs when the schema is changed.</summary>
		public event EventHandler SchemaChanged;
		///<summary>Raises the SchemaChanged event.</summary>
		internal protected virtual void OnSchemaChanged() { OnSchemaChanged(EventArgs.Empty); }
		///<summary>Raises the SchemaChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnSchemaChanged(EventArgs e) {
			if (SchemaChanged != null)
				SchemaChanged(this, e);
		}
		///<summary>Occurs when a column is added.</summary>
		public event EventHandler<ColumnEventArgs> ColumnAdded;
		///<summary>Raises the ColumnAdded event.</summary>
		///<param name="e">A ColumnEventArgs object that provides the event data.</param>
		internal protected virtual void OnColumnAdded(ColumnEventArgs e) {
			if (ColumnAdded != null)
				ColumnAdded(this, e);
		}
		///<summary>Occurs when a column is removed.</summary>
		public event EventHandler<ColumnEventArgs> ColumnRemoved;
		///<summary>Raises the ColumnRemoved event.</summary>
		///<param name="e">A ColumnEventArgs object that provides the event data.</param>
		internal protected virtual void OnColumnRemoved(ColumnEventArgs e) {
			if (ColumnRemoved != null)
				ColumnRemoved(this, e);
		}
		#endregion

		List<WeakReference<Row>> rows = new List<WeakReference<Row>>();
		internal void AddRow(Row row) { rows.Add(new WeakReference<Row>(row)); }
		internal void EachRow(Action<Row> method) {
			foreach (var row in rows.Select(w => w.Target).Where(r => r != null))
				method(row);
		}

		internal void ValidateName(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (Columns[name] != null)
				throw new ArgumentException("A column named " + name + " already exists", "name");
			if (ChildRelations[name] != null)
				throw new ArgumentException("A child relation named " + name + " already exists", "name");
		}

		///<summary>Checks whether a value is valid for a given column in a given row.</summary>
		///<param name="row">The row to validate for.</param>
		///<param name="column">The column containing the value.</param>
		///<param name="newValue">The value to validate.</param>
		///<returns>An error message, or null if the value is valid.</returns>
		public virtual string ValidateValue(Row row, Column column, object newValue) {
			if (row == null) throw new ArgumentNullException("row");
			if (column == null) throw new ArgumentNullException("column");
			if (row.Schema != this) throw new ArgumentException("Row must belong to this schema", "row");
			if (column.Schema != this) throw new ArgumentException("Column must belong to this schema", "column");

			return column.ValidateValue(newValue);
		}
	}
	///<summary>A relation mapping a parent row to a set of child rows.</summary>
	public sealed class ChildRelation {
		string name;
		internal ChildRelation(ForeignKeyColumn childColumn, string name) {
			ParentSchema = childColumn.ForeignSchema;
			ChildSchema = childColumn.Schema;
			ChildColumn = childColumn;

			ParentSchema.ValidateName(name);
			this.name = ChildColumn.Name;	//Don't call the property setter to avoid a redundant SchemaChanged
		}

		///<summary>Gets the schema that contains the parent rows.</summary>
		public TableSchema ParentSchema { get; private set; }
		///<summary>Gets the schema that contains the child rows.</summary>
		public TableSchema ChildSchema { get; private set; }
		///<summary>Gets the column in the child schema that references the parent rows.</summary>
		public ForeignKeyColumn ChildColumn { get; private set; }

		///<summary>Gets or sets the name of the child relation.</summary>
		public string Name {
			get { return name; }
			set {
				if (value == Name) return;
				ParentSchema.ValidateName(value);
				name = value;
				ParentSchema.OnSchemaChanged();
			}
		}
	}
	///<summary>A collection of ChildRelation objects.</summary>
	public class ChildRelationCollection : ReadOnlyCollection<ChildRelation> {
		//Called by ForeignKeyColumn
		internal ChildRelation AddRelation(ChildRelation relation) { Items.Add(relation); return relation; }
		internal void RemoveRelation(ChildRelation relation) { Items.Remove(relation); }

		internal ChildRelationCollection(TableSchema schema) : base(new List<ChildRelation>()) { Schema = schema; }


		///<summary>Gets the schema containing the columns.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the child relation with the given name, or null if there is no column with that name.</summary>
		public ChildRelation this[string name] { get { return this.FirstOrDefault(r => r.Name == name); } }
	}
}