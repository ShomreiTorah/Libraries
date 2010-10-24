using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Data.UI.DisplaySettings;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>Applies preset editor settings to editors in a designer.</summary>
	[ProvideProperty("DefaultSettingsMode", typeof(BaseEdit))]
	[Designer(typeof(EsaDesigner))]
	public class EditorSettingsApplier : Component, IExtenderProvider, ISupportInitialize {
		readonly Dictionary<BaseEdit, DefaultSettingsMode> values = new Dictionary<BaseEdit, DefaultSettingsMode>();

		///<summary>Gets the value of the DefaultSettingsMode property for an editor.</summary>
		[Description("Gets or sets whether the editor should try use the default settings for the column it's bound to.")]
		[Category("Data")]
		[DefaultValue(DefaultSettingsMode.None)]
		[RefreshProperties(RefreshProperties.All)]
		public DefaultSettingsMode GetDefaultSettingsMode(BaseEdit editor) {
			DefaultSettingsMode result;
			values.TryGetValue(editor, out result);	//Defaults to false if not found
			return result;
		}
		///<summary>Sets the value of the DefaultSettingsMode property for an editor.</summary>
		public void SetDefaultSettingsMode(BaseEdit editor, DefaultSettingsMode value) {
			if (value != DefaultSettingsMode.None)
				value = ApplySettings(editor) ? DefaultSettingsMode.Active : DefaultSettingsMode.Inactive;

			values[editor] = value;
		}

		///<summary>Applies any registered EditorSettings to an editor.</summary>
		///<returns>True if there were any settings to apply.</returns>
		bool ApplySettings(BaseEdit edit) {
			//This will be called in addition to normal designer serialization of previously applied settings
			if (edit.DataBindings.Count != 1) {
				if (ShouldShowErrors)
					Dialog.ShowError(edit.Name + " is not databound");
				return false;
			}
			var binding = edit.DataBindings[0];

			var column = GetColumn(binding);
			if (column == null) {
				if (ShouldShowErrors)
					Dialog.ShowError(edit.Name + " is not bound to a Singularity schema");
				return false;
			}

			var settings = EditorRepository.GetSettings(column);
			if (settings == null) {
				if (ShouldShowErrors)
					Dialog.ShowError("There are no settings associated with the " + column.Schema.Name + "." + column.Name + " column");
				return false;
			}

			settings.Apply(edit.Properties);
			return true;
		}
		bool ShouldShowErrors { get { return !initializing && DesignMode; } }

		static Column GetColumn(Binding binding) {
			var schema = TableSchema.GetSchema(binding.BindingManagerBase.GetItemProperties()[0]);
			if (schema == null) return null;
			return schema.Columns[binding.BindingMemberInfo.BindingField];
		}

		///<summary>Checks whether this extender can extend the given object.</summary>
		public bool CanExtend(object extendee) {
			return true;
		}

		bool initializing;
		///<summary>Called to signal the object that initialization is starting.</summary>
		public void BeginInit() { initializing = true; }
		///<summary>Called to signal the object that initialization is complete.</summary>
		public void EndInit() { initializing = false; }

		///<summary>Gets a CustomTypeDescriptor that shows information about the editors on the form at design time.</summary>
		[Category("Information")]
		[Description("Shows information about the editors on the form.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ParenthesizePropertyName]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		//[TypeConverter(typeof(DevExpress.Utils.Design.UniversalTypeConverter))]
		public object Editors { get { return new EditorsInfo(this); } }

		class EditorsInfo : CustomTypeDescriptor {
			readonly EditorSettingsApplier owner;
			public EditorsInfo(EditorSettingsApplier owner) { this.owner = owner; }

			public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
				return GetProperties();
			}
			public override PropertyDescriptorCollection GetProperties() {
				return new PropertyDescriptorCollection(
					owner.Container.Components
						.OfType<BaseEdit>()
						.OrderByDescending(e => (int)owner.GetDefaultSettingsMode(e))	//Active, then Inactive, then None.
								   .ThenBy(e => e.Name)
						.Select((e, i) => new EditorProperty(owner, i, e)).ToArray()
				);
			}
			static string GetDescription(BaseEdit edit) {
				if (edit.DataBindings.Count == 0)
					return edit.Name + " is not databound";
				if (edit.DataBindings.Count > 1)
					return edit.Name + " has multiple bound properties";

				var binding = edit.DataBindings[0];

				var column = GetColumn(binding);
				if (column == null)
					return edit.Name + " is not bound to a Singularity schema";

				return edit.Name + " is bound to " + column.Schema.Name + "." + column.Name;
			}
			class EditorProperty : PropertyDescriptor {
				readonly EditorSettingsApplier owner;
				readonly BaseEdit edit;
				public EditorProperty(EditorSettingsApplier owner, int index, BaseEdit edit)
					: base(index + ": " + edit.Name, new[] { new DescriptionAttribute(GetDescription(edit)) }) {
					this.owner = owner;
					this.edit = edit;
				}

				public override Type ComponentType { get { return typeof(EditorsInfo); } }
				public override Type PropertyType { get { return typeof(string); } }

				public override object GetValue(object component) {
					return owner.GetDefaultSettingsMode(edit).ToString();
				}
				//Non-None values should be bold.
				public override bool ShouldSerializeValue(object component) {
					return owner.GetDefaultSettingsMode(edit) != DefaultSettingsMode.None;
				}

				public override bool IsReadOnly { get { return true; } }
				public override bool CanResetValue(object component) { return false; }
				public override void ResetValue(object component) { }
				public override void SetValue(object component, object value) { }
			}
		}
	}

	sealed class EsaDesigner : IDesigner {
		public void Initialize(IComponent component) {
			if (component == null) throw new ArgumentNullException("component");
			Component = (EditorSettingsApplier)component;

			Verbs = new DesignerVerbCollection { 
				new DesignerVerb("Apply to all editors", delegate { DoDefaultAction(); }) 
			};
		}
		IComponent IDesigner.Component { get { return Component; } }
		public EditorSettingsApplier Component { get; private set; }

		public void DoDefaultAction() {
			if (Dialog.Confirm("Would you like to apply default settings to all editors?", "EditorSettingsApplier Designer"))
				ApplyToAll();
		}

		void ApplyToAll() {
			foreach (var edit in Component.Container.Components.OfType<BaseEdit>()) {
				Component.SetDefaultSettingsMode(edit, DefaultSettingsMode.Active);
			}
		}

		public DesignerVerbCollection Verbs { get; private set; }

		public void Dispose() { }
	}

	///<summary>Specifies whether default editor settings should be applied.</summary>
	public enum DefaultSettingsMode {
		///<summary>Default settings should not be applied.</summary>
		None,
		///<summary>Default settings should be applied, but there are no settings to apply for this column.</summary>
		Inactive,
		///<summary>Default settings are applied.</summary>
		Active
	}
}
