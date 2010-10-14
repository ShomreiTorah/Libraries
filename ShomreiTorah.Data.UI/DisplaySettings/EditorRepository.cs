using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors.Repository;
using ShomreiTorah.Singularity;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Contains RepositoryItem presets.</summary>
	public static partial class EditorRepository {
		//The static ctor is executed after all field initializers, so the linked list will exist
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Force deterministic initialization")]
		static EditorRepository() { SettingsRegistrator.EnsureRegistered(); }

		static readonly Dictionary<Column, IEditorSettings> dictionary = new Dictionary<Column, IEditorSettings>();

		///<summary>Registers an IEditorSettings preset for a column in a Singularity schema.</summary>
		public static void Register(Column column, IEditorSettings settings) {
			if (column == null) throw new ArgumentNullException("column");
			if (settings == null) throw new ArgumentNullException("settings");
			UIThread.Verify();
			dictionary.Add(column, settings);
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
}
