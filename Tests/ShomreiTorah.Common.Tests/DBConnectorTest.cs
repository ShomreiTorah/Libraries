using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for DBConnectorTest and is intended
	///to contain all DBConnectorTest Unit Tests
	///</summary>
	[TestClass()]
	public class DBConnectorTest {

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

		static DBConnector Connector { get { return DB.Default; } }

		/// <summary>
		///A test for OpenConnection
		///</summary>
		[TestMethod()]
		public void OpenConnectionTest() {
			using (var connection = Connector.OpenConnection()) {
				Assert.AreEqual(ConnectionState.Open, connection.State);
			}
		}


		/// <summary>
		///A test for CreateCommand
		///</summary>
		[TestMethod()]
		public void CreateCommandTest() {
			using (var command = Connector.CreateCommand("ABCDEFG"))
			using (command.Connection) {
				Assert.IsNotNull(command.Connection);
				Assert.AreEqual(ConnectionState.Open, command.Connection.State);
				Assert.AreEqual("ABCDEFG", command.CommandText);
			}
			using (var command = Connector.CreateCommand("ABCDEFG", new { BoolParam = false, StrParam = "'; DROP DATABASE; --", DecParam = 3.14159625m }))
			using (command.Connection) {
				Assert.IsNotNull(command.Connection);
				Assert.AreEqual(ConnectionState.Open, command.Connection.State);
				Assert.AreEqual("ABCDEFG", command.CommandText);

				Assert.AreEqual(3, command.Parameters.Count);

				Assert.AreEqual(false, command.Parameters["BoolParam"].Value);
				Assert.AreEqual("'; DROP DATABASE; --", command.Parameters["StrParam"].Value);
				Assert.AreEqual(3.14159625m, command.Parameters["DecParam"].Value);
			}
			var date = DateTime.Now;
			Assert.AreEqual(date.Date, Connector.CreateCommand("SELECT @date", new { date }).Execute<DateTime>().Date);
			Assert.AreEqual("ABC123", Connector.CreateCommand("SELECT @Str + @Str2", new { Str = "ABC", Str2 = "123" }).Execute<string>());
		}

		[TestMethod]
		public void NullPatametersTest() {
			Assert.AreEqual(1, Connector.Sql<int>("SELECT 1 WHERE @a IS NULL").Execute(new { a = new int?() }));
		}

		/// <summary>
		///A test for ExecuteNonQuery
		///</summary>
		[TestMethod()]
		public void ExecuteNonQueryTest() {
			Assert.AreEqual(-1, Connector.ExecuteNonQuery("SELECT * FROM sysobjects"));
			Assert.AreEqual(-1, Connector.ExecuteNonQuery("SELECT * FROM sysobjects WHERE type=@Type", new { Type = "U" }));
			//Assert.AreEqual(Connector.ExecuteScalar<int>("SELECT COUNT(*) FROM sysobjects"), Connector.ExecuteNonQuery("SELECT * FROM sysobjects"));
			//Assert.AreEqual(Connector.Sql("SELECT COUNT(*) FROM sysobjects WHERE type=@Type", new { Type = "U" }).Execute<int>(),
			//                Connector.ExecuteNonQuery("SELECT * FROM sysobjects WHERE type=@Type", new { Type = "U" }));

		}

		/// <summary>
		///A test for ExecuteReader
		///</summary>
		[TestMethod()]
		public void ExecuteReaderTest() {
			using (var reader = Connector.ExecuteReader("SELECT 57, getdate(), 'Hello!'")) {
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(57, reader.GetInt32(0));
				Assert.AreEqual(DateTime.Today, reader.GetDateTime(1).Date);
				Assert.AreEqual("Hello!", reader.GetString(2));
			}
			using (var reader = Connector.ExecuteReader("SELECT 57 * @Num", new { Num = 4 })) {
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(228, reader.GetInt32(0));
			}
		}

		/// <summary>
		///A test for ExecuteScalar
		///</summary>
		[TestMethod()]
		public void ExecuteScalarTest() {
			Assert.AreEqual(DateTime.UtcNow.Date, Connector.ExecuteScalar<DateTime>("SELECT getutcdate()").Date);
			Assert.AreEqual(3, Connector.ExecuteScalar<int>("SELECT 1 + 2"));
		}
		/// <summary>
		///A test for Sql
		///</summary>
		[TestMethod()]
		public void SqlTest() {
			Assert.AreEqual(DateTime.UtcNow.Date, Connector.Sql<DateTime>("SELECT getutcdate()").Execute().Date);
			Assert.AreEqual(DateTime.UtcNow.Date, Connector.Sql<DateTime>("SELECT @UtcNow").Execute(new { DateTime.UtcNow }).Date);

			Assert.AreEqual("ABC123", Connector.Sql<string>("SELECT @Str + @Str2").Execute(new { Str = "ABC", Str2 = "123" }));
			Assert.AreEqual(100, Connector.Sql<int>("SELECT @X + @Y").Execute(new { X = 57, Y = 43 }));
		}
	}
}