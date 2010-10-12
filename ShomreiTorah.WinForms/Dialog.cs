using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ShomreiTorah.WinForms {
	///<summary>Shows dialog boxes.</summary>
	public static class Dialog {
		///<summary>Gets or sets the default caption for a message box.</summary>
		public static string DefaultTitle { get; set; }

		///<summary>Displays an informative message box.</summary>
		public static void Inform(string text) { Inform(text, DefaultTitle); }
		///<summary>Displays an informative message box.</summary>
		public static void Inform(string text, string title) { Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information); }

		///<summary>Displays an error message.</summary>
		public static void ShowError(string text) { ShowError(text, DefaultTitle); }
		///<summary>Displays an error message.</summary>
		public static void ShowError(string text, string title) { Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error); }

		///<summary>Displays a confirmation message.</summary>
		///<returns>True if the user clicked Yes.</returns>
		public static bool Confirm(string text) { return Confirm(text, DefaultTitle); }
		///<summary>Displays a confirmation message.</summary>
		///<returns>True if the user clicked Yes.</returns>
		public static bool Confirm(string text, string title) { return DialogResult.Yes == Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question); }

		///<summary>Displays a warning message.</summary>
		///<returns>True if the user clicked Yes.</returns>
		public static bool Warn(string text) { return Warn(text, DefaultTitle); }
		///<summary>Displays a warning message.</summary>
		///<returns>True if the user clicked Yes.</returns>
		public static bool Warn(string text, string title) { return DialogResult.Yes == Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning); }

		///<summary>Displays a message box.</summary>
		public static DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon) { return Show(text, DefaultTitle, buttons, icon); }
		///<summary>Displays a message box.</summary>
		public static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
			return XtraMessageBox.Show(text, title, buttons, icon);
		}
	}
}
