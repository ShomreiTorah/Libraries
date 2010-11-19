using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace ShomreiTorah.Singularity {
	///<summary>Contains the schema of a Singularity table.</summary>
	public partial class TableSchema {
		///<summary>Gets the schema describing an object.</summary>
		///<param name="obj">A table, row, column, ChildRowCollection, or other object with a schema.</param>
		///<returns>The schema instance used by the object, or null if the object doesn't have a schema.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static TableSchema GetSchema(object obj) {
			var bs = obj as BindingSource;
			if (bs != null) return GetSchema(bs.List);

			var schema = obj as TableSchema;
			if (schema != null) return schema;

			var s = obj as ISchemaItem;
			if (s != null) return s.Schema;

			return null;
		}

		///<summary>Initializes a new instance of the <see cref="TableSchema"/> class.</summary>
		public TableSchema(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			Columns = new ColumnCollection(this);
			ChildRelations = new ChildRelationCollection(this);
			Name = name;
		}

		///<summary>Creates a table for this schema.</summary>
		///<remarks>This method is overridden to create typed tables.</remarks>
		public virtual Table CreateTable() { return new Table(this); }

		///<summary>Gets the columns in this schema.</summary>
		public ColumnCollection Columns { get; private set; }
		///<summary>Gets the child relations in this schema.</summary>
		public ChildRelationCollection ChildRelations { get; private set; }
		///<summary>Gets the name of the schema.</summary>
		public string Name { get; private set; }
		///<summary>Returns a string representation of this instance.</summary>
		public override string ToString() { return "Schema: " + Name; }

		///<summary>Gets the type of rows in tables of the schema.</summary>
		///<remarks>This property is overridden by typed schema.</remarks>
		public virtual Type RowType { get { return typeof(Row); } }

		Column primaryKey;
		///<summary>Gets or sets the column that serves as the primary key, or null if the schema has no primary key.</summary>
		public Column PrimaryKey {
			get { return primaryKey; }
			set {
				if (value != null)
					((ValueColumn)value).Unique = true;

				primaryKey = value;
			}
		}

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
		internal virtual void AddRow(Row row) { rows.Add(new WeakReference<Row>(row)); }
		internal void RemoveRow(Row row) { rows.RemoveAll(r => r.Target == row); }

		///<summary>Gets the attached rows belonging to this schema.</summary>
		///<remarks>This will only contain rows attached to a table.  It's used to validate column changes.</remarks>
		internal IEnumerable<Row> Rows { get { return rows.Select(w => w.Target).Where(r => r != null); } }

		internal void EachRow(Action<Row> method) {
			foreach (var row in Rows)
				method(row);
		}

		internal void ValidateName(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (Columns[name] != null)
				throw new ArgumentException("A column named " + name + " already exists", "name");
			if (ChildRelations[name] != null)
				throw new ArgumentException("A child relation named " + name + " already exists", "name");
		}
	}
	///<summary>A relation mapping a parent row to a set of child rows.</summary>
	public sealed class ChildRelation : INamedObject {
		string name;
		internal ChildRelation(ForeignKeyColumn childColumn, string name) {
			ParentSchema = childColumn.ForeignSchema;
			ChildSchema = childColumn.Schema;
			ChildColumn = childColumn;

			ParentSchema.ValidateName(name);
			this.name = name;			//Don't call the property setter to avoid a redundant SchemaChanged
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
	///<summary>A collection of child relations in a schema.</summary>
	public sealed class ChildRelationCollection : ReadOnlyCollection<ChildRelation> {
		//Called by ForeignKeyColumn
		internal ChildRelation AddRelation(ChildRelation relation) { Items.Add(relation); return relation; }
		internal void RemoveRelation(ChildRelation relation) { Items.Remove(relation); }

		internal ChildRelationCollection(TableSchema schema) : base(new List<ChildRelation>()) { Schema = schema; }

		///<summary>Gets the schema containing the columns.</summary>
		public TableSchema Schema { get; private set; }
		///<summary>Gets the child relation with the given name, or null if there is no column with that name.</summary>
		public ChildRelation this[string name] { get { return this.FirstOrDefault(r => r.Name == name); } }
	}

	///<summary>An object that references a TableSchema.</summary>
	interface ISchemaItem {
		TableSchema Schema { get; }
	}
}
