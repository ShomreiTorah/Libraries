using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace ShomreiTorah.Common.Updates {
	///<summary>Describes a single (non-downloaded) file in an update.</summary>
	public class UpdateFile {
		static readonly string signatureAlgorithm = CryptoConfig.MapNameToOID("SHA512");

		//HashAlgorithm uses shared state and cannot be re-used
		static readonly Func<HashAlgorithm> hasherCreator = () => new SHA512Managed();

		byte[] hash, signature;

		#region Creation
		private UpdateFile() { }

		///<summary>Parses an UpdateFile object from an XML element.</summary>
		///<param name="element">An XML element from an update manifest.</param>
		public static UpdateFile FromXml(XElement element) {
			if (element == null) throw new ArgumentNullException("element");

			var retVal = new UpdateFile {
				RelativePath = element.Attribute("RelativePath").Value,
				Length = long.Parse(element.Attribute("Size").Value, CultureInfo.InvariantCulture),
				DateModifiedUtc = DateTime.ParseExact(element.Attribute("Timestamp").Value, "o", CultureInfo.InvariantCulture).ToUniversalTime(),
				RemoteUrl = new Uri(UpdateChecker.BaseUri, new Uri(element.Attribute("Url").Value, UriKind.Relative)),

				hash = Convert.FromBase64String(element.Element("Hash").Value),
				signature = Convert.FromBase64String(element.Element("Signature").Value)
			};
			if (!UpdateChecker.UpdateVerifier.VerifyHash(retVal.hash, signatureAlgorithm, retVal.signature))
				throw new InvalidDataException("Bad signature for " + retVal.RelativePath);
			return retVal;
		}

		///<summary>Creates an UpdateFile object describing an existing file on disk.</summary>
		///<param name="basePath">The path to the base directory containing the source files.</param>
		///<param name="relativePath">The relative path to the source file to describe.</param>
		///<param name="remotePath">The relative path on the server where the file will be uploaded.</param>
		///<param name="signer">An RSA instance containing the private key to sign the file.</param>
		///<remarks>This method is called by the update publisher.</remarks>
		public static UpdateFile Create(string basePath, string relativePath, Uri remotePath, RSACryptoServiceProvider signer) {
			var filePath = Path.Combine(basePath, relativePath);
			if (remotePath == null) throw new ArgumentNullException("remotePath");
			if (signer == null) throw new ArgumentNullException("signer");

			var info = new FileInfo(filePath);

			if (!remotePath.IsAbsoluteUri)
				remotePath = new Uri(UpdateChecker.BaseUri, remotePath);
			var retVal = new UpdateFile {
				RelativePath = relativePath,
				RemoteUrl = remotePath,
				Length = info.Length,
				DateModifiedUtc = info.LastWriteTimeUtc
			};
			using (var hasher = hasherCreator())
			using (var stream = File.OpenRead(filePath))
				retVal.hash = hasher.ComputeHash(stream);
			retVal.signature = signer.SignHash(retVal.hash, signatureAlgorithm);

			return retVal;
		}

		///<summary>Writes this UpdateFile to an XML element for an update manifest.</summary>
		public XElement ToXml() {
			return new XElement("File",
				new XAttribute("RelativePath", RelativePath),
				new XAttribute("Url", UpdateChecker.BaseUri.MakeRelativeUri(RemoteUrl)),
				new XAttribute("Size", Length.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("Timestamp", DateModifiedUtc.ToString("o", CultureInfo.InvariantCulture)),

				new XElement("Hash", Convert.ToBase64String(hash)),
				new XElement("Signature", Convert.ToBase64String(signature))
			);
		}
		#endregion

		///<summary>Gets the relative path to the file.</summary>
		public string RelativePath { get; private set; }
		///<summary>Gets the URL to the encrypted file on the server, relative to the base Updates directory.</summary>
		public Uri RemoteUrl { get; private set; }
		///<summary>Gets the size of the file in bytes.</summary>
		public long Length { get; private set; }
		///<summary>Gets the file's timestamp in UTC.</summary>
		public DateTime DateModifiedUtc { get; private set; }

		///<summary>Checks whether an existing file is identical to this update file.</summary>
		///<param name="basePath">The base directory expected to match the update.</param>
		///<returns>True if the file exists in the base directory and its hash, timestamp, and size are identical.</returns>
		public bool Matches(string basePath) {
			if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException(basePath + " does not exist");
			var filePath = Path.Combine(basePath, RelativePath);

			var info = new FileInfo(filePath);
			if (!info.Exists) return false;
			if (info.Length != Length) return false;
			if (!AreClose(info.LastWriteTimeUtc, DateModifiedUtc)) return false;

			using (var hasher = hasherCreator())
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				if (!hash.SequenceEqual(hasher.ComputeHash(stream)))
					return false;

			return true;
		}

		static bool AreClose(DateTime a, DateTime b) {
			//TODO: Allow for DST issues?
			return Math.Abs((a - b).TotalSeconds) <= 2;
		}

		///<summary>Downloads the file from the server.</summary>
		///<param name="basePath">The directory to download into.</param>
		///<param name="ui">An optional IProgressReporter to report progress.</param>
		///<exception cref="InvalidDataException">The hash didn't match.</exception>
		public void DownloadFile(string basePath, IProgressReporter ui) {
			if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException(basePath + " does not exist");
			var filePath = Path.Combine(basePath, RelativePath);
			if (File.Exists(filePath)) throw new InvalidOperationException(filePath + " already exists");

			Directory.CreateDirectory(Path.GetDirectoryName(filePath));	//Won't throw

			ui = ui ?? new EmptyProgressReporter();

			long actualSize;

			var request = WebRequest.Create(RemoteUrl);

			using (var hasher = hasherCreator()) {
				using (var transform = UpdateChecker.CreateFileDecryptor())

				using (var response = request.GetResponse())
				using (var cypherStream = response.GetResponseStream())
				using (var decryptingStream = new CryptoStream(cypherStream, transform, CryptoStreamMode.Read))
				using (var unzipper = new GZipStream(decryptingStream, CompressionMode.Decompress))
				using (var hashingStream = new CryptoStream(unzipper, hasher, CryptoStreamMode.Read))

				using (var fileStream = File.Create(filePath)) {
					actualSize = hashingStream.CopyTo(fileStream, Length, ui);
				}

				if (Length != actualSize) {
					File.Delete(filePath);
					throw new InvalidDataException("Bad length");
				}
				if (!hasher.Hash.SequenceEqual(hash)) {
					File.Delete(filePath);
					throw new InvalidDataException("Bad hash");
				}
				File.SetLastWriteTimeUtc(filePath, DateModifiedUtc);
			}
		}
	}
}