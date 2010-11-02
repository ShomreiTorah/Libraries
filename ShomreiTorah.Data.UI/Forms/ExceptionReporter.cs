using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using DevExpress.XtraEditors;
using ShomreiTorah.Common;
using ShomreiTorah.WinForms;
using System.Threading;

namespace ShomreiTorah.Data.UI.Forms {
	///<summary>Displays an unhandled exception to the user.  This form is called by AppFramework.HandleException.</summary>
	public sealed partial class ExceptionReporter : XtraForm {
		readonly Exception exception;
		readonly Stream imageStream;

		///<summary>Creates an ExceptionReporter form that reports the given exception.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore all GDI+ errors")]
		public ExceptionReporter(Exception exception) {
			if (exception == null) throw new ArgumentNullException("exception");
			InitializeComponent();


			while (exception is TargetInvocationException)
				exception = exception.InnerException;

			try {
				imageStream = SaveScreenshot();
			} catch { }

			this.exception = exception;
			emailWorker.RunWorkerAsync();

			Text = Dialog.DefaultTitle + " Error";
			errorDetails.Text = exception.ToString();
		}
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			errorDetails.DeselectAll();
		}
		static Stream SaveDB() {
			var retVal = new MemoryStream();

			using (var compressor = new GZipStream(retVal, CompressionMode.Compress, true))
			using (var writer = XmlWriter.Create(compressor))
				AppFramework.Current.DataContext.ToXml().WriteTo(writer);

			retVal.Position = 0;
			return retVal;
		}

		static Stream SaveScreenshot() {
			var retVal = new MemoryStream();
			var screen = SystemInformation.VirtualScreen;
			using (var image = new Bitmap(screen.Width, screen.Height))
			using (var g = Graphics.FromImage(image)) {
				g.CopyFromScreen(screen.Location, Point.Empty, screen.Size);
				image.Save(retVal, ImageFormat.Jpeg);
			}
			retVal.Position = 0;
			return retVal;
		}

		private void emailWorker_DoWork(object sender, DoWorkEventArgs e) {
			Thread.Sleep(6500);
			using (var message = new MailMessage(Email.AlertsAddress, Email.AdminAddress)) {
				//TODO: Version
				message.Subject = Dialog.DefaultTitle + " Error from " + Environment.MachineName + "\\" + Environment.UserName;
				message.Body = exception.ToString();

				if (AppFramework.Current != null && AppFramework.Current.DataContext != null)
					message.Attachments.Add(new Attachment(SaveDB(), "Data.xml.gz", "application/x-gzip"));

				if (imageStream != null)
					message.Attachments.Add(new Attachment(imageStream, "Screen.jpeg", MediaTypeNames.Image.Jpeg));

				Email.Default.Send(message);
			}
		}

		private void emailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			loadingPanel.Hide();
			if (e.Error != null) {
				sendStatusLabel.Text = "A different error occurred while sending the error report.";
				sendStatusLabel.SuperTip = Utilities.CreateSuperTip(e.Error.GetType().ToString(), e.Error.ToString());
				sendStatusLabel.SuperTip.MaxWidth = 3 * Screen.GetWorkingArea(this).Width / 4;
			} else
				sendStatusLabel.Text = "The error has been emailed to our IT department.";
			sendStatusLabel.Show();
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			if (e.KeyData == Keys.Escape)
				Close();
		}

		private void errorDetails_KeyUp(object sender, KeyEventArgs e) {
			if (e.KeyData == Keys.Escape)
				Close();
		}
	}
}