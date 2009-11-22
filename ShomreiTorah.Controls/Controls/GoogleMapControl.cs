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

namespace ShomreiTorah.Controls {
	///<summary>A control that displays a Google Map of an address.</summary>
	[Description("A control that displays a Google Map of an address.")]
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(HebrewCalendar), "Images.GoogleMaps.png")]
	[DefaultEvent("SelectionChanged")]
	[DefaultProperty("Address")]
	public partial class GoogleMapControl : XtraUserControl {
		const string ApiKey = "ABQIAAAAxElVnCsXmGsU2ZxWYPP3bhQz-N-4Jc1YS__jTpeG4G3_qb4iFxRvPsoEDVZ1mMOP9BvHOw4WIvAz8A";
		readonly WebClient geoCoder = new WebClient();

		MapType mapType;
		int zoom = 15;
		string addressString = "Passaic, NJ";

		string latitude, longitude;

		int updateCount;
		///<summary>Creates a new GoogleMapsControl.</summary>
		public GoogleMapControl() {
			InitializeComponent();
			geoCoder.DownloadStringCompleted += geoCoder_DownloadStringCompleted;
			UpdateMap();
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
				addressString = value; DoUpdate();
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
		#endregion

		///<summary>Prevents the map from being downloaded until EndUpdate is called.</summary>
		public void BeginUpdate() { updateCount++; }
		///<summary>Re-enables autmatic downlaod after calling BeginUpdate.</summary>
		public void EndUpdate() { if (--updateCount == 0 && updateQueued) DoUpdate(); }

		object currentVersion;

		bool updateQueued;
		///<summary>Downloads the map from the server.</summary>
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
			currentVersion = new object();
			pictureBox.Cursor = String.IsNullOrEmpty(AddressString) ? null : Cursors.Hand;
			pictureBox.Show();

			if (String.IsNullOrEmpty(AddressString)) {
				pictureBox.Image = null;
				return;
			}

			pictureBox.CancelAsync();
			geoCoder.CancelAsync();
			pictureBox.Image = pictureBox.InitialImage;

			var geoCodeUri = AddKey("http://maps.google.com/maps/geo?output=csv&oe=utf8&q=" + Uri.EscapeDataString(addressString));
			try {
				geoCoder.DownloadStringAsync(geoCodeUri, currentVersion);
			} catch (WebException ex) { SetError(ex.Message); }
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
		void geoCoder_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			if (e.UserState != currentVersion) return;
			latitude = longitude = null;
			if (e.Error != null)
				SetError(e.Error.Message);
			else if (string.IsNullOrEmpty(e.Result))
				SetError("Google didn't reply");
			else {
				var parts = e.Result.Split(',');
				if (parts.Length == 4 && parts[0] == "200") {
					latitude = parts[2];
					longitude = parts[3];

					LoadMapImage();
				} else SetError(e.Result);
			}
		}
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lowercase parameter")]
		void LoadMapImage() {
			if (latitude == null) return;
			pictureBox.CancelAsync();
			var mapTypeString = MapType == MapType.Road ? "roadmap" : MapType.ToString().ToLowerInvariant();

			pictureBox.LoadAsync(AddKey(String.Format(CultureInfo.InvariantCulture,
													  "http://maps.google.com/staticmap?size={0}x{1}&zoom={2}&maptype={3}&markers={4},{5},blue",
													  640, 640, Zoom, mapTypeString, latitude, longitude)).ToString());
		}
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error message")]
		private void pictureBox_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(AddressString))
				ThreadPool.QueueUserWorkItem(delegate {
					try {
						Process.Start("http://maps.google.com/maps?q=" + Uri.EscapeDataString(AddressString));
					} catch (Exception ex) {
						MessageBox.Show("An error occurred while opening a map of " + addressString + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				});
		}

		void SetError(string text) {
			message.Text = text;
			pictureBox.Hide();
		}

		private void pictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e) {
			if (e.Error != null)
				SetError(e.Error.Message);
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
