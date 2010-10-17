using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity;

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

			AccountEditor = new ComboBoxSettings(Names.AccountNames);
			PaymentMethodEditor = new ComboBoxSettings(Names.PaymentMethods);
			StateEditor = new ComboBoxSettings(Names.CommonStates.Concat(Names.StateAbbreviations));
			#endregion
		}

		///<summary>Gets the EditorSettings for a currency field.</summary>
		public static EditorSettings<RepositoryItemSpinEdit> CurrencyEditor { get; private set; }
		///<summary>Gets the EditorSettings for the Billing Account field.</summary>
		public static ComboBoxSettings AccountEditor { get; private set; }
		///<summary>Gets the EditorSettings for the payment method field.</summary>
		public static ComboBoxSettings PaymentMethodEditor { get; private set; }
		///<summary>Gets the EditorSettings for a US State field.</summary>
		public static ComboBoxSettings StateEditor { get; private set; }

		///<summary>Gets the number of behavior registrations,</summary>
		///<remarks>This property is used by the grid to verify design-time
		///registrations.  See <see cref="SmartGrid.RegistrationCount"/>.</remarks>
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

			var schema = ((Table)dataSource).Schema;
			IEditorSettings retVal;
			dictionary.TryGetValue(schema.Columns[columnName], out retVal);
			return retVal;
		}
	}

	///<summary>Contains a preset RepositoryItem.</summary>
	public interface IEditorSettings {
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

		public override void Apply(RepositoryItemComboBox item) {
			item.Items.Clear();
			item.Items.AddRange(Items);
			item.DropDownRows = Math.Min(15, Items.Count);
			item.HighlightedItemStyle = HighlightStyle.Skinned;

			base.Apply(item);	//Call the optional configurator
		}
	}
}
