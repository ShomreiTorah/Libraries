using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Singularity {
	///<summary>A column in a Singularity table.</summary>
	public class Column {
		/// <summary>Initializes a new instance of the <see cref="Column"/> class.</summary>
		public Column(string name, Type dataType) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (dataType == null) throw new ArgumentNullException("dataType");

			Name = name; DataType = dataType;
		}

		///<summary>Gets or sets the name of the column.</summary>
		public string Name { get; set; }

		///<summary>Gets or sets the data-type of the column.</summary>
		public Type DataType { get; set; }

		///<summary>Gets or sets the default value of the column.</summary>
		public object DefaultValue { get; set; }
	}

	///<summary>A collection of Column objects.</summary>
	public class ColumnCollection : Collection<Column> {
		///<summary>Inserts an element into the ColumnCollection at the specified index.</summary>
		protected override void InsertItem(int index, Column item) {
			if (item == null) throw new ArgumentNullException("item");
			if (this[item.Name] != null) throw new ArgumentException("A column named " + item.Name + " already exists.");

			base.InsertItem(index, item);
		}

		///<summary>Gets the column with the given name, or null if there is no column with that name.</summary>
		public Column this[string name] { get { return this.FirstOrDefault(c => c.Name == name); } }
	}
}
