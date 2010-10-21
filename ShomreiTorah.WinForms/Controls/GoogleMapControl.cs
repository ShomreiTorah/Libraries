using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DevExpress.Utils;
using System.Text.RegularExpressions;

namespace ShomreiTorah.WinForms.Controls {
	///<summary>A control that displays a Google Map of an address.</summary>
	[Description("A control that displays a Google Map of an address.")]
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(GoogleMapControl), "Images.GoogleMaps.png")]
	[DefaultEvent("SelectionChanged")]
	[DefaultProperty("Address")]
	public partial class GoogleMapControl : XtraUserControl {
		const string ApiKey = "ABQIAAAAxElVnCsXmGsU2ZxWYPP3bhQz-N-4Jc1YS__jTpeG4G3_qb4iFxRvPsoEDVZ1mMOP9BvHOw4WIvAz8A";
		readonly SuperToolTip tooltip = new SuperToolTip();
		readonly ToolTipTitleItem tooltipTitle;
		readonly ToolTipItem tooltipContents;

		MapType mapType;
		int zoom = 15;
		string addressString = "Passaic, NJ";

		int updateCount;
		///<summary>Creates a new GoogleMapsControl.</summary>
		public GoogleMapControl() {
			InitializeComponent();
			UpdateMap();

			tooltipTitle = tooltip.Items.AddTitle("Address");
			tooltipContents = tooltip.Items.Add(AddressString);
			toolTipController.SetSuperTip(this, tooltip);
			toolTipController.SetSuperTip(pictureBox, tooltip);
		}

		#region Properties
		///<summary>Gets or sets the zoom level for the map.</summary>
		[Description("Gets or sets the zoom level for the map.")]
		[Category("Map")]
		[DefaultValue(15)]
		public int Zoom {
			get { return zoom; }
			set {
				if (Zoom == value) return;
				zoom = value; DoUpdate();
			}
		}

		///<summary>Gets or sets the address being displayed.</summary>
		[Description("Gets or sets the address being displayed.")]
		[Category("Data")]
		[DefaultValue("Passaic, NJ")]
		public string AddressString {
			get { return addressString; }
			set {
				if (AddressString == value) return;
				addressString = value;
				tooltipContents.Text = (value ?? "").Replace("&", "&&");
				DoUpdate();
			}
		}

		///<summary>Gets or sets the type of map to display.</summary>
		[Description("Gets or sets the type of map to display.")]
		[Category("Map")]
		[DefaultValue(MapType.Road)]
		public MapType MapType {
			get { return mapType; }
			set {
				if (MapType == value) return;
				mapType = value; DoUpdate();
			}
		}
		///<summary>Gets or sets the title of the location being displayed.</summary>
		[Description("Gets or sets the title of the location being displayed.")]
		[Category("Data")]
		[Bindable(true)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(true)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				tooltipTitle.Text = value == null ? "Address" : value.Replace("&", "&&");
			}
		}
		#endregion
		static readonly Regex addressCleaner = new Regex(@"\s+");
		string CleanAddress { get { return addressCleaner.Replace(AddressString ?? "", " "); } }

		///<summary>Prevents the map from being downloaded until EndUpdate is called.</summary>
		public void BeginUpdate() { updateCount++; }
		///<summary>Re-enables automatic download after calling BeginUpdate.</summary>
		public void EndUpdate() { if (--updateCount == 0 && updateQueued) DoUpdate(); }

		bool updateQueued;
		///<summary>Downloads the map from the server, unless updates are blocked.</summary>
		void DoUpdate() {
			if (updateCount > 0) {
				updateQueued = true;
				return;
			}
			updateCount = 0;
			UpdateMap();
		}

		///<summary>Downloads the map image from the server.</summary>
		public void UpdateMap() {
			updateQueued = false;

			if (String.IsNullOrEmpty(CleanAddress)) {
				pictureBox.Cursor = null;
				pictureBox.Image = null;
			} else {
				pictureBox.Cursor = Cursors.Hand;
				LoadMapImage();
			}
			pictureBox.Show();
		}
		static Uri AddKey(string url) {
			var builder = new UriBuilder(url);
			var keyString = "key=" + ApiKey;

			if (String.IsNullOrEmpty(builder.Query))
				builder.Query = keyString;
			else
				builder.Query = builder.Query.Substring(1) + "&" + keyString;

			return builder.Uri;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lowercase parameter")]
		void LoadMapImage() {
			if (String.IsNullOrEmpty(AddressString)) return;
			pictureBox.CancelAsync();
			var mapTypeString = MapType == MapType.Road ? "roadmap" : MapType.ToString().ToLowerInvariant();

			pictureBox.LoadAsync(AddKey(String.Format(CultureInfo.InvariantCulture,
													  "http://maps.google.com/maps/api/staticmap?size={0}x{1}&zoom={2}&maptype={3}&markers=color:blue|{4}&sensor=false",
													  640, 640, Zoom, mapTypeString, Uri.EscapeDataString(CleanAddress))).ToString());
		}
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error message")]
		private void pictureBox_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(AddressString))
				ThreadPool.QueueUserWorkItem(delegate {
					try {
						Process.Start("http://maps.google.com/maps?q=" + Uri.EscapeDataString(CleanAddress + (String.IsNullOrEmpty(Text) ? "" : " (" + Text + ")")));
					} catch (Exception ex) {
						XtraMessageBox.Show("An error occurred while opening a map of " + AddressString + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				});
		}

		void SetError(string text) {
			message.Text = text;
			pictureBox.Hide();
		}

		private void pictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e) {
			var wex = e.Error as WebException;
			if (wex != null && wex.Response != null) {
				using (var stream = wex.Response.GetResponseStream())
				using (var reader = new StreamReader(stream)) {
					SetError(reader.ReadToEnd());
				}
			} else if (e.Error != null)
				SetError(e.Error.Message);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				tooltip.Dispose();
				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	///<summary>Specifies a type of map.</summary>
	public enum MapType {
		///<summary>A standard road map.  This is the default map type.</summary>
		Road,
		///<summary>A satellite image.</summary>
		Satellite,
		///<summary>A satellite image with road labels.</summary>
		Hybrid,
		///<summary>A physical relief map.</summary>
		Terrain
	}
}
