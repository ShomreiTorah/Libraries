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
		#region Read ShomreiTorahConfig
		static SymmetricAlgorithm CreateKD() {
			var retVal = SymmetricAlgorithm.Create(Config.ReadAttribute("Updates", "Cryptography", "FileDecryptor", "Algorithm"));

			var key = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "FileDecryptor", "Key").Value);
			var iv = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "FileDecryptor", "IV").Value);

			retVal.KeySize = key.Length * 8;
			retVal.BlockSize = iv.Length * 8;

			retVal.Key = key;
			retVal.IV = iv;
			return retVal;
		}

		///<summary>The SymmetricAlgorithm used to encrypt update files.</summary>
		static readonly SymmetricAlgorithm FileAlgorithm = CreateKD();

		///<summary>Creates an ICryptoTransform that encrypts the update files.</summary>
		public static ICryptoTransform CreateFileEncryptor() { return FileAlgorithm.CreateEncryptor(); }
		///<summary>Creates an ICryptoTransform that decrypts the update files.</summary>
		public static ICryptoTransform CreateFileDecryptor() { return FileAlgorithm.CreateDecryptor(); }

		static RSACryptoServiceProvider CreateVerifier() {
			var retVal = new RSACryptoServiceProvider();
			retVal.FromXmlString(Config.GetElement("Updates", "Cryptography", "UpdateVerifier").ToString());
			return retVal;
		}
		internal static readonly RSACryptoServiceProvider UpdateVerifier = CreateVerifier();

		///<summary>Gets the Uri that contains updates.</summary>
		public static readonly Uri BaseUri = new Uri(Config.ReadAttribute("Updates", "BaseUri"), UriKind.Absolute);
		#endregion

		///<summary>Creates an UpdateChecker from an UpdatableAttribute defined in the calling assembly.</summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public UpdateChecker() : this(Assembly.GetCallingAssembly().GetCustomAttribute<UpdatableAttribute>()) { }
		///<summary>Creates a new UpdateChecker for the product described by an UpdatableAttribute instance.</summary>
		public UpdateChecker(UpdatableAttribute attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			ProductName = attribute.ProductName;
			CurrentVersion = attribute.CurrentVersion;
		}

		///<summary>Creates a new UpdateChecker for a given product.</summary>
		///<param name="productName">The name of the product to update.</param>
		///<param name="currentVersion">The version of the product that is currently installed.</param>
		public UpdateChecker(string productName, Version currentVersion) { ProductName = productName; CurrentVersion = currentVersion; }

		///<summary>Gets the name of the product to update.</summary>
		public string ProductName { get; private set; }
		///<summary>Gets the currently installed version.</summary>
		public Version CurrentVersion { get; private set; }

		///<summary>Gets information about an available update.</summary>
		///<returns>An UpdateInfo class describing the update, or null if there is no available update.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Hide errors")]
		public UpdateInfo FindUpdate() {
			try {
				var document = XDocument.Load(new Uri(BaseUri, new Uri(ProductName + "/Manifest.xml", UriKind.Relative)).ToString());
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
