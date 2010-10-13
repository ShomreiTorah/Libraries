using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Data.UI.DisplaySettings {
	///<summary>Registers grid and column behaviors.</summary>
	static class SettingsRegistrator {
		static void InitializeStandardSettings() {
			//This method will only be called once


		}

		static SettingsRegistrator() { InitializeStandardSettings(); }

		///<summary>Ensures that all grid settings have been registered.</summary>
		public static void EnsureRegistered() { }
	}
}
