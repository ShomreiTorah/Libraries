using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>A base class for Zmanim providers</summary>
	public abstract class ZmanimProvider {
		///<summary>Gets the Zmanim for the given date.</summary>
		///<returns>A ZmanimInfo for targetDate, or null if this instance cannot provide zmanim for targetDate.</returns>
		public abstract ZmanimInfo GetZmanim(DateTime targetDate);
		///<summary>Gets Zmanim for all days between the given dates.</summary>
		///<param name="fromDate">The first date to return.</param>
		///<param name="toDate">The last date to return.</param>
		///<returns>An enumerable of ZmanimInfo objects for each date, and null for dates that this instance cannot provide.</returns>
		public virtual IEnumerable<ZmanimInfo> GetZmanim(DateTime fromDate, DateTime toDate) { return Enumerable.Range(0, (toDate - fromDate).Days + 1).Select(i => GetZmanim(fromDate.AddDays(i))); }

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public abstract DateTime MinDate { get; }
		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public abstract DateTime MaxDate { get; }

		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static readonly ZmanimProvider Instance = Get(Config.ReadAttribute("Zmanim", "DefaultProvider"));
		}
		///<summary>Gets the default ZmanimProvider with the given name.</summary>
		public static ZmanimProvider Get(string name) {
			switch (name) {
				case "Calculator": return CalculatingZmanimProvider.Default;
				case "Fixed": return FixedZmanimProvider.Default;
				case "FastCSV": return FastCsvZmanimProvider.Default;

				//TODO: Re-instate OUZmanimProvider once Common moves to .Net 4.0.
#if NET40
				case "OU": return OUZmanimProvider.Default;
#endif
				case "XML": return XmlZmanimProvider.Default;
				default:
					throw new ConfigurationException("ZmanimProvider " + name + " is not supported.\r\nSupported ZmanimProviders are Calculator, FastCSV, Fixed, XML, and OU.\r\nPlease check ShomreiTorahConfig.xml");
			}
		}

		//If the property is set before it is first read, ReadAttribute will never be called
		static ZmanimProvider defaultOverride;

		///<summary>Gets or sets the default ZmanimProvider implementation from ShomreiTorahConfig.xml.</summary>
		public static ZmanimProvider Default {
			get { return defaultOverride ?? DefaultContainer.Instance; }
			set { defaultOverride = value; }
		}
	}

	///<summary>Reads Zmanim from an XML database.</summary>
	public class XmlZmanimProvider : ZmanimProvider {
		///<summary>Gets the default XmlZmanimProvider, finding the XML file in ShomreiTorahConfig.xml.</summary>
		public new static XmlZmanimProvider Default { get { return DefaultInstance.Value; } }
		static class DefaultInstance {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultInstance() { }
			public static readonly XmlZmanimProvider Value = new XmlZmanimProvider(Config.ReadAttribute("Zmanim", "Xml", "Path"));
		}

		///<summary>Creates an XmlZmanimProvider from an XML file.</summary>
		public XmlZmanimProvider(string xmlPath) : this(Load(xmlPath).Tables[0]) { }
		///<summary>Creates an XmlZmanimProvider from a DataTable.</summary>
		public XmlZmanimProvider(DataTable table) {
			if (table == null) throw new ArgumentNullException("table");

			Table = table;
			DateColumn = table.PrimaryKey.FirstOrDefault(c => c.DataType == typeof(DateTime))
					  ?? Columns.First(c => c.DataType == typeof(DateTime));
		}

		static DataSet Load(string xmlPath) {
			var retVal = new DataSet { Locale = CultureInfo.InvariantCulture };
			retVal.ReadXml(xmlPath);
			return retVal;
		}

		///<summary>Gets the table containing the Zmanim.</summary>
		public DataTable Table { get; private set; }

		///<summary>Gets the table containing the Zmanim.</summary>
		public DataColumn DateColumn { get; private set; }
		///<summary>Gets the table containing the Zmanim.</summary>
		public IEnumerable<DataColumn> Columns { get { return Table.Columns.Cast<DataColumn>(); } }

		///<summary>Gets the Zmanim for the given date.</summary>
		public override ZmanimInfo GetZmanim(DateTime targetDate) {
			var row = Table.Rows.Find(targetDate);
			if (row == null) return null;
			return new ZmanimInfo(targetDate,
				Columns
					.Where(c => c.DataType == typeof(DateTime) && c != DateColumn)
					.ToDictionary(c => c.ColumnName, c => row.Field<DateTime>(c).TimeOfDay)
				);
		}

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public override DateTime MinDate { get { return (DateTime)Table.Compute("Min(" + DateColumn.ColumnName + ")", null); } }
		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public override DateTime MaxDate { get { return (DateTime)Table.Compute("Max(" + DateColumn.ColumnName + ")", null); } }
	}
}