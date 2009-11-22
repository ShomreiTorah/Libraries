using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common {
	///<summary><para>Finds a running instance of an Office Application object.</para>
	///<para>Usage: Office&lt;Word.ApplicationClass&gt;.App</para></summary>
	///<typeparam name="TApp">The ApplicationClass class to retrieve (eg, Word.ApplicationClass).</typeparam>
	public static class Office<TApp> where TApp : class, new() {
		///<summary>Gets the Application instance.  If the application is not running, it will be started.</summary>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Office types")]
		public static TApp App {
			get {
				try {						//Make sure that the application is still open.
					visibleSetter(AppContainer.app, true);
				} catch (COMException) { AppContainer.app = GetApp(); }
				return AppContainer.app;
			}
		}

		///<summary>Gets whether the Office application is already running.</summary>
		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Office types")]
		public static bool IsRunning {
			get {
				try {
					return Marshal.GetActiveObject(comName) is TApp;
				} catch (COMException) { return false; }
			}
		}

		//These fields must be declared before the app field for GetApp to work
		static readonly Action<TApp, bool> visibleSetter = (Action<TApp, bool>)
			Delegate.CreateDelegate(typeof(Action<TApp, bool>), typeof(TApp).GetProperty("Visible").GetSetMethod());
		static readonly string comName = typeof(TApp).ToString().Replace("Microsoft.Office.Interop.", "").Replace("Class", "");

		static class AppContainer {
			//This must be in a separate class or 
			//it will run when IsRunning is called
			public static TApp app = GetApp();
		}

		static TApp GetApp() {
			TApp app;
			try {
				app = Marshal.GetActiveObject(comName) as TApp;
				if (app == null) app = new TApp();
				visibleSetter(app, true);
			} catch (COMException) { app = new TApp(); }
			return app;
		}
	}
}
