using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.IO;

namespace ShomreiTorah.Common {
	///<summary>Interacts with an FTP server.</summary>
	public class FtpClient {
		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static readonly FtpClient Instance = new FtpClient(Config.GetElement("FTP"));
		}
		//If the property is set before it is first read, ReadAttribute will never be called
		static FtpClient defaultOverride;

		///<summary>Gets or sets the default FtpClient instance.</summary>
		public static FtpClient Default {
			get { return defaultOverride ?? DefaultContainer.Instance; }
			set { defaultOverride = value; }
		}


		///<summary>Creates an FtpClient from an element in ShomreiTorahConfig.xml.</summary>
		public FtpClient(XElement configElement)
			: this((configElement.Attribute("Server") ?? configElement.Attribute("Host")).Value,
				   (configElement.Attribute("Username") ?? configElement.Attribute("User")).Value,
				   configElement.Attribute("Password").Value,
				   bool.Parse(configElement.Attribute("SSL").Value)) { }
		///<summary>Creates an FtpClient.</summary>
		public FtpClient(string server, string userName, string password, bool ssl) {
			Server = server;
			UserName = userName;
			Password = password;
			UseSsl = ssl;

			host = new UriBuilder("ftp", server).Uri;
			login = new NetworkCredential(userName, password);
		}
		readonly Uri host;
		readonly NetworkCredential login;

		///<summary>Gets the server that this client connects to.</summary>
		public string Server { get; private set; }
		///<summary>Indicates whether the client connects using SSL.</summary>
		public bool UseSsl { get; private set; }
		///<summary>Gets the username that this client logs in as.</summary>
		public string UserName { get; private set; }
		///<summary>Gets the password that this client logs in with.</summary>
		public string Password { get; private set; }

		///<summary>Creates an FtpWebRequest for the given local path.</summary>
		///<param name="relativePath">The local path for the request.</param>
		public FtpWebRequest CreateRequest(Uri relativePath) {
			var retVal = WebRequest.Create(new Uri(host, relativePath)) as FtpWebRequest;
			retVal.Credentials = login;
			retVal.EnableSsl = UseSsl;
			return retVal;
		}

		///<summary>Uploads a file to the server.</summary>
		///<param name="relativePath">The relative path on the server to upload the file to.</param>
		///<param name="localPath">The path of a file on disk to upload.</param>
		public void UploadFile(Uri relativePath, string localPath) { UploadFile(relativePath, localPath, null); }

		///<summary>Uploads a file to the server.</summary>
		///<param name="relativePath">The relative path on the server to upload the file to.</param>
		///<param name="contents">The content to upload.</param>
		public void UploadFile(Uri relativePath, Stream contents) { UploadFile(relativePath, contents, null); }
		///<summary>Uploads a file to the server.</summary>
		///<param name="relativePath">The relative path on the server to upload the file to.</param>
		///<param name="localPath">The path of a file on disk to upload.</param>
		///<param name="progress">An IProgressReporter implementation to report the progress of the upload.</param>
		public void UploadFile(Uri relativePath, string localPath, IProgressReporter progress) {
			using (var contents = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				UploadFile(relativePath, contents, progress);
		}

		///<summary>Uploads a file to the server.</summary>
		///<param name="relativePath">The relative path on the server to upload the file to.</param>
		///<param name="contents">The content to upload.</param>
		///<param name="progress">An IProgressReporter implementation to report the progress of the upload.</param>
		public void UploadFile(Uri relativePath, Stream contents, IProgressReporter progress) {
			progress = progress ?? new EmptyProgressReporter();

			progress.Maximum = -1;

			var request = CreateRequest(relativePath);
			request.Method = WebRequestMethods.Ftp.UploadFile;
			using (var requestStream = request.GetRequestStream()) {
				contents.CopyTo(requestStream, progress);
			}
			if (!progress.WasCanceled)
				request.GetResponse().Close();
		}
	}
}
