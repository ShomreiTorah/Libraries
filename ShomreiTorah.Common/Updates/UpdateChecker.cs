using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace ShomreiTorah.Common.Updates {
	///<summary>Checks for updates.</summary>
	public class UpdateChecker {
		#region Read ShomreiTorahConfig
		///<summary>The key size for the AES algorithm that encrypts updates.</summary>
		public const int UpdateKeySize = 256;
		///<summary>The block size for the AES algorithm that encrypts updates.</summary>
		public const int UpdateBlockSize = 256;

		static SymmetricAlgorithm CreateKD() {
			var retVal = SymmetricAlgorithm.Create(Config.ReadAttribute("Updates", "Cryptography", "BlobDecryptor", "Algorithm"));

			var key = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "BlobDecryptor", "Key").Value);
			var iv = Convert.FromBase64String(Config.GetElement("Updates", "Cryptography", "BlobDecryptor", "IV").Value);

			retVal.KeySize = key.Length * 8;
			retVal.BlockSize = iv.Length * 8;

			retVal.Key = key;
			retVal.IV = iv;
			return retVal;
		}

		///<summary>Gets the SymmetricAlgorithm used to encrypt the Blob element in the update XML.</summary>
		static readonly SymmetricAlgorithm BlobAlgorithm = CreateKD();

		///<summary>Creates an ICryptoTransform that encrypts the Blob element in the update XML.</summary>
		public static ICryptoTransform CreateBlobEncryptor() { return BlobAlgorithm.CreateEncryptor(); }
		///<summary>Creates an ICryptoTransform that decrypts the Blob element in the update XML.</summary>
		public static ICryptoTransform CreateBlobDecryptor() { return BlobAlgorithm.CreateDecryptor(); }

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
		///<param name="currentVersion">The verion of the product that is currently installed.</param>
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
				var document = XDocument.Load(new Uri(BaseUri, new Uri(ProductName + ".xml", UriKind.Relative)).ToString());
				if (document == null) return null;
				return new UpdateInfo(document.Root);
			} catch { return null; }
		}
	}

	///<summary>Describes an update for a product.</summary>
	public class UpdateInfo {
		readonly RijndaelManaged decryptor;
		readonly byte[] signature;
		internal UpdateInfo(XElement element) {
			ProductName = element.Attribute("Name").Value;
			NewVersion = new Version(element.Attribute("NewVersion").Value);
			PublishDate = DateTime.Parse(element.Attribute("PublishDate").Value, CultureInfo.InvariantCulture);
			Description = element.Element("Description").Value.Replace("\n", "\r\n").Replace("\r\r", "\r");

			var blob = UpdateChecker.CreateBlobDecryptor().TransformBytes(Convert.FromBase64String(element.Element("Blob").Value));

			var key = new byte[UpdateChecker.UpdateKeySize / 8];
			Buffer.BlockCopy(blob, 0, key, 0, key.Length);

			var iv = new byte[UpdateChecker.UpdateBlockSize / 8];
			Buffer.BlockCopy(blob, key.Length, iv, 0, iv.Length);

			decryptor = new RijndaelManaged { KeySize = UpdateChecker.UpdateKeySize, BlockSize = UpdateChecker.UpdateBlockSize, Key = key, IV = iv };

			signature = new byte[blob.Length - key.Length - iv.Length];
			Buffer.BlockCopy(blob, key.Length + iv.Length, signature, 0, signature.Length);
		}

		///<summary>Gets the name of the product that the update applies to.</summary>
		public string ProductName { get; private set; }
		///<summary>Gets the version supplied by the update.</summary>
		public Version NewVersion { get; private set; }
		///<summary>Gets the date the the update was published.</summary>
		public DateTime PublishDate { get; private set; }
		///<summary>Gets a description of the update.</summary>
		public string Description { get; private set; }

		///<summary>Downloads the update and extracts its files to a temporary directory.</summary>
		///<returns>The path to the extracted files.</returns>
		public string ExtractFiles(IProgressReporter progress) {
			var request = WebRequest.Create(new Uri(UpdateChecker.BaseUri, new Uri(ProductName + ".Update", UriKind.Relative)));

			var path = Path.GetTempFileName();
			File.Delete(path);
			Directory.CreateDirectory(path);

			try {
				using (var response = request.GetResponse())
				using (var cypherStream = response.GetResponseStream())
				using (var transform = decryptor.CreateDecryptor())
				using (var hasher = new SHA512CryptoServiceProvider())
				using (var hashingStream = new CryptoStream(cypherStream, hasher, CryptoStreamMode.Read))
				using (var decryptingStream = new CryptoStream(hashingStream, transform, CryptoStreamMode.Read))
				using (var unzipper = new GZipStream(decryptingStream, CompressionMode.Decompress)) {
					UpdateStreamer.ExtractArchive(unzipper, path, progress);

					if (progress.WasCanceled) return null;
					var hash = hasher.Hash;
					if (!UpdateChecker.UpdateVerifier.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), signature))
						throw new InvalidDataException("Bad signature");
				}
			} catch (Exception ex) {
				if (progress.WasCanceled) return null;	//If it was canceled, we'l get a CryptoException because the CryptoStream was closed 

				if (Directory.Exists(path))
					Directory.Delete(path, true);
				throw new UpdateErrorException(ex);
			}
			return path;
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
		///<param name="currentVersion">The current verion of the product.</param>
		public UpdatableAttribute(string productName, string currentVersion) { ProductName = productName; CurrentVersion = new Version(currentVersion); }

		///<summary>Gets the name of the product to update.</summary>
		public string ProductName { get; private set; }
		///<summary>Gets the currently installed version.</summary>
		public Version CurrentVersion { get; private set; }
	}
}
