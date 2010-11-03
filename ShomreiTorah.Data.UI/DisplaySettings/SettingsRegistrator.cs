using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ShomreiTorah.Common;
using ShomreiTorah.Data.UI.Grid;
using ShomreiTorah.Data.UI.Properties;
using ShomreiTorah.Singularity;
using ShomreiTorah.WinForms;
using ShomreiTorah.WinForms.Controls.Lookup;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Registers grid and column behaviors.</summary>
	static class SettingsRegistrator {
		static void InitializeStandardSettings() {
			//This method will only be called once
			RegisterEditors();
			ConfigureLookups();
			RegisterColumnSuppressions();
			RegisterBehaviors();
			RegisterColumnControllers();
		}
		static void RegisterEditors() {
			EditorRepository.Register(Person.StateColumn, EditorRepository.StateEditor);
			EditorRepository.Register(Person.ZipColumn, EditorRepository.ZipEditor);
			EditorRepository.Register(Person.PhoneColumn, EditorRepository.PhoneEditor);

			EditorRepository.Register(Payment.MethodColumn, EditorRepository.PaymentMethodEditor);
			EditorRepository.Register(Payment.CheckNumberColumn, new EditorSettings<Controls.RepositoryItemCheckNumberEdit>(p => { }));

			EditorRepository.Register(new[] { Pledge.AmountColumn, Payment.AmountColumn }, EditorRepository.CurrencyEditor);
			EditorRepository.Register(new[] { Pledge.AccountColumn, Payment.AccountColumn }, EditorRepository.AccountEditor);
		}
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Configuration")]
		static void ConfigureLookups() {
			EditorRepository.PersonLookup.AddConfigurator(item => {
				if (!AppFramework.Current.IsDesignTime)
					item.DataSource = AppFramework.Current.DataContext.Table<Person>();
				item.SelectionIcon = Resources.People16;

				item.Columns.Clear();
				item.AdditionalResultColumns.Clear();
				item.Columns.AddRange(
					new DataSourceColumn("LastName", 100) { Caption = "Last name", ShouldFilter = true },
					new DataSourceColumn("HisName", 95) { Caption = "His name", ShouldFilter = true },
					new DataSourceColumn("HerName", 75) { Caption = "Her name", ShouldFilter = true },
					new DataSourceColumn("Phone"),
					new DataSourceColumn("Address", 150),
					new DataSourceColumn("Zip", 50) { Caption = "Zip code" }
				);

				item.ResultDisplayColumn = new DataSourceColumn("FullName");
				item.SortColumn = new DataSourceColumn("LastName");

				item.AdditionalResultColumns.AddRange(
					new DataSourceColumn("Address"),
					new DataSourceColumn("Phone")
				);
			});
			EditorRepository.PersonOwnedLookup.AddConfigurator(item => {
				if (item.SelectionIcon == null)
					item.SelectionIcon = Resources.People16;

				item.Columns.Clear();
				item.AdditionalResultColumns.Clear();
				item.Columns.AddRange(
					new CustomColumn<IOwnedObject>(o => o.Person.LastName, 100) { Caption = "Last name", ShouldFilter = true },
					new CustomColumn<IOwnedObject>(o => o.Person.HisName, 95) { Caption = "His name", ShouldFilter = true },
					new CustomColumn<IOwnedObject>(o => o.Person.HerName, 75) { Caption = "Her name", ShouldFilter = true },
					new CustomColumn<IOwnedObject>(o => o.Person.Phone) { Caption = "Phone" },
					new CustomColumn<IOwnedObject>(o => o.Person.Address, 150) { Caption = "Address" },
					new CustomColumn<IOwnedObject>(o => o.Person.Zip, 50) { Caption = "Zip code" }
				);

				item.ResultDisplayColumn = new CustomColumn<IOwnedObject>(o => o.Person.FullName);
				item.SortColumn = new CustomColumn<IOwnedObject>(o => o.Person.LastName);

				item.AdditionalResultColumns.AddRange(
					new CustomColumn<IOwnedObject>(o => o.Person.Address),
					new CustomColumn<IOwnedObject>(o => o.Person.Phone)
				);
			});
		}
		#region Column Suppressions
		static void RegisterColumnSuppressions() {
			GridManager.SuppressColumns(Pledge.ExternalSourceColumn, Pledge.ExternalIdColumn);
			GridManager.SuppressColumns(Payment.ExternalSourceColumn, Payment.ExternalIdColumn);

			GridManager.SuppressColumns(EmailAddress.ActiveColumn, EmailAddress.RandomCodeColumn, EmailAddress.UseHtmlColumn);

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
		}
		#endregion

		//TODO: Group summaries
		#region Behaviors
		static void RegisterBehaviors() {
			//TODO: Delete LoggedStatements?

			GridManager.RegisterBehavior(Person.Schema,
				DeletionBehavior.Disallow("You cannot delete rows from the master directory.\r\nIf you really want to delete someone, call Schabse.")
			);
			GridManager.RegisterBehavior(EmailAddress.Schema,
				DeletionBehavior.WithMessages(
					"email address from the email list?",
					"email addresses from the email list?"
				)
			);
			GridManager.RegisterBehavior(Pledge.Schema,
				DeletionBehavior.WithMessages<Pledge>(
					singular: p => "Are you sure you want to delete this " + p.Amount.ToString("c", CultureInfo.CurrentCulture) + " pledge?",
					plural: pledges => "Are you sure you want to delete "
									  + (pledges.Count().ToString(CultureInfo.InvariantCulture) + " pledges totaling "
									   + pledges.Sum(p => p.Amount).ToString("c", CultureInfo.CurrentCulture) + "?")
				)
			);
			GridManager.RegisterBehavior(Payment.Schema,
				DeletionBehavior.WithMessages<Payment>(
					singular: p => "Are you sure you want to delete this " + p.Amount.ToString("c", CultureInfo.CurrentCulture) + " payment?",
					plural: payments => "Are you sure you want to delete "
									  + (payments.Count().ToString(CultureInfo.InvariantCulture) + " payments totaling "
									   + payments.Sum(p => p.Amount).ToString("c", CultureInfo.CurrentCulture) + "?")
				)
			);
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
		}
		#endregion
		#region Column Controllers
		static ColumnController MaxWidth(int width) { return new ColumnController(c => c.MaxWidth = width); }
		static void RegisterColumnControllers() {
			GridManager.RegisterColumn(Person.StateColumn, MaxWidth(50));
			GridManager.RegisterColumn(Person.ZipColumn, MaxWidth(40));
			GridManager.RegisterColumn(Person.PhoneColumn, MaxWidth(95));
			GridManager.RegisterColumn(Payment.MethodColumn, MaxWidth(70));

			GridManager.RegisterColumns(new[] { Payment.AccountColumn, Pledge.AccountColumn }, MaxWidth(100));

			GridManager.RegisterColumns(
				new[] { Pledge.ModifiedColumn, Payment.ModifiedColumn },
				new ColumnController(c => {
					c.DisplayFormat.FormatType = FormatType.DateTime;
					c.DisplayFormat.FormatString = "g";
				})
			);

			GridManager.RegisterColumns(new[] { Payment.CommentsColumn, Pledge.CommentsColumn }, new CommentsColumnController());

			GridManager.RegisterColumn(Person.LastNameColumn, new ColumnController(c => {
				c.SortOrder = ColumnSortOrder.Ascending;
				c.SummaryItem.DisplayFormat = "{0} People";
				c.SummaryItem.SummaryType = SummaryItemType.Count;
			}));

			GridManager.RegisterColumn(
				Pledge.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);	//Copy currency format

					c.MaxWidth = 85;
					c.SummaryItem.DisplayFormat = "{0:c} Total Pledged";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);
			GridManager.RegisterColumn(
				Payment.AmountColumn,
				new ColumnController(c => {
					c.DisplayFormat.Assign(c.DefaultEditor.DisplayFormat);	//Copy currency format

					c.MaxWidth = 85;
					c.SummaryItem.DisplayFormat = "{0:c} Total Paid";
					c.SummaryItem.SummaryType = SummaryItemType.Sum;
				})
			);

			GridManager.RegisterColumn(
				c => {
					var fkc = c as ForeignKeyColumn;
					return fkc != null && fkc.ForeignSchema == Person.Schema;
				},
				new PersonColumnController()
			);
			GridManager.RegisterColumn(Payment.DepositColumn, new DepositColumnController());
			GridManager.RegisterColumn(Pledge.SubTypeColumn, new SubTypeColumnController());
		}
		#endregion

		class DepositColumnController : ColumnController {
			protected internal override void Apply(SmartGridColumn column) {
				column.OptionsColumn.AllowSort = DefaultBoolean.True;
				column.OptionsColumn.AllowGroup = DefaultBoolean.True;
				column.MaxWidth = 90;
				column.Caption = "Deposited";

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

			static readonly SuperToolTip Undeposited = Utilities.CreateSuperTip(body: "This payment has not been deposited");
			protected internal override SuperToolTip GetCellToolTip(object row, object value) {
				var deposit = value as Deposit;
				if (deposit == null)
					return Undeposited;
				return deposit.GetSuperTip();
			}
		}
		class CommentsColumnController : ColumnController {
			protected internal override void Apply(SmartGridColumn column) {
				column.SetDefaultEditor(EditorRepository.CommentsPopupEditor.CreateItem());
			}
			protected internal override SuperToolTip GetCellToolTip(object row, object value) {
				string str = (value ?? "").ToString();
				if (String.IsNullOrWhiteSpace(str) || !str.Contains('\n'))
					return null;
				return Utilities.CreateSuperTip(body: str);
			}
		}
		class SubTypeColumnController : ColumnController {
			//No settings to apply
			protected internal override void Apply(SmartGridColumn column) { }

			static readonly ReadOnlyCollection<string> SortedOrder = Names.PledgeTypes.SelectMany(p => p.Subtypes).Distinct().ReadOnlyCopy();
			protected internal override void CompareValues(CustomColumnSortEventArgs e) {
				var index1 = SortedOrder.IndexOf(e.Value1 as string);
				var index2 = SortedOrder.IndexOf(e.Value2 as string);

				e.Result = index1.CompareTo(index2);
				e.Handled = index1 != index2;
			}
		}

		static SettingsRegistrator() { InitializeStandardSettings(); AppFramework.AutoRegisterDesigner(); }

		///<summary>Ensures that all grid settings have been registered.</summary>
		public static void EnsureRegistered() { AppFramework.AutoRegisterDesigner(); }
	}
	///<summary>A ColumnController that controls a column displaying Person objects.</summary>
	public class PersonColumnController : ColumnController {
		protected internal override void Apply(SmartGridColumn column) {
			column.OptionsColumn.ReadOnly = true;
			column.OptionsColumn.AllowSort = DefaultBoolean.True;
			column.OptionsColumn.AllowGroup = DefaultBoolean.True;
			column.ShowButtonMode = ShowButtonModeEnum.ShowAlways;
			column.ShowEditorOnMouseDown = true;

			if (AppFramework.Current.CanShowDetails<Person>())
				column.SetDefaultEditor(PersonEditSettings.Instance.CreateItem());
			column.Caption = "Full Name";
		}
		protected internal override string GetDisplayText(object row, object value) {
			var person = (Person)value;
			if (person == null) return null;
			return person.FullName;
		}
		protected internal override SuperToolTip GetCellToolTip(object row, object value) {
			var person = (Person)value;
			if (person == null) return null;
			return person.GetSuperTip();
		}
		class PersonEditSettings : EditorSettings<RepositoryItemButtonEdit> {
			private PersonEditSettings() { }
			public static readonly PersonEditSettings Instance = new PersonEditSettings();

			//I need to set e.Handled to true to prevent the editor
			//from sending the event to the grid's EditorContainer 
			//handlers, which would double the effect.
			static void HandleKeyEvent(object sender, KeyEventArgs e, Action<SmartGridView> handler) {
				var view = (SmartGridView)((GridControl)((BaseEdit)sender).Parent).FocusedView;
				handler(view);
				e.Handled = true;
			}
			static void HandleKeyPressEvent(object sender, KeyPressEventArgs e) {
				var view = (SmartGridView)((GridControl)((BaseEdit)sender).Parent).FocusedView;
				view.SendKeyPress(e);
				e.Handled = true;
			}

			public override void Apply(RepositoryItemButtonEdit item) {
				item.TextEditStyle = TextEditStyles.DisableTextEditor;
				item.Buttons.Clear();
				item.Buttons.Add(new EditorButton(ButtonPredefines.Glyph) { Image = Resources.UserGrid, IsLeft = true, SuperTip = Utilities.CreateSuperTip(body: "Show Person") });

				//Forward all keyboard events to the grid; this editor should behave as part of the grid
				item.KeyDown += (sender, e) => HandleKeyEvent(sender, e, view => view.SendKeyDown(e));
				item.KeyPress += (sender, e) => HandleKeyPressEvent(sender, e);
				item.KeyUp += (sender, e) => HandleKeyEvent(sender, e, view => view.SendKeyUp(e));

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
					var view = (SmartGridView)grid.FocusedView;
					var row = view.GetFocusedRow() as Row;	//In the designer, there might not be actual rows.

					if (row != null && AppFramework.Current.CanShowDetails(row.Schema))
						AppFramework.Current.ShowDetails(row);
				};
			}
		}
	}
}
