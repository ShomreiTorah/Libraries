using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ShomreiTorah.Singularity.DataBinding {
	abstract class PropertyDescriptor<T> : PropertyDescriptor {
		protected PropertyDescriptor(string name, params Attribute[] attrs) : base(name, attrs) { }

		public override Type ComponentType { get { return typeof(T); } }

		public override bool ShouldSerializeValue(object component) {
			if (component == null) throw new ArgumentNullException("component");
			return ShouldSerializeValue((T)component);
		}
		protected abstract bool ShouldSerializeValue(T component);

		public override object GetValue(object component) {
			if (component == null) throw new ArgumentNullException("component");
			return GetValue((T)component);
		}
		protected abstract object GetValue(T component);

		public override void SetValue(object component, object value) {
			if (component == null) throw new ArgumentNullException("component");
			SetValue((T)component, value);
		}
		protected abstract void SetValue(T component, object value);

		public override void ResetValue(object component) {
			if (component == null) throw new ArgumentNullException("component");
			ResetValue((T)component);
		}
		protected abstract void ResetValue(T component);
	}

	sealed class ColumnPropertyDescriptor : PropertyDescriptor<Row> {
		public Column Column { get; private set; }

		public ColumnPropertyDescriptor(Column column) : base(column.Name) { this.Column = column; }

		protected override object GetValue(Row component) { return component[Column]; }
		protected override void SetValue(Row component, object value) { component[Column] = value; }
		protected override void ResetValue(Row component) { component[Column] = Column.DefaultValue; }

		public override bool CanResetValue(object component) { return true; }
		protected override bool ShouldSerializeValue(Row component) { return component[Column] == Column.DefaultValue; }

		public override bool IsReadOnly { get { return false; } }
		public override Type PropertyType { get { return Column.DataType; } }

		public override int GetHashCode() { return Column.GetHashCode(); }
		public override bool Equals(object obj) {
			var other = obj as ColumnPropertyDescriptor;
			return other != null && Column == other.Column;
		}
	}
	sealed class ChildRelationPropertyDescriptor : PropertyDescriptor<Row> {
		public ChildRelation Relation { get; private set; }

		public ChildRelationPropertyDescriptor(ChildRelation relation) : base(relation.Name) { this.Relation = relation; }

		protected override object GetValue(Row component) { return component.ChildRows(Relation); ; }
		protected override void SetValue(Row component, object value) { throw new NotSupportedException(); }
		protected override void ResetValue(Row component) { throw new NotSupportedException(); }

		public override bool CanResetValue(object component) { return false; }
		protected override bool ShouldSerializeValue(Row component) { return false; }

		public override bool IsReadOnly { get { return true; } }
		public override Type PropertyType { get { return typeof(IList<Row>); } }

		public override int GetHashCode() { return Relation.GetHashCode(); }
		public override bool Equals(object obj) {
			var other = obj as ChildRelationPropertyDescriptor;
			return other != null && Relation == other.Relation;
		}
	}
}
