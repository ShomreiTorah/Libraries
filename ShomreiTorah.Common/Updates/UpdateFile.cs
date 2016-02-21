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
	///<summary>Describes a single (non-downloaded) file in an update.  This class is immutable.</summary>
	public class UpdateFile {
		static readonly string signatureAlgorithm = CryptoConfig.MapNameToOID("SHA512");

		//HashAlgorithm uses shared state and cannot be re-used
		static readonly Func<HashAlgorithm> hasherCreator = () => new SHA512Managed();

		byte[] hash;
		IReadOnlyCollection<byte[]> signatures;

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
				RemoteUrl = new Uri(element.Attribute("Url").Value, UriKind.Relative),

				hash = Convert.FromBase64String(element.Element("Hash").Value),
				signatures = element.Elements("Signature").Select(e => Convert.FromBase64String(e.Value)).ToList().AsReadOnly()
			};
			if (!retVal.signatures.Any(s => UpdateConfig.Standard.UpdateVerifier.VerifyHash(retVal.hash, signatureAlgorithm, s)))
				throw new InvalidDataException("Bad signature for " + retVal.RelativePath);
			return retVal;
		}

		///<summary>Creates an UpdateFile object describing an existing file on disk.</summary>
		///<param name="basePath">The path to the base local directory containing the source files.</param>
		///<param name="relativePath">The relative path to the source file on disk to describe.</param>
		///<param name="remotePath">The path on the FTP server where the file will be uploaded, relative to the base Updates directory.</param>
		///<param name="signer">A collection of RSA instances containing the private key(s) to sign the file.</param>
		///<remarks>This method is called by the update publisher.</remarks>
		public static UpdateFile Create(string basePath, string relativePath, Uri remotePath, IEnumerable<RSACryptoServiceProvider> signers) {
			if (remotePath == null) throw new ArgumentNullException("remotePath");
			if (signers == null || !signers.Any()) throw new ArgumentNullException("signer");
			var filePath = Path.Combine(basePath, relativePath);

			var info = new FileInfo(filePath);

			var retVal = new UpdateFile {
				RelativePath = relativePath,
				RemoteUrl = remotePath,
				Length = info.Length,
				DateModifiedUtc = info.LastWriteTimeUtc
			};
			using (var hasher = hasherCreator())
			using (var stream = File.OpenRead(filePath))
				retVal.hash = hasher.ComputeHash(stream);
			retVal.signatures = signers.Select(s => s.SignHash(retVal.hash, signatureAlgorithm)).ToList().AsReadOnly();

			return retVal;
		}

		///<summary>Writes this UpdateFile to an XML element for an update manifest.</summary>
		public XElement ToXml() {
			return new XElement("File",
				new XAttribute("RelativePath", RelativePath),
				new XAttribute("Url", RemoteUrl.ToString()),
				new XAttribute("Size", Length.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("Timestamp", DateModifiedUtc.ToString("o", CultureInfo.InvariantCulture)),

				new XElement("Hash", Convert.ToBase64String(hash)),
				signatures.Select(s => new XElement("Signature", Convert.ToBase64String(s)))
			);
		}
		#endregion

		///<summary>Gets the relative path to the file within the application directory.</summary>
		public string RelativePath { get; private set; }
		///<summary>Gets the URL to the encrypted file on the server, relative to the base Updates directory.</summary>
		public Uri RemoteUrl { get; private set; }
		///<summary>Gets the size of the file in bytes.</summary>
		public long Length { get; private set; }
		///<summary>Gets the file's timestamp in UTC.</summary>
		public DateTime DateModifiedUtc { get; private set; }

		///<summary>Returns a new UpdateFile instance with the specified prefix removed, if this file starts with the prefix.  If not, returns this.</summary>
		public UpdateFile StripPrefix(string prefix) {
			if (!RelativePath.StartsWith(prefix))
				return this;
			return new UpdateFile {
				RelativePath = RelativePath.Substring(prefix.Length),
				DateModifiedUtc = DateModifiedUtc,
				hash = hash,
				Length = Length,
				RemoteUrl = RemoteUrl,
				signatures = signatures
			};
		}

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

			Directory.CreateDirectory(Path.GetDirectoryName(filePath)); //Won't throw

			ui = ui ?? new EmptyProgressReporter();

			long actualSize;

			var request = WebRequest.Create(new Uri(UpdateConfig.Standard.BaseUri, RemoteUrl));

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
