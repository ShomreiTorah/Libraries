using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using ShomreiTorah.WinForms;
using ShomreiTorah.Data.UI.DisplaySettings;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>Applies preset editor settings to editors in a designer.</summary>
	[ProvideProperty("UseDefaultSettings", typeof(BaseEdit))]
	public class EditorSettingsApplier : Component, IExtenderProvider, ISupportInitialize {
		readonly Dictionary<BaseEdit, bool> values = new Dictionary<BaseEdit, bool>();

		[Description("Gets or sets whether the editor should use the default settings for the column it's bound to.")]
		[Category("Data")]
		[DefaultValue(false)]
		[RefreshProperties(RefreshProperties.All)]
		public bool GetUseDefaultSettings(BaseEdit editor) {
			bool result;
			values.TryGetValue(editor, out result);	//Defaults to false if not found
			return result;
		}
		public void SetUseDefaultSettings(BaseEdit editor, bool value) {
			values[editor] = value && ApplySettings(editor);	//Only apply if value is true
		}

		bool ApplySettings(BaseEdit edit) {
			//This will be called in addition to normal designer serialization of previously applied settings
			if (edit.DataBindings.Count != 1) {
				if (ShouldShowErrors)
					Dialog.ShowError(edit.Name + " is not databound");
				return false;
			}
			var binding = edit.DataBindings[0];

			var schema = TableSchema.GetSchema(binding.BindingManagerBase.GetItemProperties()[0]);
			if (schema == null) {
				if (ShouldShowErrors)
					Dialog.ShowError(edit.Name + " is not bound to a Singularity schema");
				return false;
			}
			var column = schema.Columns[binding.BindingMemberInfo.BindingField];

			var settings = EditorRepository.GetSettings(column);
			if (settings == null) {
				if (ShouldShowErrors)
					Dialog.ShowError("There are no settings associated with the " + schema.Name + "." + column.Name + " column");
				return false;
			}

			settings.Apply(edit.Properties);
			return true;
		}
		bool ShouldShowErrors { get { return !initializing && DesignMode; } }

		///<summary>Checks whether this extender can extend the given object.</summary>
		public bool CanExtend(object extendee) {
			return true;
		}

		bool initializing;
		public void BeginInit() { initializing = true; }
		public void EndInit() { initializing = false; }
	}
}
