using ShomreiTorah.Common;
using ShomreiTorah.Common.Updates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using System.Linq;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for UpdateStreamerTest and is intended
	///to contain all UpdateStreamerTest Unit Tests
	///</summary>
	[TestClass]
	public class UpdateStreamerTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for WriteArchive
		///</summary>
		[TestMethod]
		public void WriteArchiveTest() {
			foreach (var progress in new[] { null, new EmptyProgressReporter() }) {

				var rand = new Random();
				var files = Enumerable.Repeat(0, rand.Next(1, 20)).Select(i => {
					var bytes = new byte[rand.Next(10, 16384)];
					rand.NextBytes(bytes);
					return new {
						Bytes = bytes,
						Name = Enumerable.Repeat(0, rand.Next(1, 4)).Select(j => Path.GetRandomFileName()).Join("\\") //new string(Array.ConvertAll(new char[rand.Next(4, 32)], c => (char)('a' + rand.Next(0, 26))))
					};
				}).ToArray();

				var folder = CreateTempDir();
				foreach (var file in files) {
					var fullPath = Path.Combine(folder, file.Name);
					Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
					File.WriteAllBytes(fullPath, file.Bytes);
				}

				var stream = new MemoryStream();
				UpdateStreamer.WriteArchive(stream, folder, progress);
				stream.Position = 0;
				File.WriteAllBytes(Path.GetTempFileName(), stream.ToArray());

				var newFolder = CreateTempDir();
				UpdateStreamer.ExtractArchive(stream, newFolder, progress);

				foreach (var file in files) {
					CollectionAssert.AreEqual(file.Bytes, File.ReadAllBytes(Path.Combine(newFolder, file.Name)));
				}
			}
		}
		static string CreateTempDir() {
			var path = Path.GetTempFileName();
			File.Delete(path);
			Directory.CreateDirectory(path);
			return path;
		}
	}
}
