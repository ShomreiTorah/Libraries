using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Data;

namespace ShomreiTorah.Common.Calendar.Zmanim {
	///<summary>Reads Zmanim from a specialized CSV file.</summary>
	public class FastCsvZmanimProvider : YearlyZmanimProvider {
		const string Extension = "csv";
		const char Separator = ',';

		[ThreadStatic]
		static FastCsvZmanimProvider defaultInstance;
		///<summary>Gets the default FastCsvZmanimProvider instance, reading the folder path from ShomreiTorahConfig.xml.</summary>
		///<remarks>Since FastCsvZmanimProvider is not thread-safe, this will create a different instance for each thread.</remarks>
		public new static FastCsvZmanimProvider Default {
			get {
				if (defaultInstance == null) defaultInstance = new FastCsvZmanimProvider(Config.ReadAttribute("Zmanim", "FastCsv", "Path"));
				return defaultInstance;
			}
		}


		///<summary>Creates a FastCsvZmanimProvider that reads the given folder.</summary>
		///<param name="dataFolder">The path to a folder containing CSV files named by year.  (eg, 2009.csv, 2010.csv, etc...)</param>
		public FastCsvZmanimProvider(string dataFolder) {
			if (!Directory.Exists(dataFolder)) throw new DirectoryNotFoundException(dataFolder + " does not exist");
			DataFolder = dataFolder;
		}

		///<summary>Gets the directory that contains the data files.</summary>
		public string DataFolder { get; private set; }

		DateTime minDate, maxDate;

		///<summary>Gets the first date that this instance can provide Zmanim for.</summary>
		public override DateTime MinDate { get { if (minDate == DateTime.MinValue)FindLimits(); return minDate; } }
		///<summary>Gets the last date that this instance can provide Zmanim for.</summary>
		public override DateTime MaxDate { get { if (maxDate == DateTime.MinValue)FindLimits(); return maxDate; } }

		void FindLimits() {
			var files = Directory.GetFiles(DataFolder, "*." + Extension);
			int minYear = int.MaxValue, maxYear = int.MinValue;
			foreach (var path in files) {
				int year;
				if (!int.TryParse(Path.GetFileNameWithoutExtension(path), out year))
					continue;
				if (year < minYear) minYear = year;
				if (year > maxYear) maxYear = year;
			}
			if (minYear == int.MaxValue) {
				minDate = maxDate = DateTime.MinValue;
			} else {
				minDate = new DateTime(minYear, 1, 1);
				maxDate = new DateTime(maxYear, 12, 31);
			}
		}

		///<summary>Creates a YearlyZmanimDictionary for the given year.</summary>
		///<returns>The Zmanim for that year, or null if they could not be loaded.</returns>
		protected override YearlyZmanimDictionary CreateYear(int year) {
			var path = Path.Combine(DataFolder, year.ToString(CultureInfo.InvariantCulture) + "." + Extension);
			if (!File.Exists(path)) return null;
			var values = new ZmanimInfo[DateTime.IsLeapYear(year) ? 366 : 365];
			var dayOfYear = 0;
			string[] columnNames = null;
			bool hasDateColumn = false;
			string line;
			using (var reader = File.OpenText(path)) {
				while (null != (line = reader.ReadLine())) {
					if (line.Length == 0) continue;
					if (line[0] == '#') continue;

					if (columnNames == null) {
						columnNames = line.Split(Separator);
						if (columnNames[0].StartsWith("Date", StringComparison.OrdinalIgnoreCase)) {
							var oldCols = columnNames;
							columnNames = new string[oldCols.Length - 1];
							Array.Copy(oldCols, 1, columnNames, 0, oldCols.Length - 1);
							hasDateColumn = true;
						}

						for (int i = 0; i < columnNames.Length; i++) {
							columnNames[i] = columnNames[i].Trim();
						}
					} else {
						int index = hasDateColumn ? line.IndexOf(Separator) + 1 : 0;
						if (line.Length != index + 9 * columnNames.Length - 1)	//The last column doesn't have a trailing comma
							throw new DataException("Bad line:\r\n" + line + "\r\n in file " + path);
						var times = new Dictionary<string, TimeSpan>(columnNames.Length);
						for (int i = 0; i < columnNames.Length; i++) {
							try {
								times.Add(columnNames[i], new TimeSpan(
									int.Parse(line.Substring(index + 0, 2), CultureInfo.InvariantCulture),
									int.Parse(line.Substring(index + 3, 2), CultureInfo.InvariantCulture),
									int.Parse(line.Substring(index + 6, 2), CultureInfo.InvariantCulture))
								);
							} catch (Exception ex) {
								throw new DataException("Bad line:\r\n" + line + "\r\n in file " + path, ex);
							}
							if (i < columnNames.Length - 1 && line[index + 8] != ',')	//The last column doesn't have a trailing comma
								throw new DataException("Bad line:\r\n" + line + "\r\n in file " + path);
							index += 9;
						}
						values[dayOfYear] = new ZmanimInfo(new DateTime(year, 1, 1).AddDays(dayOfYear), times);
						dayOfYear++;
					}
				}
			}
			return new YearlyZmanimDictionary(year, values);
		}
		#region CSV Header
		///<summary>The comment written to the beginning of each file.</summary>
		const string CsvHeader = @"#Shomrei Torah Optimized Zmanim File

#This file is a fixed-width comma-delimited file
#containing Zmanim info for the year specified in
#the filename.  These files are read by 
#ShomreiTorah.Common.Calendar.Zmanim.FastCsvZmanimProvider
#in ShomreiTorah.Common.dll

#The format is as follows; whitespace is not ignored.
#Blank lines and lines beginning with the octothorpe
#character (#) are ignored.

#The first significant line is assumed to contain
#column headers, separated by commas.  If the first
#column header begins with the word Date (case insensitive),
#the entire first column is ignored.

#Subsequent lines are assumed to contain comma-separated
#Zmanim, in order, for every date in the year.  (this is
#not verified).  If there was a date column, it is ignored,
#and each line is parsed from the first comma.  Each field
#must be in the exact format 'HH:MM:ss,' (9 characters 
#long), except that the last field must not have a comma.

#The date column is supported only for interoperability and
#is not used at all";
		#endregion
		///<summary>Writes Zmanim for a range of years from a ZmanimProvider to this instance's directory.</summary>
		///<param name="provider">The ZmanimProvider to read Zmanim from.</param>
		///<param name="fromYear">The first year to write.</param>
		///<param name="toYear">The last year to write.</param>
		///<param name="writeDateColumn">Whether to write a column with the date of each row (to make the output more readable).</param>
		public void Write(ZmanimProvider provider, int fromYear, int toYear, bool writeDateColumn) {
			if (provider == null) throw new ArgumentNullException("provider", "provider is null.");
			if (fromYear > toYear) { var t = fromYear; fromYear = toYear; toYear = t; }
			for (int year = fromYear; year <= toYear; year++) {
				var zmanim = provider.GetZmanim(new DateTime(year, 1, 1), new DateTime(year, 12, 31)).ToArray();

				if (zmanim.Any(z => z == null)) throw new ArgumentException(provider.ToString() + " cannot provide zmanim for " + year, "provider");
				WriteYear(zmanim, writeDateColumn);
			}
		}
		///<summary>Writes the given Zmanim to this instance's directory.</summary>
		///<param name="zmanim">The Zmanim to write.  This must be a contiguous range of years.</param>
		///<param name="writeDateColumn">Whether to write a column with the date of each row (to make the output more readable).</param>
		public void Write(IEnumerable<ZmanimInfo> zmanim, bool writeDateColumn) {
			var groupedYears = from zman in zmanim
							   orderby zman.Date
							   group zman by zman.Date.Year into g
							   select new { Year = g.Key, Zmanim = g.ToArray() };

			foreach (var year in groupedYears) {
				if (year.Zmanim.Length != (DateTime.IsLeapYear(year.Year) ? 366 : 365))
					throw new ArgumentException("Incomplete year " + year.Year + "\r\nMissing "
						+ String.Join(", ", Enumerable.Range(0, DateTime.IsLeapYear(year.Year) ? 366 : 365)
							.Select(i => new DateTime(year.Year, 1, 1).AddDays(i))
							.Except(year.Zmanim.Select(z => z.Date))
							.Select(d => d.ToShortDateString())
							.ToArray())
						, "zmanim");
				WriteYear(year.Zmanim, writeDateColumn);
			}
		}
		void WriteYear(ZmanimInfo[] year, bool writeDateColumn) {
			var columns = year[0].Times.Keys.ToArray();
			using (var writer = File.CreateText(Path.Combine(DataFolder, year[0].Date.Year.ToString(CultureInfo.InvariantCulture) + "." + Extension))) {
				writer.WriteLine(CsvHeader);
				if (writeDateColumn)
					writer.Write("Date,");

				for (int i = 0; i < columns.Length; i++) {
					if (i > 0) writer.Write(',');
					writer.Write(columns[i].Replace(',', '_'));
				}
				writer.WriteLine();
				foreach (var day in year) {
					if (writeDateColumn)
						writer.Write("{0:MM/dd/yyyy},", day.Date);

					for (int i = 0; i < columns.Length; i++) {
						if (i > 0) writer.Write(',');
						var time = day[columns[i]];
						writer.Write("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
					}
					writer.WriteLine();
				}
			}
		}
	}
}
