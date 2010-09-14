using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	///<summary>A column displayed in the results of an ItemSelector.</summary>
	public abstract class ResultColumn {
		int width = 100;
		bool visible = true;
		string caption;

		internal virtual void SetOwner(RepositoryItemItemSelector owner) {
			if (owner == null) throw new ArgumentNullException("owner");
			Owner = owner;
			if (Owner.DataSource != null) OnDataSourceSet();
		}
		internal virtual void OnDataSourceSet() { }

		///<summary>Gets the ItemSelector that owns the column.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RepositoryItemItemSelector Owner { get; private set; }

		///<summary>Gets or sets the width of the column.</summary>
		[Description("Gets or sets the width of the column.")]
		[Category("Appearance")]
		[DefaultValue(100)]
		public int Width {
			get { return width; }
			set { width = value; }
		}
		///<summary>Gets or sets whether the column appears in the results grid.</summary>
		[Description("Gets or sets whether the column appears in the results grid.")]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool Visible {
			get { return visible; }
			set { visible = value; }
		}
		///<summary>Gets or sets the caption displayed in the column header.</summary>
		[Description("Gets or sets the caption displayed in the column header.")]
		[Category("Appearance")]
		public string Caption {
			get { return caption; }
			set { caption = value; }
		}

		///<summary>Gets the string displayed in this column for the given row.</summary>
		public abstract string GetValue(object row);

		///<summary>Indicates whether the code is running in design mode.</summary>
		protected bool IsDesignMode { get { return Owner == null || Owner.IsDesignMode; } }

		///<summary>Creates a copy of this column for a new repository item.</summary>
		protected internal abstract ResultColumn Copy();
	}
	///<summary>A column that displays a property of the underlying data items.</summary>
	public class DataSourceColumn : ResultColumn {
		///<summary>Creates a new DataSourceColumn.</summary>
		public DataSourceColumn() { }
		///<summary>Creates a new DataSourceColumn that displays the data in the specified field.</summary>
		public DataSourceColumn(string fieldName) { FieldName = fieldName; }
		///<summary>Creates a new DataSourceColumn that displays the data in the specified field.</summary>
		public DataSourceColumn(string fieldName, int width) { FieldName = fieldName; Width = width; }

		string fieldName;
		string formatString = "{0}";

		///<summary>Gets or sets the name of the field in the underlying data source.</summary>
		[Description("Gets or sets the name of the field in the underlying data source.")]
		[Category("Data")]
		public string FieldName {
			get { return fieldName; }
			set {
				if (Caption == FieldName) Caption = null;
				fieldName = value;
				if (String.IsNullOrEmpty(Caption)) Caption = value;
			}
		}
		///<summary>Gets or sets a string used to format the column's value.</summary>
		[Description("Gets or sets a string used to format the column's value.")]
		[Category("Data")]
		[DefaultValue("{0}")]
		public string FormatString {
			get { return formatString; }
			set { formatString = value; }
		}

		internal override void OnDataSourceSet() {
			base.OnDataSourceSet();
			descriptor = Owner.ItemProperties.Cast<PropertyDescriptor>().FirstOrDefault(pd => pd.Name == FieldName);
		}
		PropertyDescriptor descriptor;

		///<summary>Gets the string displayed in this column for the given row.</summary>
		public override string GetValue(object row) {
			if (descriptor == null)
				return "(No matching column)";
			return String.Format(CultureInfo.CurrentCulture, FormatString, descriptor.GetValue(row));
		}

		///<summary>Creates a copy of this column that can be used with a different ItemSelector.</summary>
		protected internal override ResultColumn Copy() {
			return new DataSourceColumn {
				Caption = Caption,
				FieldName = FieldName,
				FormatString = FormatString,
				Visible = Visible,
				Width = Width
			};
		}
	}

	///<summary>A column that displays a value from an arbitrary function.</summary>
	public class CustomColumn : ResultColumn {
		readonly Func<object, string> getter;
		///<summary>Creates a CustomColumn that uses the given delegate.</summary>
		public CustomColumn(Func<object, string> getter) {
			if (getter == null) throw new ArgumentNullException("getter");

			this.getter = getter;
		}

		///<summary>Gets the string displayed in this column for the given row.</summary>
		public override string GetValue(object row) { return getter(row); }

		///<summary>Creates a copy of this column that can be used with a different ItemSelector.</summary>
		protected internal override ResultColumn Copy() {
			return new CustomColumn(getter) { Caption = Caption, Visible = Visible, Width = Width };
		}
	}
	///<summary>A column that displays a value from an arbitrary strongly-typed function.</summary>
	///<typeparam name="TItem">The type of the items displayed.</typeparam>
	public class CustomColumn<TItem> : CustomColumn {
		///<summary>Creates a CustomColumn that uses the given delegate.</summary>
		public CustomColumn(Func<TItem, string> getter) : base(o => getter((TItem)o)) { }
	}

}
