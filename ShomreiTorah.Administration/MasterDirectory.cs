using System;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using ShomreiTorah.Common;

namespace ShomreiTorah.Administration {
	///<summary>Manipulates the Master Directory.</summary>
	public class MasterDirectory {
		static MasterDirectory defaultInstance = new MasterDirectory(DB.Default);

		///<summary>Gets or sets the MasterDirectory instance for the production database.</summary>
		public static MasterDirectory Default { get { return defaultInstance; } set { defaultInstance = value; } }

		///<summary>Creates a new MasterDirectory instance.</summary>
		public MasterDirectory(DBConnector database) {
			if (database == null) throw new ArgumentNullException("database");
			Database = database;
		}

		static readonly Regex phoneParser = new Regex(@"^\(?(\d{3})\)?\s*-?\s*(\d{3})\s*-?\s*(\d{4})$", RegexOptions.Compiled);
		///<summary>Formats a string as a phone number.</summary>
		public static string FormatPhoneNumber(string phone) {
			if (phone == null) return null;
			var match = phoneParser.Match(phone);

			if (!match.Success)
				return phone;
			return match.Result("($1) $2 - $3");
		}


		///<summary>Gets the database that this instance connects to.</summary>
		public DBConnector Database { get; private set; }

		///<summary>Resolves contact information to a PersonID for the web site.</summary>
		///<returns>A PersonID Guid for a new or existing person.</returns>
		public Guid Resolve(string fullName, string phone) {
			if (phone == null) throw new ArgumentNullException("phone");
			if (fullName == null) throw new ArgumentNullException("fullName");

			fullName = fullName.Trim();
			phone = FormatPhoneNumber(phone);

			var phoneMatches = Database
				.ExecuteReader(@"SELECT Id, FullName FROM Data.MasterDirectory WHERE Phone = Data.FormatPhone(@phone)", new { phone })
				.Cast<IDataRecord>()
				.Select(dr => new { Id = dr.GetGuid(0), FullName = dr.GetString(1) })
				.ToArray();

			if (phoneMatches.Length == 1)
				return phoneMatches[0].Id;

			var fullNameMatches = phoneMatches.Where(m => IsFuzzyMatch(m.FullName, fullName));
			if (fullNameMatches.Count() == 1)
				return fullNameMatches.First().Id;

			var lastNameStart = fullName.LastIndexOf(' ');
			string lastName = lastNameStart > 0 ? fullName.Substring(lastNameStart + 1) : fullName;

			var guid = Guid.NewGuid();
			Database.ExecuteNonQuery(@"
INSERT INTO Data.MasterDirectory(Id, FullName, LastName, Phone, Source)
VALUES(@guid, @fullName, @lastName, Data.FormatPhone(@phone), 'Web Site')", new { guid, fullName, lastName, phone });
			return guid;
		}
		static bool IsFuzzyMatch(string a, string b) {
			return a.Replace(".", "").Replace(",", "").Replace(" ", "").Trim().Equals(
				   b.Replace(".", "").Replace(",", "").Replace(" ", "").Trim(), StringComparison.CurrentCultureIgnoreCase
			);
		}

		///<summary>Sends an email to the email addresses registered for a person, if any.</summary>
		///<returns>True if an email was sent; false if the person has no associated email addresses.</returns>
		public bool Notify(MailAddress from, Guid personId, string subject, string body) {
			var emails = Database.ExecuteReader(@"SELECT [Name], Email FROM tblMLMembers WHERE PersonId = @personId", new { personId })
								 .Cast<IDataRecord>()
								 .Select(r => new MailAddress(r.GetString(1), r.GetString(0)))
								 .ToList();
			if (emails.Count == 0) return false;

			var message = new MailMessage {
				BodyEncoding = Email.DefaultEncoding,
				SubjectEncoding = Email.DefaultEncoding,
				From = from,
				Subject = subject,
				Body = body
			};
			message.To.AddRange(emails);

			Email.Default.SendAsync(message);
			return true;
		}
	}
}