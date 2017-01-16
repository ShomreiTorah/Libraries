using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using ShomreiTorah.Common;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;
using ShomreiTorah.WinForms;
using ShomreiTorah.WinForms.Forms;

namespace ShomreiTorah.Data.UI {
	///<summary>The base class for a standard ShomreiTorah application.</summary>
	public abstract class AppFramework {
		#region Automatic Designer Registration
		///<summary>Automatically registers the AppFramework for the current project at design time.</summary>
		public static void AutoRegisterDesigner() {
			if (Current != null) return;
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) return;

			var dh = (IDesignerHost)LicenseManager.CurrentContext.GetService(typeof(IDesignerHost));
			if (dh == null) return;

			string typeName;
			if (dh.RootComponentClassName.Contains('.'))
				typeName = dh.RootComponentClassName;
			else
				typeName = GetTargetNamespace(dh) + "." + dh.RootComponentClassName;
			var rootType = Type.GetType(typeName);
			if (rootType == null) {
				MessageBox.Show("Cannot find  type " + typeName + ".\r\nTry rebuilding the project.",
								"Shomrei Torah Design System", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			//TODO: TypeScan fall-back if GetTargetNamespace fails; error handling

			//if (rootType.Assembly.EntryPoint == null) return;	//Type is in a DLL

			var appFrameworkType = rootType.Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(AppFramework)))
								?? FindParentProjects(rootType.Assembly.GetName().Name);

			if (appFrameworkType == null) {
				MessageBox.Show("Cannot find an AppFramework type in " + rootType.Assembly + ".\r\nTry rebuilding the project.",
								"Shomrei Torah Design System", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			CheckDesignTime((AppFramework)Activator.CreateInstance(appFrameworkType));
		}
		///<summary>Looks for AppFramework subclasses in assemblies that are prefixes of the specified assembly.  Useful for plugin projects.</summary>
		static Type FindParentProjects(string assemblyName) {
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => assemblyName.StartsWith(a.GetName().Name, StringComparison.OrdinalIgnoreCase))
				.OrderByDescending(a => a.GetName().Name.Length)
				.SelectMany(a => a.GetTypes())
				.FirstOrDefault(t => t.IsSubclassOf(typeof(AppFramework)));
		}

		static string GetTargetNamespace(IDesignerHost dh) {
			var _loader = dh.GetType().GetField("_loader", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dh);
			var CodeDom = _loader.GetType().GetProperty("CodeDom", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_loader, null);

			var TypeNamespace = (CodeNamespace)CodeDom.GetType().GetProperty("TypeNamespace", BindingFlags.Public | BindingFlags.Instance).GetValue(CodeDom, null);

			return TypeNamespace.Name;
		}
		#endregion

		///<summary>Gets the typed table for the given row type from the current application's DataContext.</summary>
		public static TypedTable<TRow> Table<TRow>() where TRow : Row { return Current.DataContext.Table<TRow>(); }

		///<summary>Gets the AppFramework instance for the current application.</summary>
		///<remarks>This property is also available at design time.</remarks>
		public static AppFramework Current { get; protected set; }

		bool isDesignTime = true;
		///<summary>Indicates whether the code is running in the Visual Studio designer.</summary>
		public bool IsDesignTime {
			get { return isDesignTime; }
			protected set { isDesignTime = value; }
		}

		///<summary>Registers an AppFramework instance for design time, if none is registered.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore design-time SQL errors")]
		protected static void CheckDesignTime(AppFramework instance) {
			if (Current != null)
				return;
			Current = instance;

			RegisterStandardSettings();

			//Call CreateDataContext() first so that RegisterSettings()
			//can use additional columns created in CreateDataContext()
			instance.SyncContext = instance.CreateDataContext();

			instance.RegisterSettings();

			try {
				instance.SyncContext.ReadData();
			} catch { instance.SyncContext = instance.CreateDataContext(); }    //Ignore SQL errors at design time
		}

		///<summary>Gets the application's main form.</summary>
		public Form MainForm { get; private set; }
		///<summary>Gets the Singularity DataContext used by the application.</summary>
		public DataContext DataContext { get { return SyncContext.DataContext; } }
		///<summary>Gets the DataSyncContext used to synchronize with a database server.</summary>
		public DataSyncContext SyncContext { get; protected set; }

		///<summary>Overridden by derived classes to create the application's splash screen.  Called on the main thread.</summary>
		protected abstract ISplashScreen CreateSplash();
		///<summary>Overridden by derived classes to register grid and editor settings.</summary>
		protected abstract void RegisterSettings();
		///<summary>Creates the application's main form.</summary>
		protected abstract Form CreateMainForm();
		///<summary>Creates the main DataContext used by the application.</summary>
		///<returns>A DataSyncContext used to synchronize with a database server.</returns>
		protected abstract DataSyncContext CreateDataContext();

		///<summary>Indicates whether EnableVisualStyles and SetCompatibleTextRenderingDefault have already been called.</summary>
		///<remarks>Set this to true to prevent them from being called again.</remarks>
		protected bool IsWinFormsSetup { get; set; }

		///<summary>Runs the application.</summary>
		protected void Run() {
			if (Current != null)
				throw new InvalidOperationException("An application is already running");
			isDesignTime = false;
			Current = this;

			if (!IsWinFormsSetup) {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				IsWinFormsSetup = true;
			}

			StartSplash();
			AddDefaultExceptionHandlers();
			if (!Debugger.IsAttached) {
				Application.ThreadException += (sender, e) => HandleException(e.Exception);
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException((Exception)e.ExceptionObject);
			}

			if (!OnBeforeInit()) {
				if (splashScreen != null)
					splashScreen.CloseSplash();
				return;
			}

			SetSplashCaption("Loading behaviors");
			RegisterStandardSettings();

			//TODO: Updates
			SyncContext = CreateDataContext();
			Debug.Assert(SyncContext.Tables.Any(), "There aren't any TableSynchronizers!");

			//Call CreateDataContext() first so that RegisterSettings()
			//can use additional columns created in CreateDataContext()
			RegisterSettings();
			Debug.Assert(!String.IsNullOrWhiteSpace(Dialog.DefaultTitle), "Please set a dialog title (in RegisterSettings)");

			SetSplashCaption("Reading database");
			SyncContext.ReadData();

			SetSplashCaption("Loading UI");
			MainForm = CreateMainForm();
			MainForm.Shown += delegate {
				if (splashScreen != null)
					splashScreen.CloseSplash();
				splashScreen = null;
			};
			try {
				Application.Run(MainForm);
			} catch (Exception ex) { MessageBox.Show(ex.ToString()); }
		}
		///<summary>Called immediately after showing the splash screen, before any further initialization.</summary>
		///<returns>True to proceed with the launch; false to exit the program.</returns>
		protected virtual bool OnBeforeInit() { return true; }

		///<summary>Prompts the user to create a new person.</summary>
		///<remarks>The new Person row, or null if the user clicked cancel.</remarks>
		public virtual Person PromptPerson(Person template = null) {
			return Forms.PersonCreator.Prompt(template);
		}

		///<summary>Registers the standard settings in Data.UI.dll.</summary>
		protected static void RegisterStandardSettings() {
			WindowsFormsSettings.AllowPixelScrolling = DefaultBoolean.True;
			WindowsFormsSettings.ColumnFilterPopupMode = ColumnFilterPopupMode.Excel;
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
		}

		#region Splash
		ISplashScreen splashScreen;

		///<summary>Shows the splash screen on a background thread.</summary>
		void StartSplash() {
			splashScreen = CreateSplash();
			if (splashScreen == null) return;

			var splashThread = new Thread(splashScreen.RunSplash) { IsBackground = true };
			splashThread.SetApartmentState(ApartmentState.STA);
			splashThread.Start();
		}
		///<summary>Sets the splash screen's loading message, if supported.</summary>
		///<param name="message">A message to display on the splash screen.</param>
		protected virtual void SetSplashCaption(string message) {
			if (splashScreen != null)
				splashScreen.SetCaption(message);
		}

		///<summary>Invokes a method on the splash screen thread and returns the result.</summary>
		///<remarks>This method should be used to show UI while loading.</remarks>
		protected T InvokeSplash<T>(Func<T> func) {
			return (T)((ISynchronizeInvoke)splashScreen).Invoke(func, null);
		}
		#endregion

		#region Database
		///<summary>Indicates whether there are any unsaved changes in the Singularity DataContext.</summary>
		public bool HasDataChanged {
			get { return SyncContext.Tables.Any(t => t.Changes.Count > 0); }
		}

		///<summary>Saves any changes made to the Singularity DataContext.</summary>
		public void SaveDatabase() {
			Debug.Assert(SyncContext.Tables.Any(), "There aren't any TableSynchronizers!");
			if (!HasDataChanged) return;
			ProgressWorker.Execute(SyncContext.WriteData, cancellable: true);
		}

		///<summary>Reads any changes from the database server.</summary>
		public void RefreshDatabase() {
			Debug.Assert(SyncContext.Tables.Any(), "There aren't any TableSynchronizers!");
			var threadContext = SynchronizationContext.Current;
			ProgressWorker.Execute(progress => {
				if (HasDataChanged)
					SyncContext.WriteData(progress);

				progress.Caption = "Reading database";
				progress.Maximum = -1;
				SyncContext.ReadData(threadContext);
			}, cancellable: false);
		}
		#endregion

		#region LoadTable
		///<summary>Ensures that a table is loaded in the DataContext.</summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static void LoadTable<TRow>() where TRow : Row { LoadTables(TypedSchema<TRow>.Instance); }
		///<summary>Ensures that a set of tables are loaded in the DataContext.</summary>
		public static void LoadTables(params TableSchema[] schemas) { LoadTables((IEnumerable<TableSchema>)schemas); }

		///<summary>Ensures that a set of tables are loaded in the DataContext.</summary>
		public static void LoadTables(IEnumerable<TableSchema> schemas) {
			var allSchemas = new HashSet<TableSchema>();
			foreach (var schema in schemas.Where(s => Current.DataContext.Tables[s] == null)) {
				allSchemas.Add(schema);
				allSchemas.UnionWith(schema.GetDependencies());
			}

			if (allSchemas.Count == 0) return;  //All of the tables are already loaded
			var tables = allSchemas
				.SortDependencies()
				.Except(Current.DataContext.Tables.Select(t => t.Schema))   //Must be called after SortDependencies, since sorting requires all dependencies
				.Select(ts => ts.CreateTable())
				.ToList();

			//Calculated columns can use child rows. I must add the
			//tables in reverse order to allow the RowDependencies
			//to register handlers for the child tables.
			tables.Reverse();
			tables.ForEach(Current.DataContext.Tables.AddTable);

			var syncers = tables.ConvertAll(t => new TableSynchronizer(t, SchemaMapping.GetPrimaryMapping(t.Schema), Current.SyncContext.SqlProvider));
			syncers.ForEach(Current.SyncContext.Tables.Add);

			var threadContext = SynchronizationContext.Current;
			ProgressWorker.Execute(ui => {
				if (Current.HasDataChanged)
					Current.SyncContext.WriteData(ui);          //I must save before loading in case a parent row was deleted.  (The DB is expected to cascade)

				ui.Maximum = -1;
				ui.Caption = "Loading " + tables.Join(", ", t => t.Schema.Name);
				Current.SyncContext.ReadData(threadContext);    //I must refresh everything to pick up potential changes in parent rows
			}, false);
		}
		#endregion

		#region Row Activators
		readonly Dictionary<TableSchema, Action<Row>> rowActivators = new Dictionary<TableSchema, Action<Row>>();
		///<summary>Registers a details form for a Singularity schema.</summary>
		protected void RegisterRowDetail(TableSchema schema, Action<Row> activator) {
			if (schema == null) throw new ArgumentNullException("schema");
			if (activator == null) throw new ArgumentNullException("activator");
			rowActivators.Add(schema, activator);
		}
		///<summary>Registers a details form for a strongly typed Singularity schema.</summary>
		protected void RegisterRowDetail<TRow>(Action<TRow> activator) where TRow : Row {
			if (activator == null) throw new ArgumentNullException("activator");
			RegisterRowDetail(TypedSchema<TRow>.Instance, r => activator((TRow)r));
		}

		///<summary>Checks whether the given row type has an associated details form.</summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Convenience")]
		public bool CanShowDetails<TRow>() where TRow : Row { return CanShowDetails(TypedSchema<TRow>.Instance); }
		///<summary>Checks whether the given schema has an associated details form.</summary>
		public bool CanShowDetails(TableSchema schema) {
			if (schema == null) throw new ArgumentNullException("schema");

			return rowActivators.ContainsKey(schema);
		}
		///<summary>Shows a details form for a row in a Singularity table.</summary>
		public void ShowDetails(Row row) {
			if (row == null) throw new ArgumentNullException("row");
			if (IsDesignTime)
				Dialog.Inform("Not showing row details at design-time for\r\n" + row, "Singularity UI Framework");
			else
				rowActivators[row.Schema](row);
		}
		#endregion

		#region Error Handling
		private LinkedList<Func<Exception, bool>> exceptionHandlers = new LinkedList<Func<Exception, bool>>();

		///<summary>Adds a callback to handle uncaught exceptions of a specific type.</summary>
		public void AddExceptionHandler<TException>(Action<TException> handler) where TException : Exception {
			exceptionHandlers.AddFirst(ex => {
				var e = ex as TException;
				if (e != null)
					handler(e);
				return e != null;
			});
		}

		///<summary>Adds the standard exception handlers built in to the framework.</summary>
		protected virtual void AddDefaultExceptionHandlers() {
			AddExceptionHandler<Exception>(ex => new Forms.ExceptionReporter(ex).Show(MainForm));
		}

		///<summary>Shows appropriate UI in response to an unhandled exception.</summary>
		public virtual void HandleException(Exception ex) {
			if (ex == null) throw new ArgumentNullException("ex");

			bool handled = exceptionHandlers.Any(d => d(ex));
			if (!handled)
				Dialog.ShowError("A very unhandled error occurred.\r\n\r\n" + ex.ToString());
		}
		#endregion
	}



	///<summary>A component that automatically binds to the DataContext in the current AppFramework.</summary>
	[Description("A component that automatically binds to the DataContext in the current AppFramework.")]
	[SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped", Justification = "BindingSource")]
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "BindingSource")]
	[SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers", Justification = "BindingSource")]
	public class FrameworkBindingSource : BindingSource {
		///<summary>Creates a FrameworkBindingSource.</summary>
		public FrameworkBindingSource() : base() { Init(); }
		///<summary>Creates a FrameworkBindingSource and adds it to a container.</summary>
		public FrameworkBindingSource(IContainer container) : base(container) { container.Add(this); Init(); }
		void Init() {
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
			DataSource = DisplaySettings.GridManager.DefaultDataSource;
		}

		///<summary>Gets or sets the data source that the connector binds to.</summary>
		[Description("Gets or sets the data source that the connector binds to.")]
		[Category("Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[AttributeProvider(typeof(IListSource))]
		public new object DataSource {
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}
	}


	///<summary>A class that displays a splash screen.</summary>
	public interface ISplashScreen {
		///<summary>Shows the splash screen.  This is a blocking call.</summary>
		void RunSplash();

		///<summary>Sets the splash screen's loading message, if supported.</summary>
		///<param name="message">A message to display on the splash screen.</param>
		void SetCaption(string message);

		///<summary>Indicates whether this splash screen can display a loading message.</summary>
		bool SupportsCaption { get; }

		///<summary>Closes the splash screen.</summary>
		void CloseSplash();
	}
}