using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ShomreiTorah.Common;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid control that automatically reads column settings from metadata.</summary>
	[Description("A grid control that automatically reads column settings from metadata.")]
	[DefaultProperty("DataMember")]
	[ToolboxBitmap(typeof(GridControl), "Bitmaps256.GridControl.bmp")]
	public partial class SmartGrid : GridControl {
		///<summary>Initializes a new SmartGrid instance.</summary>
		public SmartGrid() { Init(); }
		///<summary>Initializes a new SmartGrid instance.</summary>
		public SmartGrid(IContainer container) { container.Add(this); Init(); }
		void Init() {
			DisplaySettings.SettingsRegistrator.EnsureRegistered();
			Source = DisplaySettings.GridManager.DefaultDataSource;
		}

		private bool ShouldSerializeSource() { return Source != DisplaySettings.GridManager.DefaultDataSource; }
		private void ResetSource() { Source = DisplaySettings.GridManager.DefaultDataSource; }
		///<summary>Gets or sets the grid control's data source.</summary>
		[Category("Data")]
		[Description("Gets or sets the grid control's data source.")]
		[AttributeProvider(typeof(IListSource))]
		public object Source {
			get { return DataSource; }
			set { DataSource = value; }
		}

		///<summary>Gets or sets the grid control's data source.</summary>
		///<remarks>This property is replaced by Source to suppress its DefaultValueAttribute.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override object DataSource {
			get { return base.DataSource; }
			set { base.DataSource = value; }
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
