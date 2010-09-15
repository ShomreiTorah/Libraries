using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Accessibility;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using ShomreiTorah.Common;

#pragma warning disable 1591
namespace ShomreiTorah.WinForms.Controls.Lookup {
	///<summary>A control that allows the user to select an item from a list.</summary>
	[DefaultEvent("ItemSelected")]
	[Description("A control that allows the user to select an item from a list.")]
	[ComplexBindingProperties("DataSource", "DataMember")]
	public class ItemSelector : PopupBaseEdit {
		static readonly ReadOnlyCollection<string> EmptyStrings = new ReadOnlyCollection<string>(new string[0]);

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ItemSelector() { RepositoryItemItemSelector.Register(); }

		///<summary>Creates a new ItemSelector.</summary>
		public ItemSelector() {
			FilterWords = EmptyStrings;
		}

		///<summary>Gets the editor's type name.</summary>
		public override string EditorTypeName { get { return "ItemSelector"; } }

		///<summary>Gets settings specific to the ItemSelector.</summary>
		[Category("Properties")]
		[Description("Gets an object containing properties, methods and events specific to the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new RepositoryItemItemSelector Properties { get { return (RepositoryItemItemSelector)base.Properties; } }

		[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "For internal use only")]
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
			MaskBox.SetEditValue("", "", true);
			RunFilter(force: true);
			base.DoShowPopup();
		}
		protected override void OnPopupClosed(PopupCloseMode closeMode) {
			UpdateDisplayText();	//Clear the filter text
			base.OnPopupClosed(closeMode);
		}
		protected override void OnMouseWheel(MouseEventArgs e) {
			DXMouseEventArgs ee = DXMouseEventArgs.GetMouseArgs(e);
			try {
				base.OnMouseWheel(ee);
				if (ee.Handled) return;

				if (IsPopupOpen)
					PopupForm.OnEditorMouseWheel(ee);
			} finally { ee.Sync(); }
		}

		protected override void SetEmptyEditValue(object emptyEditValue) {
			base.SetEmptyEditValue(emptyEditValue);
			//TODO: Draw selected item.  (ViewInfo / Painter?)
		}

		internal IList AllItems { get; set; }

		#region Filter
		protected override void OnEditorKeyDown(KeyEventArgs e) {
			base.OnEditorKeyDown(e);
			if (e.Handled) return;
			ShowPopup();
		}
		protected override void OnEditorKeyUp(KeyEventArgs e) {
			base.OnEditorKeyUp(e);
			RunFilter();
		}

		internal IList CurrentItems { get; private set; }
		internal ReadOnlyCollection<string> FilterWords { get; private set; }

		static readonly char[] filterSplitChars = new[] { ' ' };
		string lastFilterText;
		void RunFilter(bool force = false) {
			if (!force && Text.Equals(lastFilterText, StringComparison.CurrentCultureIgnoreCase)) return;
			if (String.IsNullOrEmpty(Text)) {
				FilterWords = EmptyStrings;
				CurrentItems = AllItems;
			} else {
				FilterWords = new ReadOnlyCollection<string>(Text.Split(filterSplitChars, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(w => w.Length).ToArray());
				CurrentItems = AllItems.Cast<object>().Where(MeetsFilter).ToArray();
			}

			lastFilterText = Text;
			if (PopupForm != null)	//This function is called just before the popup is shown
				PopupForm.RefreshItems();
		}
		bool MeetsFilter(object item) {
			//For each column, remove the longest word that matches the column.
			//If every word was matched, the item passes.

			var unmetStrings = new List<string>(FilterWords);

			foreach (var column in Properties.Columns.Where(c => c.ShouldFilter)) {
				var columnValue = column.GetValue(item);
				unmetStrings.Remove(unmetStrings.FirstOrDefault(fw => ValueMatches(filterWord: fw, columnValue: columnValue)));
			}

			return unmetStrings.Count == 0;
		}
		internal static bool ValueMatches(string filterWord, string columnValue) {
			//If it ever becomes possible to match a different number of characters,
			//the painter will need to be made aware of the difference in lengths.
			return columnValue.Replace(' ', '-').StartsWith(filterWord, StringComparison.CurrentCultureIgnoreCase);
		}
		#endregion

		protected override void OnMaskBox_ValueChanged(object sender, EventArgs e) {
			//Suppress EditValue changes
			//base.OnMaskBox_ValueChanged(sender, e);
		}
		protected override void OnMaskBox_ValueChanging(object sender, ChangingEventArgs e) {
			//Suppress EditValue changes
			//base.OnMaskBox_ValueChanging(sender, e);
		}
		protected override void OnMaskBox_Click(object sender, EventArgs e) {
			base.OnMaskBox_Click(sender, e);
			ShowPopup();
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

		const string DefaultNullValuePrompt = "Click here to select an item, or type to search";

		object dataSource;
		string dataMember = "";
		bool showColumnHeaders = true;
		bool showVerticalLines = true;
		bool allowResize = true;

		///<summary>Creates a new RepositoryItemItemSelector.</summary>
		public RepositoryItemItemSelector() {
			Columns = new ItemSelectorColumnCollection(this);

			AppearanceColumnHeader = CreateAppearance("ColumnHeader");
			AppearanceMatch = CreateAppearance("Match");
			NullValuePrompt = DefaultNullValuePrompt;

			UserPopupHeight = 300;
		}

		public override void Assign(RepositoryItem item) {
			base.Assign(item);
			var source = (RepositoryItemItemSelector)item;

			Columns.Clear();
			Columns.AddRange(source.Columns.Select(c => c.Copy()));	//The InsertItem overload will set the source.

			UserPopupHeight = source.UserPopupHeight;
			AllowResize = source.AllowResize;
			ShowColumnHeaders = source.ShowColumnHeaders;
			ShowVerticalLines = source.ShowVerticalLines;

			AppearanceColumnHeader.Assign(source.AppearanceColumnHeader);
			AppearanceMatch.Assign(source.AppearanceMatch);
			UpdateDataSource(source.DataSource, source.DataMember);
		}

		///<summary>Gets or sets the popup form's desired height, as set by the user.</summary>
		internal int UserPopupHeight { get; set; }
		//    get { return PopupFormSize.Height; }
		//    set { PopupFormSize = new Size(PopupFormSize.Width, value); }
		//}

		#region Appearances
		protected override void DestroyAppearances() {
			DestroyAppearance(AppearanceColumnHeader);
			DestroyAppearance(AppearanceMatch);
			base.DestroyAppearances();
		}

		void ResetAppearanceMatch() { AppearanceMatch.Reset(); }
		bool ShouldSerializeAppearanceMatch() { return AppearanceMatch.ShouldSerialize(); }
		///<summary>Gets or sets the appearance of the matched prefixes in the results grid.</summary>
		[Description("Gets or sets the appearance of the matched prefixes in the results grid.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public AppearanceObject AppearanceMatch { get; private set; }

		void ResetAppearanceColumnHeader() { AppearanceColumnHeader.Reset(); }
		bool ShouldSerializeAppearanceColumnHeader() { return AppearanceColumnHeader.ShouldSerialize(); }
		///<summary>Gets or sets the appearance of the column headers.</summary>
		[Description("Gets or sets the appearance of the column headers.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public AppearanceObject AppearanceColumnHeader { get; private set; }
		#endregion

		///<summary>Gets the columns displayed in the results grid.</summary>
		[Description("Gets the columns displayed in the results grid.")]
		[Category("Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ItemSelectorColumnCollection Columns { get; private set; }

		///<summary>Gets or sets whether the results grid will display column headers.</summary>
		[Description("Gets or sets whether the results grid will display column headers.")]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool ShowColumnHeaders {
			get { return showColumnHeaders; }
			set { showColumnHeaders = value; }
		}
		///<summary>Gets or sets whether to draw vertical gridlines in the results grid.</summary>
		[Description("Gets or sets whether to draw vertical gridlines in the results grid.")]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool ShowVerticalLines {
			get { return showVerticalLines; }
			set { showVerticalLines = value; }
		}
		///<summary>Gets or sets whether the end-user can resize the popup form.</summary>
		[Description("Gets or sets whether the end-user can resize the popup form.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool AllowResize {
			get { return allowResize; }
			set {
				if (AllowResize == value) return;
				allowResize = value;
				OnPropertiesChanged();	//Force a new ViewInfo, since I read this property in the constructor.
			}
		}

		///<summary>Gets or sets the text displayed grayed out when the editor doesn't have focus, and its edit value is not set to a valid value.</summary>
		[Description("Gets or sets the text displayed grayed out when the editor doesn't have focus, and its edit value is not set to a valid value.")]
		[Category("Behavior")]
		[DefaultValue(DefaultNullValuePrompt)]
		[Localizable(true)]
		public override string NullValuePrompt {
			get { return base.NullValuePrompt; }
			set { base.NullValuePrompt = value; }
		}

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
			OwnerEdit.AllItems = (IList)ListBindingHelper.GetList(DataSource, DataMember);
		}
		#endregion
		#region Suppressed Properties
		///<summary>This property is irrelevant for this control.</summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override DevExpress.XtraEditors.Mask.MaskProperties Mask {
			get { return base.Mask; }
		}
		///<summary>This property is irrelevant for this control.</summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override EditValueChangedFiringMode EditValueChangedFiringMode {
			get { return EditValueChangedFiringMode.Default; }
			set { }
		}
		///<summary>This property is irrelevant for this control.</summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override TextEditStyles TextEditStyle {
			get { return TextEditStyles.Standard; }
			set { }
		}
		#endregion

		#region Events
		///<summary>Raises the ItemSelecting event.</summary>
		///<param name="newVal">The item that is being selected.</param>
		///<returns>False if the selection was cancelled.</returns>
		internal bool RaiseItemSelecting(object newVal) {
			var args = new ItemSelectingEventArgs(newVal);
			OnItemSelecting(args);
			return !args.Cancel;
		}

		///<summary>Occurs when the user selects an item in the results grid.</summary>
		public event EventHandler<ItemSelectingEventArgs> ItemSelecting;
		///<summary>Raises the ItemSelecting event.</summary>
		///<param name="e">A ItemSelectingEventArgs object that provides the event data.</param>
		internal protected virtual void OnItemSelecting(ItemSelectingEventArgs e) {
			if (ItemSelecting != null)
				ItemSelecting(GetEventSender(), e);
		}
		#endregion
	}
	///<summary>Provides data for ItemSelecting events.</summary>
	public class ItemSelectingEventArgs : CancelEventArgs {
		///<summary>Creates a new ItemSelectingEventArgs instance.</summary>
		public ItemSelectingEventArgs(object newItem) { NewItem = newItem; }

		///<summary>Gets the item that was selected in the results grid.</summary>
		public object NewItem { get; private set; }
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
