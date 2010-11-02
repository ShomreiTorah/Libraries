using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Accessibility;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using ShomreiTorah.Common;

#pragma warning disable 1591  //XML doc comments
namespace ShomreiTorah.WinForms.Controls.Lookup {
	///<summary>A control that allows the user to select an item from a list.</summary>
	[Description("A control that allows the user to select an item from a list.")]
	[ComplexBindingProperties("DataSource", "DataMember")]
	[ToolboxBitmap(typeof(ItemSelector), "Images.NewLookup.bmp")]
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

		internal IList AllItems { get; set; }

		#region Filter
		protected override void OnEditorKeyDown(KeyEventArgs e) {
			base.OnEditorKeyDown(e);
			if (e.Handled) return;
			if (!Char.IsControl((Char)e.KeyValue))
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

			ArrayList unsortedItems = null;
			if (String.IsNullOrEmpty(Text)) {
				FilterWords = EmptyStrings;
				if (Properties.SortComparer == null)
					CurrentItems = AllItems;	//If there is no sort and no filter, I can just use the original list.
				else
					unsortedItems = new ArrayList(AllItems);
			} else {
				FilterWords = new ReadOnlyCollection<string>(Text.Split(filterSplitChars, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(w => w.Length).ToArray());

				unsortedItems = new ArrayList(AllItems.Count);
				foreach (object item in AllItems)
					if (MeetsFilter(item)) unsortedItems.Add(item);

				if (Properties.SortComparer == null)
					CurrentItems = unsortedItems;
			}
			if (Properties.SortColumn != null) {
				unsortedItems.Sort(Properties.SortComparer);
				CurrentItems = unsortedItems;
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

		protected override void OnMouseWheel(MouseEventArgs e) {
			DXMouseEventArgs ee = DXMouseEventArgs.GetMouseArgs(e);
			try {
				base.OnMouseWheel(ee);
				if (ee.Handled) return;

				if (IsPopupOpen)
					PopupForm.OnEditorMouseWheel(ee);
			} finally { ee.Sync(); }
		}

		protected override void OnMaskBox_ValueChanged(object sender, EventArgs e) {
			//Suppress EditValue changes when editing the filter
			//base.OnMaskBox_ValueChanged(sender, e);
		}
		protected override void OnMaskBox_ValueChanging(object sender, ChangingEventArgs e) {
			//Suppress EditValue changes when editing the filter
			//base.OnMaskBox_ValueChanging(sender, e);
		}
		protected override void OnMouseDownClosePopup() {
			//Don't close the popup on a MouseDown in the editor
			//base.OnMouseDownClosePopup();
		}
		protected override void OnMaskBox_Click(object sender, EventArgs e) {
			base.OnMaskBox_Click(sender, e);
			ShowPopup();
		}
		protected override void DoShowPopup() {
			//Clear the filter textbox without
			//affecting the editor's EditValue
			MaskBox.SetEditValue("", "", true);
			RunFilter(force: true);
			base.DoShowPopup();
		}
		protected override void AcceptPopupValue(object val) {
			//DevExpress will occasionally close the popup
			//and accept a null value.  I don't want that.
			if (val != null) {
				base.AcceptPopupValue(val);
				IsModified = true;	//Force validation to commit the new value for databinding
				DoValidate();
			}
		}
		protected override void DoClosePopup(PopupCloseMode closeMode) {
			//Clear the filter textbox without
			//affecting the editor's EditValue
			MaskBox.SetEditValue("", "", true);
			base.DoClosePopup(closeMode);
		}

		internal bool PreventClose { get; set; }
		protected override void ClosePopup(PopupCloseMode closeMode) {
			if (!PreventClose)
				base.ClosePopup(closeMode);
		}
	}


	///<summary>Holds settings for an ItemSelector.</summary>
	[UserRepositoryItem("Register")]
	public class RepositoryItemItemSelector : RepositoryItemPopupBase {
		#region Editor Registration
		static RepositoryItemItemSelector() { Register(); }
		///<summary>Registers the RepositoryItem.</summary>
		public static void Register() {
			RegisterDerived(typeof(ItemSelector), typeof(RepositoryItemItemSelector));
		}

		///<summary>Gets the owning editor's type name.</summary>
		public override string EditorTypeName { get { return "ItemSelector"; } }
		///<summary>Gets the owning ItemSelector.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new ItemSelector OwnerEdit { get { return (ItemSelector)base.OwnerEdit; } }

		///<summary>Registers an inherited RepositoryItemItemSelector editor.</summary>
		protected static void RegisterDerived(Type editorType, Type repositoryType) {
			EditorRegistrationInfo.Default.Editors.Add(
				new EditorClassInfo(
					editorType.Name,
					editorType,
					repositoryType,
					typeof(ItemSelectorViewInfo),
					new ItemSelectorPainter(),
					true, EditImageIndexes.ButtonEdit,	//TODO: Icon
					typeof(PopupEditAccessible)
				)
			);
		}
		#endregion

		class InvalidatingColumnCollection : ItemSelectorColumnCollection {
			public InvalidatingColumnCollection(RepositoryItemItemSelector owner) : base(owner) { }

			void Invalidate() { Owner.OnPropertiesChanged(); }

			protected override void ClearItems() {
				base.ClearItems();
				Invalidate();
			}
			protected override void InsertItem(int index, ResultColumn item) {
				base.InsertItem(index, item);
				Invalidate();
			}
			protected override void RemoveItem(int index) {
				base.RemoveItem(index);
				Invalidate();
			}
			protected override void SetItem(int index, ResultColumn item) {
				base.SetItem(index, item);
				Invalidate();
			}
		}

		const string DefaultNullValuePrompt = "Click here to select an item, or type to search";

		object dataSource;
		string dataMember = "";
		bool showColumnHeaders = true;
		bool showVerticalLines = true;
		bool allowResize = true;
		Image selectionIcon;

		ResultColumn resultDisplayColumn;
		ResultColumn sortColumn;

		///<summary>Creates a new RepositoryItemItemSelector.</summary>
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Simple property setter")]
		public RepositoryItemItemSelector() {
			Columns = new ItemSelectorColumnCollection(this);
			AdditionalResultColumns = new InvalidatingColumnCollection(this);

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

			if (source.SortColumn == null)
				SortColumn = null;
			else
				SortColumn = source.SortColumn.Copy();
			if (source.ResultDisplayColumn == null)
				ResultDisplayColumn = null;
			else
				ResultDisplayColumn = source.ResultDisplayColumn.Copy();
			AdditionalResultColumns.AddRange(source.AdditionalResultColumns.Select(c => c.Copy()));

			SelectionIcon = source.SelectionIcon;
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

		#region Basic Properties
		///<summary>Gets the columns displayed in the results grid.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ItemSelectorColumnCollection Columns { get; private set; }
		///<summary>Gets columns that display additional information about the selected item.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ItemSelectorColumnCollection AdditionalResultColumns { get; private set; }
		///<summary>Gets or sets the column to display in the editor when a value is selected.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ResultColumn ResultDisplayColumn {
			get { return resultDisplayColumn; }
			set {
				resultDisplayColumn = value;
				if (value != null)
					value.SetOwner(this);
				OnPropertiesChanged();
			}
		}
		///<summary>Gets or sets the column to to sort the results by.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ResultColumn SortColumn {
			get { return sortColumn; }
			set {
				sortColumn = value;
				if (value != null)
					value.SetOwner(this);
				SortComparer = value == null ? null : new ColumnComparer(value);
				OnPropertiesChanged();
			}
		}
		///<summary>Gets the comparer to sort the results, if any.</summary>
		internal IComparer SortComparer { get; private set; }
		class ColumnComparer : IComparer {
			readonly ResultColumn column;
			public ColumnComparer(ResultColumn column) { this.column = column; }
			public int Compare(object x, object y) {
				if (Equals(x, y)) return 0;
				if (x == null) return -1;
				if (y == null) return -1;
				return Comparer.Default.Compare(column.GetValue(x), column.GetValue(y));
			}
		}

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
		///<summary>Gets or sets the icon displayed in the editor when an item is selected.</summary>
		[Description("Gets or sets the icon displayed in the editor when an item is selected.")]
		[Category("Appearance")]
		[DefaultValue(null)]
		public Image SelectionIcon {
			get { return selectionIcon; }
			set { selectionIcon = value; OnPropertiesChanged(); }
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

			SetEditorData();
		}
		///<summary>Sets the owning edit's DataSource.  This method must be called when initialization is complete.</summary>
		void SetEditorData() {
			if (IsLoading || OwnerEdit == null || OwnerEdit.BindingContext == null)
				return;
			if (DataSource == null) {
				OwnerEdit.AllItems = new object[0];
				return;
			}

			ResultsBindingManager = OwnerEdit.BindingContext[DataSource, DataMember];
			ItemProperties = ResultsBindingManager.GetItemProperties();
			OwnerEdit.AllItems = (IList)ListBindingHelper.GetList(DataSource, DataMember);
			Columns.OnDataSourceSet();
			AdditionalResultColumns.OnDataSourceSet();
			if (ResultDisplayColumn != null)
				ResultDisplayColumn.SetOwner(this);
			if (SortColumn != null)
				SortColumn.SetOwner(this);
		}
		#endregion
		///<summary>Called when the repository item is connected to an editor control.</summary>
		protected override void OnOwnerEditChanged() {
			base.OnOwnerEditChanged();
			SetEditorData();
		}
		///<summary>Notifies the editor that the initialization has been completed.</summary>
		public override void EndInit() {
			base.EndInit();
			SetEditorData();
		}

		#region ItemSelecting Event
		///<summary>Raises the ItemSelecting event.</summary>
		///<param name="newVal">The item that is being selected.</param>
		///<returns>False if the selection was cancelled.</returns>
		internal bool RaiseItemSelecting(object newVal) {
			var args = new ItemSelectingEventArgs(newVal);

			try {	//Wrap OnItemSelecting overrides 
				OwnerEdit.PreventClose = true;
				OnItemSelecting(args);
			} finally { OwnerEdit.PreventClose = false; }
			return !args.Cancel;
		}

		///<summary>Occurs when the user selects an item in the results grid.</summary>
		public event EventHandler<ItemSelectingEventArgs> ItemSelecting;
		///<summary>Raises the ItemSelecting event.</summary>
		///<param name="e">A ItemSelectingEventArgs object that provides the event data.</param>
		protected virtual void OnItemSelecting(ItemSelectingEventArgs e) {
			if (ItemSelecting != null) ItemSelecting(GetEventSender(), e);
		}
		#endregion
	}
	sealed class ItemSelectorViewInfo : PopupBaseEditViewInfo {
		public new RepositoryItemItemSelector Item { get { return (RepositoryItemItemSelector)base.Item; } }
		public ItemSelectorViewInfo(RepositoryItem item)
			: base(item) {
			CreateAppearances();
		}

		#region Appearances
		///<summary>Gets the appearance used to paint the result value.</summary>
		public AppearanceObject AppearanceResult { get; private set; }
		///<summary>Gets the appearance used to paint additional columns of the result value.</summary>
		public AppearanceObject AppearanceResultInfo { get; private set; }

		///<summary>Creates appearance objects used by the editor.</summary>
		///<remarks>Any properties that will never change (such as alignment) should be set here.</remarks>
		void CreateAppearances() {
			AppearanceResult = new AppearanceObject {
				Font = new Font(Appearance.GetFont(), FontStyle.Bold)
			};
			AppearanceResult.TextOptions.VAlignment = VertAlignment.Center;

			AppearanceResultInfo = new AppearanceObject();
			AppearanceResultInfo.TextOptions.VAlignment = VertAlignment.Center;
			AppearanceResultInfo.TextOptions.Trimming = Trimming.EllipsisCharacter;
		}

		public override void UpdatePaintAppearance() {
			base.UpdatePaintAppearance();

			//Update skin-dependant properties here.
			AppearanceResult.ForeColor = OwnerEdit.ForeColor;
			AppearanceResultInfo.ForeColor = base.NullValuePromptForeColor;
		}
		#endregion

		///<summary>Indicates whether the editor should display the selected item or a normal textbox.</summary>
		public bool DrawSelectedItem { get; private set; }
		///<summary>Gets the icon to draw in the editor area, if any.</summary>
		public Image Image { get; private set; }
		///<summary>Gets the result value to display in the editor.</summary>
		public string ResultCaption { get; private set; }
		///<summary>Gets additional information about the result value.</summary>
		public ReadOnlyCollection<string> AdditionalCaptions { get; private set; }

		///<summary>Gets the SkinElement used to draw the selected item in the editor.</summary>
		public SkinElement SelectionBackgroundElement { get; private set; }
		///<summary>Gets the bounds of the icon to draw in the editor area.</summary>
		public Rectangle ImageBounds { get; private set; }
		///<summary>Gets the rectangle in which to draw the selected item.</summary>
		public Rectangle SelectionBounds { get; private set; }

		protected override void CalcContentRect(Rectangle bounds) {
			base.CalcContentRect(bounds);

			SelectionBackgroundElement = CommonSkins.GetSkin(LookAndFeel)[CommonSkins.SkinLayoutItemBackground];

			if (OwnerEdit.EditValue == null || OwnerEdit.EditValue == DBNull.Value || Item.SelectionIcon == null) {
				Image = null;
				ImageBounds = new Rectangle(ContentRect.Location, Size.Empty);
				DrawSelectedItem = false;
			} else {
				Image = Item.SelectionIcon;
				//TODO: Scale image to fit editor?
				const int ImageHeight = 16;
				ImageBounds = new Rectangle(
					ContentRect.X,
					ClientRect.Y + (ClientRect.Height - ImageHeight) / 2 - 1,
					Image.Width * (Image.Height / ImageHeight),
					ImageHeight
				);
				fMaskBoxRect.X += ImageBounds.Width + 2;
				fMaskBoxRect.Width -= ImageBounds.Width + 2;
			}

			//If an item is selected and the user is not in the middle
			//of selecting another one, draw the selected item instead
			//of the MaskBox.  Instead of hiding the maskbox, I shrink
			//to not occupy any space, while leaving it focused.  This
			//allows the user to start typing to re-show the popup.
			if (OwnerEdit.EditValue != null && OwnerEdit.EditValue != DBNull.Value && !OwnerEdit.IsPopupOpen) {
				DrawSelectedItem = true;
				SelectionBounds = Rectangle.Inflate(base.MaskBoxRect, 0, 1);
				fMaskBoxRect = Rectangle.Empty;

				if (Item.ResultDisplayColumn == null)
					ResultCaption = OwnerEdit.EditValue.ToString();
				else
					ResultCaption = Item.ResultDisplayColumn.GetValue(OwnerEdit.EditValue);
				AdditionalCaptions = Item.AdditionalResultColumns.Select(c => c.GetValue(OwnerEdit.EditValue)).ReadOnlyCopy();
			} else {
				DrawSelectedItem = false;
			}
		}
	}
	class ItemSelectorPainter : ButtonEditPainter {
		protected override void DrawTextBoxArea(ControlGraphicsInfoArgs info) {
			var vi = (ItemSelectorViewInfo)info.ViewInfo;

			if (vi.DrawSelectedItem)
				vi.PaintAppearance.FillRectangle(info.Cache, vi.ContentRect);	//The native painter won't paint any background in the middle, since it expects the MaskBox to be there
			else
				base.DrawTextBoxArea(info);
		}
		public override void Draw(ControlGraphicsInfoArgs info) {
			base.Draw(info);
			//I need to draw after the base draws everything, since I overlap some of the padding, which it fills with background.
			var vi = (ItemSelectorViewInfo)info.ViewInfo;

			if (vi.Image != null)
				DrawIcon(info);

			if (vi.DrawSelectedItem)
				DrawSelection(info);
		}

		static void DrawIcon(ControlGraphicsInfoArgs args) {
			var info = (ItemSelectorViewInfo)args.ViewInfo;

			args.Graphics.DrawImage(info.Image, info.ImageBounds);
		}

		static void DrawSelection(ControlGraphicsInfoArgs args) {
			var info = (ItemSelectorViewInfo)args.ViewInfo;

			var selectionInfo = new SkinElementInfo(info.SelectionBackgroundElement, info.SelectionBounds) {
				ImageIndex = 1,
				Cache = args.Cache
			};

			SkinElementPainter.Default.DrawObject(selectionInfo);
			args.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			Rectangle textArea = Rectangle.Inflate(info.SelectionBounds, -2, 0);

			info.AppearanceResult.DrawString(args.Cache, info.ResultCaption, textArea);

			var textWidth = 8 + (int)Math.Ceiling(info.AppearanceResult.CalcTextSize(args.Cache, info.ResultCaption, textArea.Width).Width);
			textArea.Width -= textWidth;
			textArea.X += textWidth;

			foreach (var caption in info.AdditionalCaptions) {
				info.AppearanceResultInfo.DrawString(args.Cache, caption, textArea);

				textWidth = 4 + (int)Math.Ceiling(info.AppearanceResultInfo.CalcTextSize(args.Cache, caption, textArea.Width).Width);
				if (textWidth > textArea.Width) break;
				textArea.Width -= textWidth;
				textArea.X += textWidth;
			}
		}
	}

	///<summary>Provides data for ItemSelecting events.</summary>
	public class ItemSelectingEventArgs : CancelEventArgs {
		///<summary>Creates a new ItemSelectingEventArgs instance.</summary>
		public ItemSelectingEventArgs(object newItem) { NewItem = newItem; }

		///<summary>Gets the item that was selected in the results grid.</summary>
		public object NewItem { get; private set; }
	}

	///<summary>Contains the columns in an ItemSelector.</summary>
	public class ItemSelectorColumnCollection : Collection<ResultColumn> {
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
