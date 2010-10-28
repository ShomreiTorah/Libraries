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
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;
using ShomreiTorah.WinForms;
using ShomreiTorah.WinForms.Forms;

namespace ShomreiTorah.Data.UI {
	///<summary>The base class for a standard ShomreiTorah application.</summary>
	public abstract class AppFramework {
		///<summary>Automatically registers the AppFramework for the current project at design time.</summary>
		public static void AutoRegisterDesigner() {
			if (Current != null) return;
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) return;

			var dh = (IDesignerHost)LicenseManager.CurrentContext.GetService(typeof(IDesignerHost));
			if (dh == null) return;

			var rootType = Type.GetType(GetTargetNamespace(dh) + "." + dh.RootComponentClassName);
			//TODO: TypeScan fall-back if GetTargetNamespace fails; error handling

			if (rootType.Assembly.EntryPoint == null) return;	//Type is in a DLL

			var appFrameworkType = rootType.Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(AppFramework)));

			if (appFrameworkType == null) {
				MessageBox.Show("Cannot find an AppFramework type in " + rootType.Assembly + ".\r\nTry rebuilding the project.",
								"Shomrei Torah Design System", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			CheckDesignTime((AppFramework)Activator.CreateInstance(appFrameworkType));
		}

		static string GetTargetNamespace(IDesignerHost dh) {
			var _loader = dh.GetType().GetField("_loader", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dh);
			var CodeDom = _loader.GetType().GetProperty("CodeDom", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_loader, null);

			var TypeNamespace = (CodeNamespace)CodeDom.GetType().GetProperty("TypeNamespace", BindingFlags.Public | BindingFlags.Instance).GetValue(CodeDom, null);

			return TypeNamespace.Name;
		}


		///<summary>Gets the typed table for the given row type from the current application's DataContext.</summary>
		public static TypedTable<TRow> Table<TRow>() where TRow : Row { return Current.DataContext.Table<TRow>(); }

		///<summary>Gets the AppFramework instance for the current application.</summary>
		///<remarks>This property is also available at design time.</remarks>
		public static AppFramework Current { get; private set; }

		bool isDesignTime = true;
		///<summary>Indicates whether the code is running in the Visual Studio designer.</summary>
		public bool IsDesignTime { get { return isDesignTime; } }

		///<summary>Registers an AppFramework instance for design time, if none is registered.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore design-time SQL errors")]
		protected static void CheckDesignTime(AppFramework instance) {
			if (Current != null)
				return;
			Current = instance;
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
			instance.RegisterSettings();
			instance.SyncContext = instance.CreateDataContext();

			try {
				instance.SyncContext.ReadData();
			} catch { instance.SyncContext = instance.CreateDataContext(); }	//Ignore SQL errors at design time
		}

		///<summary>Gets the application's main form.</summary>
		public Form MainForm { get; private set; }
		///<summary>Gets the Singularity DataContext used by the application.</summary>
		public DataContext DataContext { get { return SyncContext.DataContext; } }
		///<summary>Gets the DataSyncContext used to synchronize with a database server.</summary>
		public DataSyncContext SyncContext { get; private set; }

		///<summary>Overridden by derived classes to create the application's splash screen.  Called on the main thread.</summary>
		protected abstract ISplashScreen CreateSplash();
		///<summary>Overridden by derived classes to register grid and editor settings.</summary>
		protected abstract void RegisterSettings();
		///<summary>Creates the application's main form.</summary>
		protected abstract Form CreateMainForm();
		///<summary>Creates the main DataContext used by the application.</summary>
		///<returns>A DataSyncContext used to synchronize with a database server.</returns>
		protected abstract DataSyncContext CreateDataContext();


		///<summary>Runs the application.</summary>
		protected void Run() {
			if (Current != null)
				throw new InvalidOperationException("An application is already running");
			isDesignTime = false;
			Current = this;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			StartSplash();
			if (!Debugger.IsAttached) {
				Application.ThreadException += (sender, e) => HandleException(e.Exception);
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException((Exception)e.ExceptionObject);
			}

			SetSplashCaption("Loading behaviors");
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
			RegisterSettings();
			Debug.Assert(!String.IsNullOrWhiteSpace(Dialog.DefaultTitle), "Please set a dialog title (in RegisterSettings)");

			//TODO: Updates, Error handling
			SetSplashCaption("Reading database");
			SyncContext = CreateDataContext();
			SyncContext.ReadData();

			SetSplashCaption("Loading UI");
			MainForm = CreateMainForm();
			MainForm.Shown += delegate {
				if (splashScreen != null)
					splashScreen.CloseSplash();
				splashScreen = null;
			};
			Application.Run(MainForm);
		}

		///<summary>Prompts the user to create a new person.</summary>
		///<remarks>The new Person row, or null if the user clicked cancel.</remarks>
		public virtual Person PromptPerson() {
			return Forms.PersonCreator.Prompt();
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
		#endregion

		#region Database
		///<summary>Indicates whether there are any unsaved changes in the Singularity DataContext.</summary>
		public bool HasDataChanged {
			get { return SyncContext.Tables.Any(t => t.Changes.Count > 0); }
		}

		///<summary>Saves any changes made to the Singularity DataContext.</summary>
		public void SaveDatabase() {
			ProgressWorker.Execute(SyncContext.WriteData, cancellable: false);
		}

		///<summary>Reads any changes from the database server.</summary>
		public void RefreshDatabase() {
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
		///<summary>Handles an unhandled exception.</summary>
		public virtual void HandleException(Exception ex) {
			if (ex == null) throw new ArgumentNullException("ex");

			new Forms.ExceptionReporter(ex).Show(MainForm);
		}
		#endregion
	}

	///<summary>A component that automatically binds to the DataContext in the current AppFramework.</summary>
	[Description("A component that automatically binds to the DataContext in the current AppFramework.")]
	[SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped", Justification = "BindingSource")]
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "BindingSource")]
	[SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers", Justification = "BindingSource")]
	public class FrameworkBindingSource : BindingSource {
		public FrameworkBindingSource() : base() { Init(); }
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
