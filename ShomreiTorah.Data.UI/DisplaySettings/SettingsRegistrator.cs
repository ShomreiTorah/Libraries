using System;
using System.Linq;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Data.UI.Properties;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Registers grid and column behaviors.</summary>
	static class SettingsRegistrator {
		static void InitializeStandardSettings() {
			//This method will only be called once
			EditorRepository.Register(new[] { Pledge.AmountColumn, Payment.AmountColumn }, EditorRepository.CurrencyEditor);
			EditorRepository.Register(new[] { Pledge.AccountColumn, Payment.AccountColumn }, EditorRepository.AccountEditor);

			#region Column Suppressions
			GridManager.SuppressColumns(Pledge.ExternalSourceColumn, Pledge.ExternalIdColumn);
			GridManager.SuppressColumns(Payment.ExternalSourceColumn, Payment.ExternalIdColumn);

			GridManager.SuppressColumn(c => {
				//Detail views in the designer won't have column types.
				if (c.ColumnType != typeof(object))
					return c.ColumnType == typeof(Guid);

				var column = c.GetSchemaColumn();
				return column != null && column.DataType == typeof(Guid);
			});
			GridManager.SuppressColumn(c => {
				var parentSchema = c.View.GetParentSchema();
				var childSchema = c.View.GetSourceSchema();

				if (parentSchema == null || childSchema == null)
					return false;
				var fkc = childSchema.Columns[c.FieldName] as ForeignKeyColumn;

				//If this column is a foreign key that references the schema of the parent view, hide it.
				return fkc != null && fkc.ForeignSchema == parentSchema && fkc.ChildRelation.Name == c.View.LevelName;
			});
			#endregion

			#region Behaviors
			GridManager.RegisterBehavior(
				new TableSchema[] { Pledge.Schema, Payment.Schema },
				new AdvancedColumnsBehavior("modifier columns", new[] { "Modified", "Modifier" })
			);
			//The predicate will only be called after views are created,
			//after the client application has registered its activators
			GridManager.RegisterBehavior(
				ds => {
					var schema = TableSchema.GetSchema(ds);
					return schema != null && AppFramework.Current.CanShowDetails(schema);
				},
				new RowDetailBehavior()
			);
			#endregion

			GridManager.RegisterColumn(
				Pledge.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);

					c.MaxWidth = 70;
					c.SummaryItem.DisplayFormat = "{0:c} Total Pledged";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);
			GridManager.RegisterColumn(
				Payment.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);

					c.MaxWidth = 70;
					c.SummaryItem.DisplayFormat = "{0:c} Total Paid";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);

			GridManager.RegisterColumns(
				new[] { Payment.AccountColumn, Pledge.AccountColumn },
				new ColumnController(c => { c.MaxWidth = 100; })
			);

			GridManager.RegisterColumn(
				c => {
					var fkc = c as ForeignKeyColumn;
					return fkc != null && fkc.ForeignSchema == Person.Schema;
				},
				new PersonColumnController()
			);
			GridManager.RegisterColumn(Payment.DepositColumn, new DepositColumnController());
		}

		class PersonColumnController : ColumnController {
			protected internal override void Apply(SmartGridColumn column) {
				column.OptionsColumn.ReadOnly = true;
				column.OptionsColumn.AllowSort = DefaultBoolean.True;
				column.OptionsColumn.AllowGroup = DefaultBoolean.True;
				column.ShowButtonMode = ShowButtonModeEnum.ShowAlways;

				if (AppFramework.Current.CanShowDetails<Person>())
					column.SetDefaultEditor(PersonEditSettings.Instance.CreateItem());
				column.Caption = "Full Name";
			}
			protected internal override string GetDisplayText(object row, object value) {
				var person = (Person)value;
				if (person == null) return "";
				return person.FullName;
			}

			class PersonEditSettings : EditorSettings<RepositoryItemButtonEdit> {
				private PersonEditSettings() { }
				public static readonly PersonEditSettings Instance = new PersonEditSettings();

				public override void Apply(RepositoryItemButtonEdit item) {
					item.TextEditStyle = TextEditStyles.DisableTextEditor;
					item.Buttons.Clear();
					item.Buttons.Add(new EditorButton(ButtonPredefines.Glyph) { Image = Resources.UserGrid, IsLeft = true, ToolTip = "Show Person" });

					item.CustomDisplayText += (sender, e) => {
						var person = e.Value as Person;
						if (person != null) e.DisplayText = person.FullName;
					};
					item.ButtonClick += (sender, e) => {
						var edit = sender as BaseEdit;
						var row = edit.EditValue as Row;
						if (row != null)
							AppFramework.Current.ShowDetails(row);
					};
					item.DoubleClick += (sender, e) => {
						var edit = sender as BaseEdit;
						var grid = (GridControl)edit.Parent;
						var view = (SmartGridView)grid.MainView;
						var row = view.GetFocusedRow() as Row;	//In the designer, there might not be actual rows.

						if (row != null && AppFramework.Current.CanShowDetails(row.Schema))
							AppFramework.Current.ShowDetails(row);
					};
				}
			}
		}
		class DepositColumnController : ColumnController {
			protected internal override void Apply(SmartGridColumn column) {
				column.OptionsColumn.AllowSort = DefaultBoolean.True;
				column.OptionsColumn.AllowGroup = DefaultBoolean.True;

				column.OptionsColumn.AllowEdit = false;
				column.OptionsColumn.ReadOnly = true;
			}
			protected internal override string GetDisplayText(object row, object value) {
				var deposit = (Deposit)value;
				if (deposit == null)
					return "Undeposited";
				return deposit.Date.ToShortDateString() + " #" + deposit.Number;
			}
			protected internal override void OnShowFilterPopupListBox(FilterPopupListBoxEventArgs e) {
				foreach (var item in e.ComboBox.Items.OfType<FilterItem>()) {
					var valueItem = item.Value as FilterItem;
					if (valueItem != null) {
						if ((int)valueItem.Value == 2)
							item.Text = "(Undeposited)";
						else if ((int)valueItem.Value == 3)
							item.Text = "(All deposited)";
					}
				}
			}
		}

		static SettingsRegistrator() { InitializeStandardSettings(); }

		///<summary>Ensures that all grid settings have been registered.</summary>
		public static void EnsureRegistered() { }
	}
}
