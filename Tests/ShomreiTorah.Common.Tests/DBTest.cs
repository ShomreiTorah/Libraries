using ShomreiTorah.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for DBTest and is intended
	///to contain all DBTest Unit Tests
	///</summary>
	[TestClass()]
	public class DBTest {

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
		///A test for Default
		///</summary>
		[TestMethod()]
		public void DefaultTest() {
			Assert.IsNotNull(DB.Default);
			Assert.AreEqual(1, DB.Default.ExecuteScalar<int>("SELECT 1"));
		}

		/// <summary>
		///A test for CreateCommand
		///</summary>
		[TestMethod()]
		public void CreateCommandTests() {
			var connection = new SqlConnection();
			var command = connection.CreateCommand("ABCDEFG");
			Assert.AreEqual("ABCDEFG", command.CommandText);
			Assert.AreSame(connection, command.Connection);

			command = connection.CreateCommand("123", new { IntParam = 57, DateParam = DateTime.MaxValue });
			Assert.AreEqual("123", command.CommandText);
			Assert.AreSame(connection, command.Connection);
			Assert.AreEqual(2, command.Parameters.Count);
			Assert.AreEqual(57, command.Parameters["IntParam"].Value);
			Assert.AreEqual(DateTime.MaxValue, command.Parameters["DateParam"].Value);
		}

		[TestMethod()]
		public void AddParametersTest() {
			var command = new SqlCommand();
			command.AddParameters(new { IntParam = 123, DateParam = DateTime.MinValue });

			Assert.AreEqual(2, command.Parameters.Count);
			Assert.AreEqual(123, command.Parameters["IntParam"].Value);
			Assert.AreEqual(DateTime.MinValue, command.Parameters["DateParam"].Value);

			command = new SqlCommand();
			command.AddParameters<object>(null);
			Assert.AreEqual(0, command.Parameters.Count);
		}

		static bool test;
		static DbConnection OpenConnection() { test = !test; return (test ? DB.Test : DB.Default).OpenConnection(); }
		/// <summary>
		///A test for Sql
		///</summary>
		[TestMethod()]
		public void SqlTest() {
			using (var connection = OpenConnection()) {
				Assert.AreEqual(DateTime.Now.Date, connection.Sql<DateTime>("SELECT getdate()").Execute().Date);
				Assert.AreEqual(DateTime.Now.Date, connection.Sql<DateTime>("SELECT @Now").Execute(new { DateTime.Now }).Date);

				Assert.AreEqual("ABC123", connection.Sql<string>("SELECT @Str + @Str2").Execute(new { Str = "ABC", Str2 = "123" }));
				Assert.AreEqual(100, connection.Sql<int>("SELECT @X + @Y").Execute(new { X = 57, Y = 43 }));
			}
		}

		/// <summary>
		///A test for ExecuteNonQuery
		///</summary>
		[TestMethod()]
		public void ExecuteNonQueryTest() {
			using (var connection = OpenConnection()) {
				Assert.AreEqual(-1, connection.ExecuteNonQuery("SELECT * FROM sysobjects"));
				Assert.AreEqual(-1, connection.ExecuteNonQuery("SELECT * FROM sysobjects WHERE type=@Type", new { Type = "U" }));
			}
		}

		/// <summary>
		///A test for ExecuteScalar
		///</summary>
		[TestMethod()]
		public void ExecuteScalarTest() {
			using (var connection = OpenConnection()) {
				Assert.AreEqual(DateTime.UtcNow.Date, connection.ExecuteScalar<DateTime>("SELECT getutcdate()").Date);
				Assert.AreEqual(3, connection.ExecuteScalar<int>("SELECT 1 + 2"));
			}
		}

		/// <summary>
		///A test for ExecuteReader
		///</summary>
		[TestMethod()]
		public void ExecuteReaderTest() {
			using (var connection = OpenConnection()) {
				using (var reader = connection.ExecuteReader("SELECT 57, getdate(), 'Hello!'")) {
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(57, reader.GetInt32(0));
					Assert.AreEqual(DateTime.Today, reader.GetDateTime(1).Date);
					Assert.AreEqual("Hello!", reader.GetString(2));
				}
				using (var reader = connection.ExecuteReader("SELECT 57 * @Num", new { Num = 4 })) {
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(228, reader.GetInt32(0));
				}
			}
		}
	}
}