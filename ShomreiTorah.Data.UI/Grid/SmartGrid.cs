using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using ShomreiTorah.Common;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid control that automatically reads column settings from metadata.</summary>
	[Description("A grid control that automatically reads column settings from metadata.")]
	public class SmartGrid : GridControl {
		protected override BaseView CreateDefaultView() {
			return CreateView("SmartGridView");
		}
		protected override void RegisterAvailableViewsCore(InfoCollection collection) {
			base.RegisterAvailableViewsCore(collection);
			collection.Add(SmartGridView.CreateRegistrator());
		}
	}
}
