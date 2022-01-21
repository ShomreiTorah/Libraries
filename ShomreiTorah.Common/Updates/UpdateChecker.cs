using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ShomreiTorah.Common.Updates {
	///<summary>Checks for updates.</summary>
	public class UpdateChecker {
		///<summary>Creates an ICryptoTransform that encrypts the update files.</summary>
		public static ICryptoTransform CreateFileEncryptor() { return UpdateConfig.Standard.FileAlgorithm.CreateEncryptor(); }
		///<summary>Creates an ICryptoTransform that decrypts the update files.</summary>
		public static ICryptoTransform CreateFileDecryptor() { return UpdateConfig.Standard.FileAlgorithm.CreateDecryptor(); }

		///<summary>Creates an UpdateChecker from an UpdatableAttribute defined in the calling assembly.</summary>
		///<returns>An UpdateChecker instance that for this program, or null if updates are not configured in ShomreiTorahConfig.xml.</returns>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static UpdateChecker CreateForCaller() {
			return Create(Assembly.GetCallingAssembly().GetCustomAttribute<UpdatableAttribute>());
		}

		///<summary>Creates a new UpdateChecker for the product described by an UpdatableAttribute instance.</summary>
		///<returns>An UpdateChecker instance that for the program described by the attribute, or null if updates are not configured in ShomreiTorahConfig.xml.</returns>
		public static UpdateChecker Create(UpdatableAttribute attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");

			if (UpdateConfig.Standard == null)
				return null;

			return new UpdateChecker(attribute);
		}
		///<summary>Creates a new UpdateChecker for a given product.</summary>
		///<param name="productName">The name of the product to update.</param>
		///<param name="currentVersion">The version of the product that is currently installed.</param>
		///<returns>An UpdateChecker instance that for the program described by the attribute, or null if updates are not configured in ShomreiTorahConfig.xml.</returns>
		public static UpdateChecker Create(string productName, Version currentVersion) {
			if (UpdateConfig.Standard == null)
				return null;

			return new UpdateChecker(productName, currentVersion);
		}

		private UpdateChecker(UpdatableAttribute attribute) {
			ProductName = attribute.ProductName;
			CurrentVersion = attribute.CurrentVersion;
		}

		private UpdateChecker(string productName, Version currentVersion) {
			ProductName = productName;
			CurrentVersion = currentVersion;
		}

		///<summary>Gets the name of the product to update.</summary>
		public string ProductName { get; private set; }
		///<summary>Gets the currently installed version.</summary>
		public Version CurrentVersion { get; private set; }

		///<summary>Gets information about an available update.</summary>
		///<returns>An UpdateInfo class describing the update, or null if there is no available update.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Hide errors")]
		public UpdateInfo FindUpdate() {
			try {
				var document = XDocument.Load(new Uri(UpdateConfig.Standard.BaseUri, new Uri(ProductName + "/Manifest.xml", UriKind.Relative)).ToString());
				if (document == null) return null;
				var newUpdate = new UpdateInfo(document.Root);
				return newUpdate.NewVersion > CurrentVersion ? newUpdate : null;
			} catch { return null; }
		}

		///<summary>Applies a downloaded update.  After this method returns, the calling program must exit.</summary>
		///<param name="updatePath">The path to the temporary folder containing the downloaded update (returned by <see cref="UpdateInfo.DownloadFiles"/>)</param>
		///<param name="appPath">The directory to apply the update to (the folder that contains the program being updated).</param>
		public static void ApplyUpdate(string updatePath, string appPath) {
			if (updatePath == null) throw new ArgumentNullException("updatePath");
			if (appPath == null) throw new ArgumentNullException("appPath");

			if (!Directory.Exists(updatePath)) throw new ArgumentException(updatePath + " does not exist", "updatePath");
			if (!Directory.Exists(appPath)) throw new ArgumentException(appPath + " does not exist", "appPath");

			var updaterExePath = MakeUpdaterExePath();

			using (var updaterExeStream = typeof(UpdateChecker).Assembly.GetManifestResourceStream(typeof(UpdateChecker), "UpdateApplier.exe"))
			using (var fileStream = File.Create(updaterExePath)) {
				updaterExeStream.CopyTo(fileStream);
			}

			string args;
			using (var me = Process.GetCurrentProcess()) args = me.Id.ToString(CultureInfo.InvariantCulture);
			args += " " + EscapeCommandArg(updatePath) + " " + EscapeCommandArg(appPath);

			using (var updateProcess = Process.Start(new ProcessStartInfo {
				FileName = updaterExePath,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true
			})) {
				updateProcess.StandardOutput.ReadLine();	//Wait until the child process is ready, to ensure that it can get our command line before we exit.
			}
		}
		static string EscapeCommandArg(string arg) { return "\"" + arg.Replace("\"", "\"\"") + "\""; }
		static string MakeUpdaterExePath() {
			var tempDir = Path.GetTempPath();
			var updaterExePath = Path.Combine(tempDir, "UpdateApplier.exe");
			int fileNameIndex = 0;
			while (File.Exists(updaterExePath)) {
				try {
					File.Delete(updaterExePath);
				} catch (IOException) {
					fileNameIndex++;
					updaterExePath = Path.Combine(tempDir, "UpdateApplier (" + fileNameIndex + ".exe");
				}
			}
			return updaterExePath;
		}
	}
	///<summary>Stores the update configuration read from ShomreiTorahConfig.xml.</summary>
	public class UpdateConfig {
		///<summary>Gets the update configuration info from ShomreiTorahConfig, or null if ShomreiTorahConfig.xml doesn't have an Updates element.</summary>
		public static UpdateConfig Standard { get; } = Config.Xml.Root.Element("Updates") == null ? null : new UpdateConfig();


		///<summary>Reads settings from ShomreiTorahConfig into a new UpdateConfig instance.</summary>
		private UpdateConfig() {
			FileAlgorithm = SymmetricAlgorithm.Create(Config.ReadAttribute("Updates", "Cryptography", "FileDecryptor", "Algorithm"));

			var key = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "FileDecryptor", "Key").Value);
			var iv = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "FileDecryptor", "IV").Value);

			FileAlgorithm.KeySize = key.Length * 8;
			FileAlgorithm.BlockSize = iv.Length * 8;

			FileAlgorithm.Key = key;
			FileAlgorithm.IV = iv;

			UpdateVerifier = new RSACryptoServiceProvider();
			UpdateVerifier.FromXmlString(Config.GetElement("Updates", "Cryptography", "UpdateVerifier").ToString());


			Uri domain = new Uri("https://" + Config.DomainName, UriKind.Absolute);
			RemotePath = new Uri(Config.ReadAttribute("Updates", "Path"), UriKind.RelativeOrAbsolute);
			BaseUri = new Uri(domain, RemotePath);
		}

		///<summary>Gets the SymmetricAlgorithm used to encrypt individual update files.</summary>
		public SymmetricAlgorithm FileAlgorithm { get; private set; }
		///<summary>Holds the RSA public key used to verify file hashes.</summary>
		public RSACryptoServiceProvider UpdateVerifier { get; private set; }

		///<summary>Gets the absolute path on the file server to upload and download updates.</summary>
		public Uri RemotePath { get; private set; }

		///<summary>Gets the absolute URL (including domain name) to the website directory that contains updates.</summary>
		public Uri BaseUri { get; private set; }
	}

	///<summary>An exception thrown when an error occurs during the update process.</summary>
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Requires InnerException")]
	public class UpdateErrorException : Exception {
		///<summary>Creates an UpdateErrorException.</summary>
		public UpdateErrorException(Exception inner) : base("An error occurred while downloading an update", inner) { }
		///<summary>Serialization constructor</summary>
		protected UpdateErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	///<summary>Marks the main assembly of an updatable product.</summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Version type")]
	public sealed class UpdatableAttribute : Attribute {
		///<summary>Creates an UpdatableAttribute instance.</summary>
		///<param name="productName">The name of the product.</param>
		///<param name="currentVersion">The current version of the product.</param>
		public UpdatableAttribute(string productName, string currentVersion) { ProductName = productName; CurrentVersion = new Version(currentVersion); }

		///<summary>Gets the name of the product to update.</summary>
		public string ProductName { get; private set; }
		///<summary>Gets the currently installed version.</summary>
		public Version CurrentVersion { get; private set; }
	}
}
