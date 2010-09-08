using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.XtraEditors;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using DevExpress.XtraEditors.Popup;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	///<summary>A control that allows the user to select an item from a list.</summary>
	[DefaultEvent("ItemSelected")]
	[Description("A control that allows the user to select an item from a list.")]
	[ComplexBindingProperties("DataSource", "DataMember")]
	public class ItemSelector : PopupBaseEdit {
		readonly ResultsList resultsControl;

		object dataSource;
		string dataMember = "";

		///<summary>Creates a new ItemSelector.</summary>
		public ItemSelector() {
			Columns = new ItemSelectorColumnCollection(this);
			resultsControl = new ResultsList(this);
		}

		///<summary>Gets the columns displayed in the results grid.</summary>
		[Description("Gets the columns displayed in the results grid.")]
		[Category("Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ItemSelectorColumnCollection Columns { get; private set; }

		#region DataSource
		///<summary>Gets or sets the name of the list or table in the data source to display data in.</summary>
		[Description("Gets or sets the name of the list or table in the data source to display data in.")]
		[Category("Data")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
		[DefaultValue("")]
		public string DataMember {
			get { return dataMember; }
			set { UpdateDataSource(DataSource, value ?? ""); }
		}

		///<summary>Gets or sets the data source displayed by this lookup.</summary>
		[Description("Gets or sets the data source displayed by this lookup.")]
		[Category("Data")]
		[AttributeProvider(typeof(IListSource))]
		[DefaultValue(null)]
		public object DataSource {
			get { return dataSource; }
			set { UpdateDataSource(value, DataMember); }
		}

		internal BindingManagerBase ResultsBindingManager { get; private set; }
		internal PropertyDescriptorCollection ItemProperties { get; private set; }

		private void UpdateDataSource(object source, string member) {
			if (source == DataSource && member == DataMember) return;
			dataSource = source; dataMember = member;

			if (source == null) {
				//TODO: Set state?
				return;
			}

			ResultsBindingManager = BindingContext[DataSource, DataMember];
			ItemProperties = ResultsBindingManager.GetItemProperties();
			Columns.OnDataSourceSet();
		}
		#endregion

		///<summary>Creates the editor's popup form.</summary>
		protected override PopupBaseForm CreatePopupForm() {
			throw new NotImplementedException();
		}
	}
	///<summary>Contains the columns in an ItemSelector.</summary>
	public sealed class ItemSelectorColumnCollection : Collection<ResultColumn> {
		internal ItemSelectorColumnCollection(ItemSelector owner) {
			Owner = owner;
		}

		///<summary>Gets the ItemSelector that contains the columns.</summary>
		public ItemSelector Owner { get; private set; }

		///<summary>Inserts an element into the Collection at the specified index.</summary>
		protected override void InsertItem(int index, ResultColumn item) {
			base.InsertItem(index, item);
			item.SetOwner(Owner);
		}
		///<summary>Replaces the element at the specified index.</summary>
		protected override void SetItem(int index, ResultColumn item) {
			base.SetItem(index, item);
			item.SetOwner(Owner);
		}

		internal void OnDataSourceSet() {
			foreach (var col in this) {
				col.OnDataSourceSet();
			}
		}
	}
}
