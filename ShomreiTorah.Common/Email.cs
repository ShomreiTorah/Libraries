using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Xml.Linq;
using System.Net;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Common {
	///<summary>Sends emails using the Shul's mail servers from ShomreiTorahConfig.xml.</summary>
	///<remarks>Reads SMTP settings from ShomreiTorahConfig.</remarks>
	public class Email {
		#region Email addresses
		///<summary>A MailAddress instance used to send email to the mailing list.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MailAddress is immutable")]
		public static readonly MailAddress ListAddress = new MailAddress("List@ShomreiTorah.us", "Congregation Shomrei Torah");
		///<summary>A MailAddress instance used to send generic emails.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MailAddress is immutable")]
		public static readonly MailAddress InfoAddress = new MailAddress("Info@ShomreiTorah.us", "Congregation Shomrei Torah");
		///<summary>A MailAddress instance used to send administrative emails.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MailAddress is immutable")]
		public static readonly MailAddress AdminAddress = new MailAddress("Admin@ShomreiTorah.us", "Shomrei Torah Administration");
		///<summary>A MailAddress instance used to send notification emails.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MailAddress is immutable")]
		public static readonly MailAddress AlertsAddress = new MailAddress("Alerts@ShomreiTorah.us", "Shomrei Torah Alerts");
		#endregion

		#region Static instances
		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static readonly Email Instance = Config.ReadAttribute("SMTP", "Default") == "Hosted" ? Hosted : Gmail;
		}
		//If the property is set before it is first read, ReadAttribute will never be called
		static Email defaultOverride;

		///<summary>Gets or sets the default Email instance.</summary>
		public static Email Default {
			get { return defaultOverride ?? DefaultContainer.Instance; }
			set { defaultOverride = value; }
		}

		///<summary>Gets an Email instance for Google Apps.</summary>
		///<remarks>This server cannot send more than 25 messages at once.</remarks>
		public static Email Gmail { get { return GmailContainer.Instance; } }
		static class GmailContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static GmailContainer() { }
			public static readonly Email Instance = new Email(Config.GetElement("SMTP", "Gmail"));
		}


		///<summary>Gets an Email instance for the SMTP server provided by the Shul's web host.</summary>
		public static Email Hosted { get { return HostedContainer.Instance; } }
		static class HostedContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static HostedContainer() { }
			public static readonly Email Instance = new Email(Config.GetElement("SMTP", "Hosted"));
		}
		#endregion

		///<summary>Creates an Email instance from an XElement in ShormeiTorahConfig.xml</summary>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Co-constructor call")]
		public Email(XElement configElement)
			: this((configElement.Attribute("Server") ?? configElement.Attribute("Host")).Value,
					int.Parse(configElement.Attribute("Port").Value, CultureInfo.InvariantCulture),
					bool.Parse(configElement.Attribute("SSL").Value),
					configElement.Attribute("Password").Value) { }


		///<summary>Creates an Email instance.</summary>
		public Email(string host, int port, bool ssl, string password) {
			SmtpServer = host;
			Port = port;
			EnableSsl = ssl;
			Password = password;
		}

		///<summary>To be used by derived classes, if any.</summary>
		protected Email() { }

		#region SMTP Settings
		///<summary>Gets the SMTP server used by this instance.</summary>
		public string SmtpServer { get; protected set; }
		///<summary>Gets the port number used by this instance.</summary>
		public int Port { get; protected set; }
		///<summary>Gets whether this instance connects using SSL.</summary>
		public bool EnableSsl { get; protected set; }
		///<summary>Gets the password for the email accounts on this instance.</summary>
		public string Password { get; protected set; }

		///<summary>Creates an SMTP client for the given username.</summary>
		///<param name="userName">The username to login as.</param>
		public virtual SmtpClient CreateSmtp(string userName) {
			return new SmtpClient { Credentials = new NetworkCredential(userName, Password), EnableSsl = EnableSsl, Host = SmtpServer, Port = Port };
		}
		#endregion
		///<summary>The encoding to be used for MailMessages.</summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Encoding is immutable")]
		public static readonly Encoding DefaultEncoding = Encoding.UTF8;

		///<summary>Sends a notification email from Alerts@ShomreiTorah.us to Info@ShomreiTorah.us.</summary>
		public static void Notify(string subject, string body) { Notify(subject, body, false); }
		///<summary>Sends a notification email from Alerts@ShomreiTorah.us.</summary>
		public static void Notify(MailAddress to, string subject, string body) { Notify(to, subject, body, false); }
		///<summary>Sends a notification email from Alerts@ShomreiTorah.us to Info@ShomreiTorah.us.</summary>
		public static void Notify(string subject, string body, bool html) { Notify(InfoAddress, subject, body, html); }
		///<summary>Sends a notification email from Alerts@ShomreiTorah.us.</summary>
		public static void Notify(MailAddress to, string subject, string body, bool html) { Default.SendAsync(AlertsAddress, to, subject, body, html); }

		///<summary>Sends a technical alert to Admin@ShomreiTorah.us.</summary>
		public static void Warn(string subject, string body) { Notify(AdminAddress, subject, body); }

		#region Send overloads
		///<summary>Sends a MailMessage synchronously.</summary>
		public void Send(MailMessage message) {
			if (message == null) throw new ArgumentNullException("message");
			CreateSmtp(message.From.Address).Send(message);
		}
		///<summary>Sends an email from Info@ShomreiTorah.us (or Alerts if sent to Info) synchronously.</summary>
		public void Send(MailAddress to, string subject, string body) { Send(to, subject, body, false); }
		///<summary>Sends an email from Info@ShomreiTorah.us (or Alerts if sent to Info) synchronously.</summary>
		public void Send(MailAddress to, string subject, string body, bool html) { Send(InfoAddress.Equals(to) ? AlertsAddress : InfoAddress, to, subject, body, html); }
		///<summary>Sends an email synchronously.</summary>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "CA Bug")]
		public void Send(MailAddress from, MailAddress to, string subject, string body, bool html) {
			if (to == null) throw new ArgumentNullException("to");
			if (from == null) throw new ArgumentNullException("from");


			using (var message = new MailMessage(from, to) {
				BodyEncoding = DefaultEncoding,
				SubjectEncoding = DefaultEncoding,
				Subject = subject,
				Body = body,
				IsBodyHtml = html
			})
				Send(message);
		}

		///<summary>Sends a MailMessage asynchronously.</summary>
		public void SendAsync(MailMessage message) {
			if (message == null) throw new ArgumentNullException("message");
			CreateSmtp(message.From.Address).SendAsync(message, null);
		}
		///<summary>Sends an email from Info@ShomreiTorah.us (or Alerts if sent to Info) asynchronously.</summary>
		public void SendAsync(MailAddress to, string subject, string body) { SendAsync(to, subject, body, false); }
		///<summary>Sends an email from Info@ShomreiTorah.us (or Alerts if sent to Info) asynchronously.</summary>
		public void SendAsync(MailAddress to, string subject, string body, bool html) { SendAsync(InfoAddress.Equals(to) ? AlertsAddress : InfoAddress, to, subject, body, html); }
		///<summary>Sends an email asynchronously.</summary>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "CA Bug")]
		public void SendAsync(MailAddress from, MailAddress to, string subject, string body, bool html) {
			if (to == null) throw new ArgumentNullException("to");
			if (from == null) throw new ArgumentNullException("from");

			var message = new MailMessage(from, to) {
				BodyEncoding = DefaultEncoding,
				SubjectEncoding = DefaultEncoding,
				Subject = subject,
				Body = body,
				IsBodyHtml = html
			};

			var smtp = CreateSmtp(from.Address);
			smtp.SendCompleted += delegate { message.Dispose(); };
			smtp.SendAsync(message, null);
		}
		#endregion
	}
}
