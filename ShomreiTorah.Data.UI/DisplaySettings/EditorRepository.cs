using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms.Controls.Lookup;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Contains RepositoryItem presets.</summary>
	public static partial class EditorRepository {
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Annoying")]
		static EditorRepository() {
			//This file should create reusable presets.
			//Column registration is done in SettingsRegistrator.
			#region Editor Presets
			CurrencyEditor = new EditorSettings<RepositoryItemSpinEdit>(properties => {
				properties.Increment = 10;
				properties.DisplayFormat.FormatType = FormatType.Numeric;
				properties.DisplayFormat.FormatString = "c";
				properties.EditFormat.Assign(properties.DisplayFormat);
				properties.Mask.EditMask = "c";
			});
			CommentsPopupEditor = new EditorSettings<RepositoryItemMemoExEdit>(properties => {
				properties.ShowIcon = false;
			});
			AccountEditor = new ComboBoxSettings(Names.AccountNames);
			PaymentMethodEditor = new ComboBoxSettings(Names.PaymentMethods);
			RelationListEditor = new ComboBoxSettings(Names.RelationNames);
			MelaveMalkaSourceEditor = new ComboBoxSettings(Names.MelaveMalkaSources, p => p.TextEditStyle = TextEditStyles.DisableTextEditor);

			StateEditor = new ComboBoxSettings(Names.CommonStates.Concat(new[] { "Canada", "Israel" }).Concat(Names.StateAbbreviations));
			ZipEditor = new EditorSettings<RepositoryItemTextEdit>(properties => {
				properties.MaxLength = 10;
			});
			PhoneEditor = new EditorSettings<RepositoryItemTextEdit>(properties => {
				properties.Mask.EditMask = @"\(\d\d\d\) \d\d\d - \d\d\d\d";
				properties.Mask.MaskType = MaskType.RegEx;
				properties.Mask.ShowPlaceHolders = false;
			});

			TicketIdEditor = new EditorSettings<RepositoryItemSpinEdit>(properties => {
				properties.MinValue = 0;
				properties.MaxValue = int.MaxValue / 2;
				properties.IsFloatValue = false;
				properties.Mask.EditMask = "N0";
			});
			OptionalSeatEditor = new EditorSettings<RepositoryItemSpinEdit>(properties => {
				properties.AllowNullInput = DefaultBoolean.True;
				properties.NullText = "Not sure";
				properties.MinValue = 0;
				properties.MaxValue = 9;
			});
			#endregion
			PersonLookup = new MutableEditorSettings<RepositoryItemItemSelector>();
			PersonOwnedLookup = new MutableEditorSettings<RepositoryItemItemSelector>();
		}

		///<summary>Gets the EditorSettings for a comments field in a grid.</summary>
		public static EditorSettings<RepositoryItemMemoExEdit> CommentsPopupEditor { get; private set; }
		///<summary>Gets the EditorSettings for a currency field.</summary>
		public static EditorSettings<RepositoryItemSpinEdit> CurrencyEditor { get; private set; }
		///<summary>Gets the EditorSettings for the Billing Account field.</summary>
		public static ComboBoxSettings AccountEditor { get; private set; }
		///<summary>Gets the EditorSettings for the payment method field.</summary>
		public static ComboBoxSettings PaymentMethodEditor { get; private set; }

		///<summary>Gets the EditorSettings for the combobox of relative types.</summary>
		public static ComboBoxSettings RelationListEditor { get; private set; }

		///<summary>Gets the EditorSettings for the Source field for the Melave Malka.</summary>
		public static ComboBoxSettings MelaveMalkaSourceEditor { get; private set; }
		///<summary>Gets the EditorSettings for the Melave Malka seating fields.</summary>
		public static EditorSettings<RepositoryItemSpinEdit> OptionalSeatEditor { get; private set; }

		///<summary>Gets the EditorSettings for the raffle's TicketID field.</summary>
		public static EditorSettings<RepositoryItemSpinEdit> TicketIdEditor { get; private set; }

		///<summary>Gets the EditorSettings for a US State field.</summary>
		public static ComboBoxSettings StateEditor { get; private set; }
		///<summary>Gets the EditorSettings for a ZIP code field.</summary>
		public static EditorSettings<RepositoryItemTextEdit> ZipEditor { get; private set; }
		///<summary>Gets the EditorSettings for a phone number field.</summary>
		public static EditorSettings<RepositoryItemTextEdit> PhoneEditor { get; private set; }

		///<summary>Gets the EditorSettings for a lookup displaying the master directory.</summary>
		public static MutableEditorSettings<RepositoryItemItemSelector> PersonLookup { get; private set; }
		///<summary>Gets the EditorSettings for a lookup displaying a child table of the master directory.</summary>
		public static MutableEditorSettings<RepositoryItemItemSelector> PersonOwnedLookup { get; private set; }

		///<summary>Gets the number of behavior registrations,</summary>
		///<remarks>This property is used by the grid to verify design-time
		///registrations.  See <see cref="ShomreiTorah.Data.UI.Grid.SmartGrid.RegistrationCount"/>.</remarks>
		internal static int RegistrationCount { get { return dictionary.Count; } }
		static readonly Dictionary<Column, IEditorSettings> dictionary = new Dictionary<Column, IEditorSettings>();

		///<summary>Registers an IEditorSettings preset for a column in a Singularity schema.</summary>
		public static void Register(Column column, IEditorSettings settings) {
			if (column == null) throw new ArgumentNullException("column");
			if (settings == null) throw new ArgumentNullException("settings");
			UIThread.Verify();
			dictionary.Add(column, settings);
		}
		///<summary>Registers an IEditorSettings preset for one or more columns in Singularity schemas.</summary>
		public static void Register(IEnumerable<Column> columns, IEditorSettings settings) {
			if (columns == null) throw new ArgumentNullException("columns");

			foreach (var column in columns)
				Register(column, settings);
		}

		///<summary>Gets an IEditorSettings to use for the given column, or null if there is no preset for that column.</summary>
		public static IEditorSettings GetSettings(object dataSource, string columnName) {
			UIThread.Verify();

			var schema = TableSchema.GetSchema(dataSource);
			if (schema == null) return null;
			var column = schema.Columns[columnName];
			if (column == null) return null;	//eg, unbound columns

			return GetSettings(column);
		}
		///<summary>Gets an IEditorSettings to use for the given grid column, or null if there is no preset for that column.</summary>
		public static IEditorSettings GetSettings(GridColumn gridColumn) {
			var column = gridColumn.GetSchemaColumn();
			if (column == null) return null;	//eg, unbound columns
			return GetSettings(column);
		}
		///<summary>Gets an IEditorSettings to use for the given Singularity schema column, or null if there is no preset for that column.</summary>
		public static IEditorSettings GetSettings(Column column) {
			IEditorSettings retVal;
			dictionary.TryGetValue(column, out retVal);
			return retVal;
		}
	}

	///<summary>Contains a preset RepositoryItem.</summary>
	public interface IEditorSettings {
		///<summary>Gets the type of RepositoryItem that these settings apply to.</summary>
		Type ItemType { get; }

		///<summary>Creates a new RepositoryItem pre-configured for this instance's settings.</summary>
		RepositoryItem CreateItem();
		///<summary>Configures an existing RepositoryItem with this instance's settings.</summary>
		void Apply(RepositoryItem item);
	}
	///<summary>Contains a preset RepositoryItem.</summary>
	public class EditorSettings<TRepositoryItem> : IEditorSettings where TRepositoryItem : RepositoryItem, new() {
		///<summary>Creates a new EditorSettings without an application delegate.</summary>
		protected EditorSettings() { }

		readonly Action<TRepositoryItem> applier;

		///<summary>Creates an EditorSettings instance.</summary>
		///<param name="applier">A delegate that applies the settings to a RepositoryItem.</param>
		public EditorSettings(Action<TRepositoryItem> applier) {
			if (applier == null) throw new ArgumentNullException("applier");
			this.applier = applier;
		}

		///<summary>Creates a new RepositoryItem pre-configured for this instance's settings.</summary>
		public virtual TRepositoryItem CreateItem() {
			var editor = new TRepositoryItem();
			Apply(editor);
			return editor;
		}
		///<summary>Configures an existing RepositoryItem with this instance's settings.</summary>
		public virtual void Apply(TRepositoryItem item) {
			applier(item);
		}

		RepositoryItem IEditorSettings.CreateItem() { return CreateItem(); }
		void IEditorSettings.Apply(RepositoryItem item) { Apply((TRepositoryItem)item); }
		///<summary>Gets the type of RepositoryItem that these settings apply to.</summary>
		public Type ItemType { get { return typeof(TRepositoryItem); } }
	}
	///<summary>Contains a preset RepositoryItem that can be modified by client applications.</summary>
	public class MutableEditorSettings<TRepositoryItem> : EditorSettings<TRepositoryItem> where TRepositoryItem : RepositoryItem, new() {
		readonly LinkedList<Action<TRepositoryItem>> configurators = new LinkedList<Action<TRepositoryItem>>();

		///<summary>Configures an existing RepositoryItem with this instance's settings.</summary>
		public override void Apply(TRepositoryItem item) {
			foreach (var method in configurators)
				method(item);
		}

		///<summary>Adds an additional configuration delegate to apply settings to repository items.</summary>
		public void AddConfigurator(Action<TRepositoryItem> method) {
			configurators.AddLast(method);
		}
	}

	///<summary>Contains settings for a simple RepositoryItemComboBox.</summary>
	public class ComboBoxSettings : EditorSettings<RepositoryItemComboBox> {
		static readonly Action<RepositoryItemComboBox> emptyAction = _ => { };

		///<summary>Creates a new ComboBoxSettings instance.</summary>
		public ComboBoxSettings(params string[] items) : this((IEnumerable<string>)items) { }
		///<summary>Creates a new ComboBoxSettings instance.</summary>
		public ComboBoxSettings(IEnumerable<string> items) : this(items, null) { }
		///<summary>Creates a new ComboBoxSettings instance with an additional configurator delegate.</summary>
		public ComboBoxSettings(IEnumerable<string> items, Action<RepositoryItemComboBox> configurator)
			: base(configurator ?? emptyAction) {
			Items = items.ReadOnlyCopy();
		}

		///<summary>Gets the items in the dropdown list.</summary>
		public ReadOnlyCollection<string> Items { get; private set; }

		///<summary>Configures an existing RepositoryItem with this instance's settings.</summary>
		public override void Apply(RepositoryItemComboBox item) {
			item.Items.Clear();
			item.Items.AddRange(Items);
			item.DropDownRows = Math.Min(15, Items.Count);
			item.HighlightedItemStyle = HighlightStyle.Skinned;

			base.Apply(item);	//Call the optional configurator
		}
	}
}
