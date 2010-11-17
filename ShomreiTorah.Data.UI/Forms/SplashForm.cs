using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ShomreiTorah.Data.UI.Properties;

namespace ShomreiTorah.Data.UI.Forms {
	///<summary>Displays a splash screen while an application loads, optionally with a status message.</summary>
	public class SplashForm : Form, ISplashScreen {
		readonly Rectangle captionBounds;
		readonly Bitmap captionedImage;
		readonly Brush captionBrush;

		readonly ManualResetEventSlim shownWaiter = new ManualResetEventSlim();

		///<summary>Creates a splash screen showing the given image, without a caption.</summary>
		public SplashForm(Bitmap image) : this(image, Rectangle.Empty, Color.Empty) { }
		///<summary>Creates a splash screen showing the given image, with a caption at the given location.</summary>
		public SplashForm(Bitmap image, Rectangle captionBounds, Color captionColor) {
			if (image == null) throw new ArgumentNullException("image");

			this.Icon = Resources.LoadingIcon;
			this.BackgroundImage = image;
			this.Size = image.Size;
			this.captionBounds = captionBounds;
			this.captionBrush = new SolidBrush(captionColor);
			this.FormBorderStyle = FormBorderStyle.None;

			SetStyle(ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint, true);

			if (SupportsCaption) {
				captionedImage = new Bitmap(image.Width, image.Height);
				captionedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			} else
				captionedImage = image;
		}

		///<summary>Raises the Shown event.</summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			#region Handle monitor span
			Size = captionedImage.Size;

			Screen desktop = Screen.FromPoint(Control.MousePosition);

			var width = desktop.WorkingArea.Width;
			if (desktop.Bounds.Width > 2 * desktop.Bounds.Height)
				width /= 2;
			Rectangle screenRect = desktop.WorkingArea;
			Location = new Point(
				Math.Max(screenRect.X, screenRect.X + (width - Width) / 2),
				Math.Max(screenRect.Y, screenRect.Y + (screenRect.Height - Height) / 2)
			);

			#endregion
			shownWaiter.Set();
			if (SupportsCaption)
				SetCaption("Loading");		//SetCaption calls UpdateWindow
			else
				UpdateWindow();
		}

		///<summary>Gets parameters used to create the window handle.</summary>
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= NativeMethods.LayeredWindow;						//To support transparency, the window must be a layered window.
				return createParams;
			}
		}
		readonly StringFormat captionFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

		///<summary>Sets the splash screen's loading message, if supported.</summary>
		///<param name="message">A message to display on the splash screen.</param>
		public void SetCaption(string message) {
			if (!SupportsCaption) return;

			if (!IsHandleCreated) shownWaiter.Wait();
			if (InvokeRequired) {
				BeginInvoke(new Action<string>(SetCaption), message);
				return;
			}

			using (var g = Graphics.FromImage(captionedImage)) {
				g.Clear(Color.Transparent);
				g.DrawImage(BackgroundImage, Point.Empty);
				g.DrawString(message, SystemFonts.CaptionFont, captionBrush, captionBounds, captionFormat);
			}
			UpdateWindow();
		}

		static Point Zero;
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		void UpdateWindow() {
			IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr imageDC = NativeMethods.CreateCompatibleDC(screenDC);
			IntPtr gdiBitmap = IntPtr.Zero;
			IntPtr oldBitmap = IntPtr.Zero;

			try {
				gdiBitmap = captionedImage.GetHbitmap(Color.FromArgb(0));		//Get a GDI handle to the image.
				oldBitmap = NativeMethods.SelectObject(imageDC, gdiBitmap);		//Select the image into the DC, and cache the old bitmap.

				Size size = captionedImage.Size;								//Get the size and location of the form, as integers.
				Point location = this.Location;

				BlendFunction alphaInfo = new BlendFunction { SourceConstantAlpha = 255, AlphaFormat = 1 };	//This struct provides information about the opacity of the form.

				NativeMethods.UpdateLayeredWindow(Handle, screenDC, ref location, ref size, imageDC, ref Zero, 0, ref alphaInfo, UlwType.Alpha);
			} finally {
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);					//Release the Screen's DC.

				if (gdiBitmap != IntPtr.Zero) {									//If we got a GDI bitmap,
					NativeMethods.SelectObject(imageDC, oldBitmap);				//Select the old bitmap into the DC
					NativeMethods.DeleteObject(gdiBitmap);						//Delete the GDI bitmap,
				}
				NativeMethods.DeleteDC(imageDC);								//And delete the DC.
			}
			Invalidate();
		}


		///<summary>Releases the unmanaged resources used by the SplashForm and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				captionFormat.Dispose();
				BackgroundImage.Dispose();
				captionedImage.Dispose();
				captionBrush.Dispose();
				shownWaiter.Dispose();
			}
			base.Dispose(disposing);
		}

		///<summary>Shows the splash screen.  This is a blocking call.</summary>
		public void RunSplash() {
			Application.Run(this);
		}

		void InvokeIfNeeded(Action method) {
			if (IsDisposed) return;
			if (!IsHandleCreated) shownWaiter.Wait();
			if (InvokeRequired)
				BeginInvoke(method);
			else
				method();
		}


		///<summary>Indicates whether this splash screen can display a loading message.</summary>
		public bool SupportsCaption { get { return !captionBounds.IsEmpty; } }

		///<summary>Closes the splash screen.</summary>
		public void CloseSplash() { InvokeIfNeeded(Close); }
	}
}
