using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ShomreiTorah.WinForms {
	///<summary>Saves the contents of the clipboard, restoring it when disposed.</summary>
	public sealed class ClipboardScope : IDisposable {
		IDataObject contents;

		///<summary>Creates a new ClipboardScope, capturing the contents of the clipboard.</summary>
		public ClipboardScope() { try { contents = Clipboard.GetDataObject(); } catch (ExternalException) { } }

		///<summary>Restores the contents of the clipboard that were captured by the constructor.</summary>
		public void Dispose() {
			if (contents != null) {
				try { Clipboard.SetDataObject(contents, true); } catch (ExternalException) { }
				contents = null;
			}
		}
	}
}
