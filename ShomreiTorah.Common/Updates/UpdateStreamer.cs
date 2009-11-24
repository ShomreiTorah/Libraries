using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace ShomreiTorah.Common.Updates {
	///<summary>Manipulates update archives.</summary>
	public static class UpdateStreamer {
		///<summary>Recursively writes all of files in a directory to an update archive.</summary>
		///<param name="target">The stream to write the archive to.  This stream should be encrypted.</param>
		///<param name="folder">The directory containing the files to put in the archive.</param>
		///<param name="progressReporter">An optional IProgressReporter implementation to report progress.</param>
		public static void WriteArchive(Stream target, string folder, IProgressReporter progressReporter) {
			WriteArchive(target, folder, progressReporter, Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
		}

		///<summary>Writes a set of files to an update archive.</summary>
		///<param name="target">The stream to write the archive to.  This stream should be encrypted.</param>
		///<param name="rootPath">The root path that the files in the archive will be made relative to.</param>
		///<param name="progressReporter">An optional IProgressReporter implementation to report progress.</param>
		///<param name="paths">The full paths of the files to put in the archive.</param>
		public static void WriteArchive(Stream target, string rootPath, IProgressReporter progressReporter, params string[] paths) {
			if (target == null) throw new ArgumentNullException("target");
			if (rootPath == null) throw new ArgumentNullException("rootPath");
			if (paths == null) throw new ArgumentNullException("paths");

			var totalLength = paths.Sum(p => new FileInfo(p).Length);
			int totalRead = 0;
			if (progressReporter != null)
				progressReporter.Maximum = (int)totalLength;

			var buffer = new byte[4096];

			target.Write(totalLength);
			target.Write(paths.Length);
			var rootUri = new Uri(rootPath + "\\", UriKind.Absolute);
			for (int i = 0; i < paths.Length; i++) {
				var uri = new Uri(paths[i], UriKind.Absolute);
				var relativePath = rootUri.MakeRelativeUri(uri).ToString();

				if (progressReporter != null) {
					if (progressReporter.WasCanceled)
						return;
					progressReporter.Caption = "Adding " + relativePath;
				}

				target.WriteString(relativePath);
				using (var file = File.Open(paths[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					target.Write(file.Length);

					while (true) {
						if (progressReporter != null) progressReporter.Progress = totalRead;

						var bytesRead = file.Read(buffer, 0, buffer.Length);
						if (bytesRead == 0) break;

						totalRead += bytesRead;

						target.Write(buffer, 0, bytesRead);
					}
				}
			}
		}

		///<summary>Extracts all files from an update archive.</summary>
		///<param name="source">The stream to read the archive from.</param>
		///<param name="destination">The folder to extract the files to.  If the folder does not exist, it will be created.</param>
		///<param name="progressReporter">An optional IProgressReporter implementation to report progress.</param>
		public static void ExtractArchive(Stream source, string destination, IProgressReporter progressReporter) {
			if (source == null) throw new ArgumentNullException("source");
			if (destination == null) throw new ArgumentNullException("destination");

			if (Directory.Exists(destination) && Directory.GetFileSystemEntries(destination).Length > 0)
				throw new ArgumentException("The destination directory cannot have existing files", "destination");

			Directory.CreateDirectory(destination);

			try {
				byte[] buffer = new byte[4096];

				var totalSize = source.Read<long>();
				if (progressReporter != null) progressReporter.Maximum = totalSize > int.MaxValue ? -1 : (int)totalSize;
				int totalRead = 0;

				int fileCount = source.Read<int>();
				for (int i = 0; i < fileCount; i++) {
					var relativePath = source.ReadString();
					if (progressReporter != null && progressReporter.WasCanceled)
						return;

					string targetPath = Path.Combine(destination, relativePath);
					Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
					long length = source.Read<long>();

					if (progressReporter != null) {
						if (progressReporter.WasCanceled)
							return;
						progressReporter.Maximum = length > int.MaxValue ? -1 : (int)length;
						progressReporter.Caption = "Extracting " + relativePath;
					}

					using (var file = File.Create(targetPath)) {
						long bytesRead = 0;
						while (true) {
							if (progressReporter != null) {
								if (progressReporter.WasCanceled)
									return;
								if (progressReporter.Maximum > 0)//If it's less than 4GB
									progressReporter.Progress = totalRead;
							}

							var chunkSize = source.Read(buffer, 0, (int)Math.Min(length - bytesRead, buffer.Length));
							if (chunkSize == 0) throw new InvalidDataException("File is too short");

							bytesRead += chunkSize;
							totalRead += chunkSize;
							file.Write(buffer, 0, chunkSize);
							if (bytesRead >= length) break;
						}
					}
				}
				if (totalSize != totalRead)
					throw new InvalidDataException("Not enough bytes");
			} finally {
				if (progressReporter != null && progressReporter.WasCanceled)
					Directory.Delete(destination, true);
			}
		}
	}
}

