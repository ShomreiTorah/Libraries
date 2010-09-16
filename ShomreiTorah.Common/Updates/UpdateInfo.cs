using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ShomreiTorah.Common.Updates {
	///<summary>Describes an update for a product.</summary>
	public class UpdateInfo {
		internal UpdateInfo(XElement element) {
			ProductName = element.Attribute("Name").Value;

			Versions = element.Element("Versions").Elements("Version")
							.Select(UpdateVersion.FromXml).OrderByDescending(v => v.Version).ReadOnlyCopy();

			Files = element.Element("Files").Elements().Select(UpdateFile.FromXml).ReadOnlyCopy();
		}

		///<summary>Gets the name of the product that the update applies to.</summary>
		public string ProductName { get; private set; }

		///<summary>Gets the version supplied by the update.</summary>
		public Version NewVersion { get { return Versions.First().Version; } }
		///<summary>Gets the date the the update was published.</summary>
		public DateTime PublishDate { get { return Versions.First().PublishDate; } }

		///<summary>Gets all of the changes supplied by the update since the current version.</summary>
		public string GetChanges(Version existingVersion) {
			return Versions.TakeWhile(v => v.Version > existingVersion).Join(Environment.NewLine, v => v.Changes.TrimEnd());
		}

		///<summary>Gets the files in the update.</summary>
		public ReadOnlyCollection<UpdateFile> Files { get; private set; }
		///<summary>Gets the versions contained and subsumed by the update.</summary>
		public ReadOnlyCollection<UpdateVersion> Versions { get; private set; }

		///<summary>Downloads the update and extracts its files to a temporary directory.</summary>
		///<param name="existingFiles">The path to the existing files that should be updated. 
		/// Any files in this directory that match files in the update will not be re-downloaded.</param>
		///<param name="ui">An optional IProgressReporter implementation to report the progress of the download.</param>
		///<returns>The path to the extracted files.</returns>
		public string DownloadFiles(string existingFiles, IProgressReporter ui) {
			if (!Directory.Exists(existingFiles)) throw new DirectoryNotFoundException(existingFiles + " does not exist");
			ui = ui ?? new EmptyProgressReporter();
			ui.CanCancel = true;

			var path = Path.GetTempFileName();
			File.Delete(path);
			Directory.CreateDirectory(path);

			try {
				var newFiles = Files.Where(f => !f.Matches(existingFiles)).ToArray();

				ui.Maximum = newFiles.Sum(f => f.Length);

				foreach (var file in newFiles) {
					ui.Caption = "Downloading " + file.RelativePath;
					file.DownloadFile(path, ui.ChildOperation());

					if (ui.WasCanceled) {
						Directory.Delete(path, true);
						return null;
					}
				}
			} catch (Exception ex) {
				Directory.Delete(path, true);

				if (ui.WasCanceled) return null;	//If it was canceled, we'll get a CryptoException because the CryptoStream was closed 
				throw new UpdateErrorException(ex);
			}
			return path;
		}
	}

	///<summary>Describes the changes in a single version.</summary>
	public class UpdateVersion {
		///<summary>Creates an UpdateVersion object.</summary>
		public UpdateVersion(DateTime publishDate, Version version, string changes) {
			PublishDate = publishDate;
			Version = version;
			Changes = changes;
		}
		///<summary>Parses an UpdateVersion object from an XML element.</summary>
		public static UpdateVersion FromXml(XElement element) {
			return new UpdateVersion(
				DateTime.Parse(element.Attribute("PublishDate").Value, CultureInfo.InvariantCulture),
				new Version(element.Attribute("Version").Value),
				changes: element.Value
			);
		}
		///<summary>Saves this UpdateVersion to an XML element.</summary>
		public XElement ToXml() {
			return new XElement("Version",
				new XAttribute("PublishDate", PublishDate.ToString("F", CultureInfo.InvariantCulture)),
				new XAttribute("Version", Version.ToString()),
				new XText(Changes)
			);
		}

		///<summary>Gets the date that the version was published.</summary>
		public DateTime PublishDate { get; private set; }
		///<summary>Gets the version number.</summary>
		public Version Version { get; private set; }
		///<summary>Gets the changes made in the version.</summary>
		public string Changes { get; private set; }
	}
}
