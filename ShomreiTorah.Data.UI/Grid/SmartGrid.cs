using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using ShomreiTorah.Common;
using DevExpress.XtraGrid.Views.Grid;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid control that automatically reads column settings from metadata.</summary>
	[Description("A grid control that automatically reads column settings from metadata.")]
	[DefaultProperty("DataMember")]
	public partial class SmartGrid : GridControl {
		///<summary>Initializes a new SmartGrid instance.</summary>
		public SmartGrid() {
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
		}

		protected override BaseView CreateDefaultView() {
			return CreateView("SmartGridView");
		}
		protected override void RegisterAvailableViewsCore(InfoCollection collection) {
			base.RegisterAvailableViewsCore(collection);
			collection.Add(SmartGridView.CreateRegistrator());
		}

		protected internal new BaseView MouseCaptureOwner {
			get { return base.MouseCaptureOwner; }
			set { base.MouseCaptureOwner = value; }
		}
	}
}
