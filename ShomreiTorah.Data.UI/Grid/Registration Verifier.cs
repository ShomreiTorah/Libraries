using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ShomreiTorah.WinForms;

namespace ShomreiTorah.Data.UI.Grid {
	partial class SmartGrid {
		static int ActualRegistrationCount { get { return DisplaySettings.GridManager.RegistrationCount; } }

#if DEBUG
		int registrationCount;
#endif
		///<summary>For internal use.</summary>
		///<remarks>This property is used to make sure that all behavior registrations
		///occur both at design-time and at runtime.  The designer will emit code that
		///sets the property to the number of registrations.  In debug builds, the setter
		///will ensure at runtime that the number of registrations hasn't changed.</remarks>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int RegistrationCount {
			get { return ActualRegistrationCount; }
			set {
#if DEBUG
				if (registrationCount != 0)
					throw new InvalidOperationException("RegistrationCount cannot be set twice");
				registrationCount = value;
				//if (!IsDesignMode) {
				//    if (value != ActualRegistrationCount)
				//        Dialog.ShowError("Missing " + (value - ActualRegistrationCount) + " behavior registrations.\r\n"
				//                       + "Make sure that all behaviors are registered both at design-time and at runtime.",
				//                         "Shomrei Torah UI Framework");
				//}
#endif
			}
		}
	}
}