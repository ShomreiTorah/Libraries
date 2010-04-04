using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using ShomreiTorah.Common;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.IO;

namespace ShomreiTorah.Administration {
	///<summary>Manages the Shul's email list.</summary>
	public class MailingList {
		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static MailingList defaultInstance = new MailingList(DB.Default);
		}

		///<summary>Gets or sets the production mailing list.</summary>
		public static MailingList Default { get { return DefaultContainer.defaultInstance; } set { DefaultContainer.defaultInstance = value; } }

		static class TestContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static TestContainer() { }
			public static MailingList testInstance = new MailingList(DB.Test);
		}

		///<summary>Gets or sets the test mailing list.</summary>
		public static MailingList Test { get { return TestContainer.testInstance; } set { TestContainer.testInstance = value; } }

		///<summary>Creates a new MailingList instance.</summary>
		public MailingList(DBConnector database) {
			if (database == null) throw new ArgumentNullException("database");
			Database = database;
		}
		///<summary>Gets the database that this instance connects to.</summary>
		public DBConnector Database { get; private set; }

		///<summary>Creates a MailSender object that sends emails to this mailing list.</summary>
		///<param name="archive">If true, the MailSender will add the messages it sends to the newsletter archive.</param>
		public MailSender CreateSender(bool archive) { return new MailSender(this, archive); }

		///<summary>Adds an email address to the mailing list.</summary>
		///<param name="person">A MailAddress object containing the name and email address of the person to add.</param>
		public void Add(MailAddress person) { Add(person, false); }
		///<summary>Adds an email address to the mailing list.</summary>
		///<param name="person">A MailAddress object containing the name and email address of the person to add.</param>
		///<param name="noNotify">If true, no notification email about the subscription will be sent.</param>
		public void Add(MailAddress person, bool noNotify) {
			if (person == null) throw new ArgumentNullException("person");
			string emailBody = "Name:  " + person.DisplayName + Environment.NewLine + "Email: " + person.Address;

			using (var connection = Database.OpenConnection()) {
				if (0 != connection.Sql<int>("SELECT COUNT(*) FROM tblMlMembers WHERE LOWER(Email)=LOWER(@Address)").Execute(new { person.Address })) {
					if (!noNotify)
						Email.Notify(person.DisplayName + " is already on the email list", emailBody);
					throw new UserInputException(UserInputProblem.Duplicate);
				} else {
					connection.ExecuteNonQuery("INSERT INTO tblMlMembers(Name, Email, ID_Code) VALUES(@DisplayName, @Address, @ID)", new { person.DisplayName, person.Address, ID = HexValue(20) });
					if (!noNotify)
						Email.Notify(person.DisplayName + " was added to the email list", emailBody);
				}
			}
		}

		///<summary>Gets the number of subscribers in the mailing list from the database.</summary>
		public int MemberCount { get { return Database.ExecuteScalar<int>("SELECT COUNT(*) FROM tblMLMembers"); } }
		///<summary>Gets the subscribers to the mailing list from the database.</summary>
		public IEnumerable<Subscriber> Members {
			get {
				using (var reader = Database.ExecuteReader("SELECT Name, Email, ID_Code AS Code, Join_Date AS JoinDate FROM tblMLMembers")) {
					while (reader.Read()) {
						yield return new Subscriber(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3));
					}
				}
			}
		}


		///<summary>Finds a subscriber by email address and code, returning null if no such subscriber exists.</summary>
		///<returns>The Subscriber instance with the given email address and code, or null if there is no matching subscriber in the database.</returns>
		///<remarks>This method is used to verify raw input in the unsubscribe page.</remarks>
		public Subscriber? FindMember(string email, string code) {
			if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(code))
				return null;
			using (var reader = Database.ExecuteReader("SELECT Name, Email, ID_Code, Join_Date FROM tblMLMembers WHERE Email = @email AND ID_Code = @code", new { email, code })) {
				if (reader.Read())
					return new Subscriber(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3));
			}
			return null;
		}

		///<summary>Removes a member from the email list.</summary>
		///<returns>Whether the subscriber was successfully removed.</returns>
		public bool Unsubscribe(Subscriber subscriber) {
			var succeeded = (1 == Database.ExecuteNonQuery("DELETE FROM tblMLMembers WHERE Name = @DisplayName AND Email = @Address AND ID_Code = @Code",
														   new { subscriber.Address.DisplayName, subscriber.Address.Address, subscriber.Code }));
			if (succeeded) {
				Email.Notify("Email Unsubscribe  :-(",
					subscriber.Address.DisplayName + " (" + subscriber.Address.Address + ") has been removed from the email list.\r\n\r\n"
				  + "(joined on " + subscriber.JoinDate.ToString("F", CultureInfo.CurrentCulture) + ")");
			}
			return succeeded;
		}
		static readonly Random rand = new Random();
		static string HexValue(int length) {
			var retVal = new StringBuilder(length);
			for (int n = 0; n < length; n++) {
				retVal.Append(rand.Next(0, 16).ToString("X", CultureInfo.InvariantCulture));
			}

			return retVal.ToString();
		}
	}

	///<summary>A subscriber to the mailing list.</summary>
	public struct Subscriber : IEquatable<Subscriber> {
		///<summary>Initializes a Subscriber instance.</summary>
		public Subscriber(string name, string email, string code, DateTime joinDate) : this(new MailAddress(email, name), code, joinDate) { }
		///<summary>Initializes a Subscriber instance.</summary>
		public Subscriber(MailAddress address, string code, DateTime joinDate)
			: this() {
			if (address == null)
				throw new ArgumentNullException("address");
			if (String.IsNullOrEmpty(code))
				throw new ArgumentException("code is null or empty.", "code");

			Address = address;
			Code = code;
			JoinDate = joinDate;
		}

		///<summary>Gets the subscriber's email address.</summary>
		public MailAddress Address { get; private set; }
		///<summary>Gets a random code that identifies this subscriber.</summary>
		public string Code { get; private set; }

		///<summary>Gets the date that this subscriber joined the list.</summary>
		public DateTime JoinDate { get; private set; }

		///<summary>Checks whether this instance is equal to an object.</summary>
		public override bool Equals(object obj) { return obj is Subscriber && Equals((Subscriber)obj); }

		///<summary>Checks whether this instance is equal to another subscriber instance.</summary>
		public bool Equals(Subscriber other) { return Equals(Address, other.Address) && Code.Equals(other.Code, StringComparison.OrdinalIgnoreCase); }

		///<summary>Gets a hash code for this value.</summary>
		public override int GetHashCode() {
			if (Address == null) return Code.GetHashCode();
			return Address.GetHashCode() ^ Code.GetHashCode();
		}

		///<summary>Checks whether two Subscriber values are equal.</summary>
		public static bool operator ==(Subscriber first, Subscriber second) { return first.Equals(second); }
		///<summary>Checks whether two Subscriber values are UNequal.</summary>
		public static bool operator !=(Subscriber first, Subscriber second) { return !first.Equals(second); }
	}
	///<summary>Contains data for the MessageSending and MessageSent events.</summary>
	public class SendMailEventArgs : EventArgs {
		///<summary>Creates a SendMailEventArgs for a specific recipient.</summary>
		public SendMailEventArgs(Subscriber recipient, MailMessage message) { Recipient = recipient; Message = message; Timestamp = Timestamp; }
		///<summary>Gets the subscriber who will receive the message.</summary>
		public Subscriber Recipient { get; private set; }
		///<summary>Gets the message that will be sent to the subscriber.</summary>
		public MailMessage Message { get; private set; }
		///<summary>Gets the time that this event occurred.</summary>
		public DateTime Timestamp { get; private set; }
	}
	///<summary>Contains data for the MailSender.SendFailed event.</summary>
	public class SendMailErrorEventArgs : SendMailEventArgs {
		///<summary>Creates a SendMailErrorEventArgs.</summary>
		public SendMailErrorEventArgs(Subscriber recipient, MailMessage message, Exception ex) : base(recipient, message) { Exception = ex; }
		///<summary>Gets the exception that occurred while sending the message.</summary>
		public Exception Exception { get; private set; }
	}


	///<summary>Sends messages to the mailing list.</summary>
	public class MailSender {
		internal MailSender(MailingList list, bool archive) { List = list; Archive = archive; }

		///<summary>Gets the mailing list that this instance sends mail to.</summary>
		public MailingList List { get; private set; }

		///<summary>Gets whether this instance will add the messages it sends to the newsletter archive.</summary>
		public bool Archive { get; private set; }

		#region Send
		///<summary>Sends an email to the mailing list, and adds it to the archive.</summary>
		///<param name="subject">The subject of the message.</param>
		///<param name="htmlBody">The HTML body of the message.</param>
		public void Send(string subject, string htmlBody) { Send(new MailMessage { Subject = subject, Body = htmlBody, IsBodyHtml = true }); }
		///<summary>Sends a MailMessage to the mailing list.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Robust error handling")]
		public void Send(MailMessage message) {
			if (message == null)
				throw new ArgumentNullException("message", "message is null.");
			message.SubjectEncoding = message.BodyEncoding = Email.DefaultEncoding;
			message.Sender = message.From = Email.ListAddress;

			if (Archive) {
				List.Database.ExecuteNonQuery("INSERT INTO tblMLNewsletter(Newsletter_subject, Newsletter, Display, HTML) VALUES(@Subject, @Body, 1, @IsBodyHtml)",
											  new { message.Subject, message.Body, message.IsBodyHtml });
			}
			var rawBody = message.Body;

			var alternateViews = message.AlternateViews.Select(av => new {
				View = av,
				Text = new StreamReader(av.ContentStream, av.TransferEncoding == TransferEncoding.Base64 ? Encoding.UTF8 : Encoding.ASCII, true).ReadToEnd()
			}).ToArray();	//Note ToArray

			foreach (var recipient in List.Members) {
				//Uri.EscapeDataString will escape ' ' as '%20' instead of '+'
				//Since neither email addresses nor codes can contain spaces, 
				//this won't do any harm.
				var unsubscribeUrl = "www.ShomreiTorah.us/?Unsubscribe=" + Uri.EscapeDataString(recipient.Address.Address) + "&code=" + Uri.EscapeDataString(recipient.Code);

				if (!string.IsNullOrEmpty(rawBody))
					message.Body = InsertFooter(rawBody, unsubscribeUrl, message.IsBodyHtml);

				message.AlternateViews.Clear();
				foreach (var view in alternateViews) {
					var isHtml = view.View.ContentType.MediaType.EndsWith("html", StringComparison.OrdinalIgnoreCase);

					var newView = AlternateView.CreateAlternateViewFromString(InsertFooter(view.Text, unsubscribeUrl, isHtml), view.View.ContentType);

					newView.BaseUri = view.View.BaseUri;
					newView.ContentId = view.View.ContentId;

					foreach (var lr in view.View.LinkedResources)
						newView.LinkedResources.Add(lr);

					//I don't think I should do this
					//newView.TransferEncoding = view.View.TransferEncoding;
				}

				message.To.Clear();
				message.To.Add(recipient.Address);

				OnMessageSending(new SendMailEventArgs(recipient, message));
				try {
					Email.Hosted.Send(message);
				} catch (Exception ex) {
					OnSendFailed(new SendMailErrorEventArgs(recipient, message, ex));
					continue;
				}
				OnMessageSent(new SendMailEventArgs(recipient, message));
			}
		}

		static string InsertFooter(string rawBody, string unsubscribeUrl, bool html) {
			if (html) {
				var footer = @"<p style='margin-top:10px;border-top:1px solid #000094'>To unsubscribe, go to <a href='http://" + unsubscribeUrl + "'>" + unsubscribeUrl + "</a>.</p>";
				var bodyClose = rawBody.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);

				if (bodyClose < 0)
					return rawBody + footer;
				else
					return rawBody.Insert(bodyClose, footer);
			} else {
				return rawBody + "\r\n\r\nTo unsubscribe, go to " + unsubscribeUrl;
			}
		}
		#endregion

		#region Events
		///<summary>Occurs when the MailSender is about to send a message.</summary>
		public event EventHandler<SendMailEventArgs> MessageSending;
		///<summary>Raises the MessageSending event.</summary>
		///<param name="e">A SendMailEventArgs object that provides the event data.</param>
		internal protected virtual void OnMessageSending(SendMailEventArgs e) {
			if (MessageSending != null)
				MessageSending(this, e);
		}
		///<summary>Occurs when the MailSender has successfully sent a message.</summary>
		public event EventHandler<SendMailEventArgs> MessageSent;
		///<summary>Raises the MessageSent event.</summary>
		///<param name="e">A SendMailEventArgs object that provides the event data.</param>
		internal protected virtual void OnMessageSent(SendMailEventArgs e) {
			if (MessageSent != null)
				MessageSent(this, e);
		}
		///<summary>Occurs when an error occurs while sending a message.</summary>
		public event EventHandler<SendMailErrorEventArgs> SendFailed;
		///<summary>Raises the SendFailed event.</summary>
		///<param name="e">A SendMailErrorEventArgs object that provides the event data.</param>
		internal protected virtual void OnSendFailed(SendMailErrorEventArgs e) {
			if (SendFailed != null)
				SendFailed(this, e);
		}
		#endregion
	}
}
