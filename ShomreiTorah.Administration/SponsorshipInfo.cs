using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using ShomreiTorah.Common;

namespace ShomreiTorah.Administration
{
	///<summary>Contains information about a sponsorship.</summary>
	public class SponsorshipInfo {
		///<summary>Gets or sets the full name of the sponsor.</summary>
		public string FullName { get; set; }
		///<summary>Gets or sets the sponsorship message.</summary>
		public string Message { get; set; }

		///<summary>Gets the נר למאור sponsors for the given month, if any.</summary>
		public static ReadOnlyCollection<SponsorshipInfo> Forנרלמאור(DBConnector db, DateTime date) {
			using (var connection = db.OpenConnection())
				return Forנרלמאור(connection, date);
		}
		///<summary>Gets the נר למאור sponsors for the given month, if any.</summary>
		public static ReadOnlyCollection<SponsorshipInfo> Forנרלמאור(DbConnection connection, DateTime date) {
			return connection.ExecuteReader(@"
SELECT FullName, Note
FROM Billing.Pledges p JOIN Data.MasterDirectory md ON p.PersonId = md.Id 
WHERE Type = N'נר למאור' AND YEAR(Date) = Year(@date) AND MONTH(Date) = MONTH(@date)
ORDER BY LastName", new { date })
					.Cast<DbDataRecord>()
					.Select(dr => new SponsorshipInfo { FullName = dr.GetString(0), Message = dr.GetString(1) })
					.ReadOnlyCopy();
		}
	}
}
