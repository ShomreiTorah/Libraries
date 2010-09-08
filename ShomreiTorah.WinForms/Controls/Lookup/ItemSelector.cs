using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Accessibility;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;

namespace ShomreiTorah.WinForms.Controls.Lookup {
	///<summary>A control that allows the user to select an item from a list.</summary>
	[DefaultEvent("ItemSelected")]
	[Description("A control that allows the user to select an item from a list.")]
	[ComplexBindingProperties("DataSource", "DataMember")]
	public class ItemSelector : PopupBaseEdit {

		///<summary>Creates a new ItemSelector.</summary>
		public ItemSelector() {
		}

		///<summary>Gets the editor's type name.</summary>
		public override string EditorTypeName { get { return "ItemSelector"; } }

		///<summary>Gets settings specific to the ItemSelector.</summary>
		[Category("Properties")]
		[Description("Gets an object containing properties, methods and events specific to the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new RepositoryItemItemSelector Properties { get { return (RepositoryItemItemSelector)base.Properties; } }

		internal new ItemSelectorPopupForm PopupForm { get { return (ItemSelectorPopupForm)base.PopupForm; } }

		///<summary>Gets or sets the text displayed in the textbox.</summary>
		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}


		///<summary>Creates the editor's popup form.</summary>
		protected override PopupBaseForm CreatePopupForm() {
			return new ItemSelectorPopupForm(this);
		}

		protected override void DoShowPopup() {
			base.DoShowPopup();
		}
		protected override void OnPopupClosed(PopupCloseMode closeMode) {
			base.OnPopupClosed(closeMode);
		}
		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel(e);
			//TODO: Scroll popup
		}
		protected override bool DoSpin(bool isUp) {
			return base.DoSpin(isUp);
		}
		protected override void SetEmptyEditValue(object emptyEditValue) {
			base.SetEmptyEditValue(emptyEditValue);
		}
	}


	///<summary>Holds settings for an ItemSelector.</summary>
	[UserRepositoryItem("Register")]
	public class RepositoryItemItemSelector : RepositoryItemPopupBase {
		#region Editor Registration
		static RepositoryItemItemSelector() { Register(); }
		///<summary>Registers the RepositoryItem.</summary>
		public static void Register() {
			EditorRegistrationInfo.Default.Editors.Add(
				new EditorClassInfo(
					"ItemSelector",
					typeof(ItemSelector),
					typeof(RepositoryItemItemSelector),
					typeof(PopupBaseEditViewInfo),		//TODO: ViewInfo
					new ButtonEditPainter(),			//TODO: Painter
					true, EditImageIndexes.ButtonEdit,	//TODO: Icon
					typeof(PopupEditAccessible)
				)
			);
		}
		///<summary>Gets the owning editor's type name.</summary>
		public override string EditorTypeName { get { return "ItemSelector"; } }
		///<summary>Gets the owning ItemSelector.</summary>
		public new ItemSelector OwnerEdit { get { return (ItemSelector)base.OwnerEdit; } }
		#endregion

		object dataSource;
		string dataMember = "";

		///<summary>Creates a new RepositoryItemItemSelector.</summary>
		public RepositoryItemItemSelector() {
			Columns = new ItemSelectorColumnCollection(this);
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

			ResultsBindingManager = OwnerEdit.BindingContext[DataSource, DataMember];
			ItemProperties = ResultsBindingManager.GetItemProperties();
			Columns.OnDataSourceSet();
		}
		#endregion
	}

	///<summary>Contains the columns in an ItemSelector.</summary>
	public sealed class ItemSelectorColumnCollection : Collection<ResultColumn> {
		internal ItemSelectorColumnCollection(RepositoryItemItemSelector owner) {
			Owner = owner;
		}

		///<summary>Gets the RepositoryItemItemSelector that contains the columns.</summary>
		public RepositoryItemItemSelector Owner { get; private set; }

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
