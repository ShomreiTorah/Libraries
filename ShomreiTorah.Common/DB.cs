using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace ShomreiTorah.Common {
	///<summary>Manages database connections.</summary>
	public static class DB {
		static class DefaultContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static DefaultContainer() { }
			public static readonly DBConnector Instance = new DBConnector(Config.GetElement("Databases", "Default"));
		}
		//If the property is set before it is first read, GetElement will never be called
		static DBConnector defaultOverride;

		///<summary>Gets or sets the default database.</summary>
		public static DBConnector Default {
			get { return defaultOverride ?? DefaultContainer.Instance; }
			set { defaultOverride = value; }
		}

		static class TestContainer {
			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static TestContainer() { }
			public static readonly DBConnector Instance = new DBConnector(Config.GetElement("Databases", "Test"));
		}
		//If the property is set before it is first read, GetElement will never be called
		static DBConnector testOverride;

		///<summary>Gets or sets the test database.</summary>
		public static DBConnector Test {
			get { return testOverride ?? TestContainer.Instance; }
			set { testOverride = value; }
		}

		static readonly Dictionary<string, DbProviderFactory> FactoryNames = new Dictionary<string, DbProviderFactory>(StringComparer.OrdinalIgnoreCase){
			{ "SQL Server",		SqlClientFactory.Instance	},
			{ "SQLServer",		SqlClientFactory.Instance	},
			{ "OleDB",			OleDbFactory.Instance		},
			{ "ODBC",			OdbcFactory.Instance		},
		};

		///<summary>Gets a DbProviderFactory with the given name.</summary>
		///<param name="name">The Type attribute from ShomreiTorahConfig.xml</param>
		public static DbProviderFactory GetFactory(XAttribute name) { return GetFactory(name == null ? null : name.Value); }
		///<summary>Gets a DbProviderFactory with the given name.</summary>
		///<param name="name">The value of the Type attribute from ShomreiTorahConfig.xml</param>
		public static DbProviderFactory GetFactory(string name) {
			if (string.IsNullOrEmpty(name)) return SqlClientFactory.Instance;
			DbProviderFactory retVal;
			if (FactoryNames.TryGetValue(name, out retVal))
				return retVal;
			return DbProviderFactories.GetFactory(name);
		}

		#region OleDB Files
		static string CreateSqlCeConnectionString(string filePath) {
			var builder = new DbConnectionStringBuilder();
			builder["Data Source"] = filePath;
			return builder.ConnectionString;
		}

		static readonly List<KeyValuePair<DatabaseFile, string>> FormatExtensions = new List<KeyValuePair<DatabaseFile, string>> {
			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Access,				".mdb"),
			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Access2007,			".accdb"),

			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Excel,				".xls"),
			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Excel2007,			".xlsx"),
			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Excel2007Binary,	".xlsb"),
			new KeyValuePair<DatabaseFile, string>(DatabaseFile.Excel2007Macro,		".xlsm"),

			new KeyValuePair<DatabaseFile, string>(DatabaseFile.SqlCe,		".sdf"),
		};

		///<summary>Gets the database format that uses the given extension.</summary>
		public static DatabaseFile GetDBType(string extension) {
			var pair = FormatExtensions.FirstOrDefault(kvp => kvp.Value.Equals(extension, StringComparison.OrdinalIgnoreCase));

			if (pair.Value == null)
				throw new ArgumentException("Unrecognized extension: " + extension, "extension");
			return pair.Key;
		}

		///<summary>Gets the file extension for a database format.</summary>
		public static string GetExtension(this DatabaseFile format) { return FormatExtensions.First(kvp => kvp.Key == format).Value; }

		///<summary>Creates a DBConnector that connects to a database file.</summary>
		///<param name="filePath">The path to the file.  The file must exist.  The format is chosen based on the path's extension.</param>
		///<returns>A DBConnector that connects to the path.</returns>
		public static DBConnector OpenFile(string filePath) { return DB.OpenFile(filePath, GetDBType(Path.GetExtension(filePath))); }
		///<summary>Creates a DBConnector that connects to a database file.</summary>
		///<param name="filePath">The path to the file.</param>
		///<param name="format">The format of the file.  The file must exist.</param>
		///<returns>A DBConnector that connects to the path.</returns>
		public static DBConnector OpenFile(string filePath, DatabaseFile format) {
			if (String.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

			var csBuilder = new OleDbConnectionStringBuilder { DataSource = filePath, PersistSecurityInfo = false };

			const string ExcelProperties = "IMEX=0;HDR=YES";
			switch (format) {
				case DatabaseFile.SqlCe:
					return new DBConnector(SqlCeProviderFactory.Instance, CreateSqlCeConnectionString(filePath));

				case DatabaseFile.Access:
					csBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
					break;
				case DatabaseFile.Access2007:
					csBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
					break;

				case DatabaseFile.Excel:
					csBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
					csBuilder["Extended Properties"] = "Excel 8.0;" + ExcelProperties;
					break;
				case DatabaseFile.Excel2007:
					csBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
					csBuilder["Extended Properties"] = "Excel 12.0 Xml;" + ExcelProperties;
					break;
				case DatabaseFile.Excel2007Binary:
					csBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
					csBuilder["Extended Properties"] = "Excel 12.0;" + ExcelProperties;
					break;
				case DatabaseFile.Excel2007Macro:
					csBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
					csBuilder["Extended Properties"] = "Excel 12.0 Macro;" + ExcelProperties;
					break;
			}

			return new DBConnector(OleDbFactory.Instance, csBuilder.ToString());
		}

		///<summary>Creates an empty database file.</summary>
		///<param name="filePath">The path to the file.  If the file already exists, it will be overwritten.  The format is chosen based on the path's extension.</param>
		///<returns>A DBConnector that connects to the path.</returns>
		public static DBConnector CreateFile(string filePath) { return DB.CreateFile(filePath, GetDBType(Path.GetExtension(filePath))); }
		///<summary>Creates an empty database file.</summary>
		///<param name="filePath">The path to the file.  If the file already exists, it will be overwritten.</param>
		///<param name="format">The format of the file.</param>
		///<returns>A DBConnector that connects to the path.</returns>
		public static DBConnector CreateFile(string filePath, DatabaseFile format) {
			switch (format) {
				case DatabaseFile.Access:
				case DatabaseFile.Access2007:
					//OleDB can't create Access databases, so I
					//embedded empty databases in the assembly.
					//I append a dummy extension so Web Publish
					//doesn't break.
					//https://connect.microsoft.com/VisualStudio/feedback/details/808438/loading-an-access-accdb-database-breaks-ftp-web-publish
					using (var originalStream = typeof(DB).Assembly.GetManifestResourceStream("ShomreiTorah.Common.Data." + format.ToString() + format.GetExtension() + ".template"))
					using (var file = File.Create(filePath)) {
						originalStream.CopyTo(file);
					}
					break;
				case DatabaseFile.Excel:
				case DatabaseFile.Excel2007:
				case DatabaseFile.Excel2007Binary:
				case DatabaseFile.Excel2007Macro:
					//It is not possible to have an empty Excel
					//file, so I just delete any existing file.
					//The file will be automatically created if
					//the client creates a table.
					File.Delete(filePath);
					break;
				case DatabaseFile.SqlCe:
					using (var engine = new SqlCeEngine(CreateSqlCeConnectionString(filePath)))
						engine.CreateDatabase();
					break;
				default:
					break;
			}

			return DB.OpenFile(filePath, format);
		}
		#endregion

		///<summary>Executes the command, returning the first value, and closes the connection.</summary>
		///<typeparam name="T">The type to return.</typeparam>
		///<param name="command">The command to execute.</param>
		///<returns>The first column of the first row returned by the query.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public static T Execute<T>(this IDbCommand command) {
			if (command == null) throw new ArgumentNullException("command");

			if (typeof(IDataReader).IsAssignableFrom(typeof(T)))
				return (T)command.ExecuteReader(CommandBehavior.CloseConnection);
			using (command)
			using (command.Connection)
				return command.ExecuteScalar().NullableCast<T>();
		}

		///<summary>Adds a parameter to an existing command.</summary>
		public static IDbDataParameter AddParameter(this IDbCommand command, string name, object value) {
			var param = command.CreateParameter();
			param.ParameterName = name;
			param.Value = value;
			command.Parameters.Add(param);
			return param;
		}

		#region Connection Extension methods
		///<summary>Creates a DbCommand.</summary>
		///<param name="connection">The connection to create the command for.</param>
		///<param name="sql">The SQL of the command.</param>
		public static DbCommand CreateCommand(this DbConnection connection, string sql) { return connection.CreateCommand<object>(sql, null); }
		///<summary>Creates a parameterized DbCommand.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="connection">The connection to create the command for.</param>
		///<param name="sql">The SQL of the command.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static DbCommand CreateCommand<TParameters>(this DbConnection connection, string sql, TParameters parameters) where TParameters : class {
			if (connection == null) throw new ArgumentNullException("connection");

			var retVal = connection.CreateCommand();
			retVal.CommandText = sql;
			retVal.AddParameters(parameters);
			return retVal;
		}

		///<summary>Adds parameters to an IDbCommand from an anonymous type.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="command">The command to add the parameters to.</param>
		///<param name="parameters">An object containing the values of the parameters to add.  If this parameter is null, nothing will happen.</param>
		///<example>
		///command.AddParameters(new { Date = DateTime.Today, User = Environment.UserName, Count = 7 });
		///</example>
		public static void AddParameters<TParameters>(this IDbCommand command, TParameters parameters) where TParameters : class { if (parameters != null) ParamAdders<TParameters>.adder(command, parameters); }

		private static T NullableCast<T>(this object o) {
			if (o != null && o != DBNull.Value)
				return (T)o;
			if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
				throw new NullReferenceException("Cannot convert null result to " + typeof(T));
			return default(T);
		}

		#region Execution

		///<summary>Executes a SQL statement against a connection.</summary>
		///<param name="connection">The connection to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public static int ExecuteNonQuery(this DbConnection connection, string sql) { return connection.ExecuteNonQuery<object>(sql, null); }
		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="connection">The connection to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public static int ExecuteNonQuery<TParameters>(this DbConnection connection, string sql, TParameters parameters) where TParameters : class {
			using (var command = connection.CreateCommand(sql, parameters)) return command.ExecuteNonQuery();
		}

		///<summary>Executes a SQL statement against a connection.</summary>
		///<param name="connection">The connection to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		public static DbDataReader ExecuteReader(this DbConnection connection, string sql) { return connection.ExecuteReader<object>(sql, null); }
		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="connection">The connection to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The DataReader will dispose the connection")]
		public static DbDataReader ExecuteReader<TParameters>(this DbConnection connection, string sql, TParameters parameters) where TParameters : class {
			return connection.CreateCommand(sql, parameters).ExecuteReader();
		}

		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="T">The type to return.</typeparam>
		///<param name="connection">The connection to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The first column of the first row returned by the query.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public static T ExecuteScalar<T>(this DbConnection connection, string sql) { using (var command = connection.CreateCommand(sql)) return command.ExecuteScalar().NullableCast<T>(); }

		///<summary>Creates a typed SQL statement for a connection.</summary>
		///<typeparam name="T">The scalar type returned by the query.</typeparam>
		///<returns>A SqlStatement object with an Execute method.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public static ISqlStatement<T> Sql<T>(this DbConnection connection, string sqlText) { return new SqlStatement<T>(connection, sqlText); }
		class SqlStatement<TReturn> : ISqlStatement<TReturn> {
			internal SqlStatement(DbConnection connection, string sql) { Connection = connection; Sql = sql; }

			public DbConnection Connection { get; private set; }
			public string Sql { get; private set; }

			public TReturn Execute() { return Execute<object>(null); }
			public TReturn Execute<TParameters>(TParameters parameters) where TParameters : class { using (var command = Connection.CreateCommand(Sql, parameters)) return command.ExecuteScalar().NullableCast<TReturn>(); }
		}
		#endregion

		///<summary>Creates a DbDataAdapter.</summary>
		///<param name="factory">The factory used to create the DbDataAdapter.</param>
		///<param name="connection">The connection for the adapter.</param>
		///<param name="selectSql">The SQL for the adapter's select command.</param>
		public static DbDataAdapter CreateDataAdapter(this DbProviderFactory factory, DbConnection connection, string selectSql) {
			if (factory == null) throw new ArgumentNullException("factory");
			if (connection == null) throw new ArgumentNullException("connection");
			if (String.IsNullOrEmpty(selectSql)) throw new ArgumentNullException("selectSql");

			var adapter = factory.CreateDataAdapter();
			adapter.SelectCommand = connection.CreateCommand(selectSql);
			return adapter;
		}
		#endregion

		#region Transaction Extension methods
		///<summary>Creates a DbCommand.</summary>
		///<param name="transaction">The connection to create the command for.</param>
		///<param name="sql">The SQL of the command.</param>
		public static DbCommand CreateCommand(this DbTransaction transaction, string sql) { return transaction.CreateCommand<object>(sql, null); }
		///<summary>Creates a parameterized DbCommand.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="transaction">The transaction to create the command for.</param>
		///<param name="sql">The SQL of the command.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static DbCommand CreateCommand<TParameters>(this DbTransaction transaction, string sql, TParameters parameters) where TParameters : class {
			if (transaction == null) throw new ArgumentNullException("transaction");

			var retVal = transaction.Connection.CreateCommand();
			retVal.Transaction = transaction;
			retVal.CommandText = sql;
			retVal.AddParameters(parameters);
			return retVal;
		}

		#region Execution
		///<summary>Executes a SQL statement against a connection.</summary>
		///<param name="transaction">The transaction to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public static int ExecuteNonQuery(this DbTransaction transaction, string sql) { return transaction.ExecuteNonQuery<object>(sql, null); }
		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="transaction">The transaction to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public static int ExecuteNonQuery<TParameters>(this DbTransaction transaction, string sql, TParameters parameters) where TParameters : class {
			using (var command = transaction.CreateCommand(sql, parameters)) return command.ExecuteNonQuery();
		}

		///<summary>Executes a SQL statement against a connection.</summary>
		///<param name="transaction">The transaction to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		public static DbDataReader ExecuteReader(this DbTransaction transaction, string sql) { return transaction.ExecuteReader<object>(sql, null); }
		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="transaction">The transaction to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The DataReader will dispose the connection")]
		public static DbDataReader ExecuteReader<TParameters>(this DbTransaction transaction, string sql, TParameters parameters) where TParameters : class {
			return transaction.CreateCommand(sql, parameters).ExecuteReader();
		}

		///<summary>Executes a SQL statement against a connection.</summary>
		///<typeparam name="T">The type to return.</typeparam>
		///<param name="transaction">The transaction to the database.  The connection is not closed.</param>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The first column of the first row returned by the query.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public static T ExecuteScalar<T>(this DbTransaction transaction, string sql) { using (var command = transaction.CreateCommand(sql)) return command.ExecuteScalar().NullableCast<T>(); }

		///<summary>Creates a typed SQL statement for a connection.</summary>
		///<typeparam name="T">The scalar type returned by the query.</typeparam>
		///<returns>A SqlStatement object with an Execute method.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public static ISqlStatement<T> Sql<T>(this DbTransaction transaction, string sqlText) { return new SqlTransactionStatement<T>(transaction, sqlText); }
		class SqlTransactionStatement<TReturn> : ISqlStatement<TReturn> {
			internal SqlTransactionStatement(DbTransaction transaction, string sql) { Transaction = transaction; Sql = sql; }

			public DbTransaction Transaction { get; private set; }
			public string Sql { get; private set; }

			public TReturn Execute() { return Execute<object>(null); }
			public TReturn Execute<TParameters>(TParameters parameters) where TParameters : class { using (var command = Transaction.CreateCommand(Sql, parameters)) return command.ExecuteScalar().NullableCast<TReturn>(); }
		}
		#endregion

		///<summary>Creates a DbDataAdapter.</summary>
		///<param name="factory">The factory used to create the DbDataAdapter.</param>
		///<param name="transaction">The transaction for the adapter.</param>
		///<param name="selectSql">The SQL for the adapter's select command.</param>
		public static DbDataAdapter CreateDataAdapter(this DbProviderFactory factory, DbTransaction transaction, string selectSql) {
			if (factory == null) throw new ArgumentNullException("factory");
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (String.IsNullOrEmpty(selectSql)) throw new ArgumentNullException("selectSql");

			var adapter = factory.CreateDataAdapter();
			adapter.SelectCommand = transaction.CreateCommand(selectSql);
			return adapter;
		}
		#endregion

		#region ParamAdder
		static readonly MethodInfo createParameterMethod = typeof(IDbCommand).GetMethod("CreateParameter");
		static readonly MethodInfo getParametersMethod = typeof(IDbCommand).GetProperty("Parameters").GetGetMethod();
		static readonly MethodInfo setParameterNameMethod = typeof(IDataParameter).GetProperty("ParameterName").GetSetMethod();
		static readonly MethodInfo setValueMethod = typeof(IDataParameter).GetProperty("Value").GetSetMethod();
		static readonly MethodInfo addMethod = typeof(IList).GetMethod("Add");
		static readonly FieldInfo dbNull = typeof(DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public);
		static class ParamAdders<TParam> where TParam : class {
			public delegate void ParamAdder(IDbCommand command, TParam parameters);
			public static readonly ParamAdder adder = CreateParamAdder();

			private static ParamAdder CreateParamAdder() {
				var parametersType = typeof(TParam);

				var addInputParam = new DynamicMethod(
					"ParamAdder" + parametersType.GetHashCode().ToString(CultureInfo.InvariantCulture),
					null, new Type[] { typeof(IDbCommand), parametersType }, parametersType);

				var generator = addInputParam.GetILGenerator();

				PropertyInfo[] properties = parametersType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				if (properties.Length > 0) {
					generator.DeclareLocal(typeof(IDbDataParameter));

					for (int i = 0; i < properties.Length; i++) {
						var property = properties[i];

						if (property.CanRead) {
							generator.Emit(OpCodes.Ldarg_0);
							generator.EmitCall(OpCodes.Callvirt, createParameterMethod, null);
							generator.Emit(OpCodes.Stloc_0);

							generator.Emit(OpCodes.Ldloc_0);
							generator.Emit(OpCodes.Ldstr, property.Name);
							generator.EmitCall(OpCodes.Callvirt, setParameterNameMethod, null);

							generator.Emit(OpCodes.Ldloc_0);
							generator.Emit(OpCodes.Ldarg_1);
							var propertyGet = property.GetGetMethod();
							generator.EmitCall(propertyGet.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
								 propertyGet, null);

							if (property.PropertyType.IsValueType
							 && (!property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))) {
								generator.Emit(OpCodes.Box, property.PropertyType);
								generator.EmitCall(OpCodes.Callvirt, setValueMethod, null);
							} else {
								LocalBuilder propertyValue;
								if (property.PropertyType.IsValueType) {
									propertyValue = generator.DeclareLocal(typeof(object));
									generator.Emit(OpCodes.Box, property.PropertyType);
								} else
									propertyValue = generator.DeclareLocal(property.PropertyType);

								generator.Emit(OpCodes.Stloc, propertyValue);
								generator.Emit(OpCodes.Ldloc, propertyValue);
								var dbNullLoad = generator.DefineLabel();
								var valueLoad = generator.DefineLabel();
								generator.Emit(OpCodes.Brfalse_S, dbNullLoad);
								generator.Emit(OpCodes.Ldloc, propertyValue);
								generator.Emit(OpCodes.Br_S, valueLoad);
								generator.MarkLabel(dbNullLoad);
								generator.Emit(OpCodes.Ldsfld, dbNull);
								generator.MarkLabel(valueLoad);
								generator.EmitCall(OpCodes.Callvirt, setValueMethod, null);
							}

							generator.Emit(OpCodes.Ldarg_0);
							generator.EmitCall(OpCodes.Callvirt, getParametersMethod, null);
							generator.Emit(OpCodes.Ldloc_0);
							generator.EmitCall(OpCodes.Callvirt, addMethod, null);
							generator.Emit(OpCodes.Pop);
						}
					}
				}

				generator.Emit(OpCodes.Ret);
				return (ParamAdder)addInputParam.CreateDelegate(typeof(ParamAdder));
			}
		}
		#endregion
	}

	///<summary>A format for a database file.</summary>
	public enum DatabaseFile {
		///<summary>An Access 2003 .mdb file.</summary>
		Access,
		///<summary>An Access 2007 .accdb file.</summary>
		Access2007,

		///<summary>An Excel 97-2003 .xls file.</summary>
		Excel,
		///<summary>An Excel 2007 .xlsx file.</summary>
		Excel2007,
		///<summary>An Excel 2007 .xlsb binary file.</summary>
		Excel2007Binary,
		///<summary>An Excel 2007 .xlsm file with macros.</summary>
		Excel2007Macro,

		///<summary>A SQL Server Compact Edition database.</summary>
		SqlCe
	}

	///<summary>Connects to a database.</summary>
	public class DBConnector {
		///<summary>Creates a DBConnector from a Database element in ShomreiTorahConfig.xml.</summary>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Co-constructor call")]
		public DBConnector(XElement configElement)	//The Type attribute is optional
			: this(DB.GetFactory(configElement.Attribute("Type") ?? configElement.Attribute("Provider") ?? configElement.Attribute("Factory")),
				   configElement.Attribute("ConnectionString").Value) { }

		///<summary>Creates a DBConnector instance.</summary>
		public DBConnector(DbProviderFactory factory, string connectionString) { Factory = factory; ConnectionString = connectionString; }

		///<summary>Gets the connection string used to connect to the database server.</summary>
		public string ConnectionString { get; private set; }
		///<summary>Gets a factory that creates db objects.</summary>
		public DbProviderFactory Factory { get; private set; }

		///<summary>Opens a connection to the database.</summary>
		public DbConnection OpenConnection() {
			var retVal = Factory.CreateConnection();
			retVal.ConnectionString = ConnectionString;
			retVal.Open();
			return retVal;
		}
		///<summary>Creates a DbCommand for this database.</summary>
		///<param name="sql">The SQL of the command.</param>
		public DbCommand CreateCommand(string sql) { return CreateCommand<object>(sql, null); }
		///<summary>Creates a parameterized DbCommand for this database.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="sql">The SQL of the command.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		public DbCommand CreateCommand<TParameters>(string sql, TParameters parameters) where TParameters : class { return OpenConnection().CreateCommand(sql, parameters); }

		///<summary>Creates a typed SQL statement.</summary>
		///<typeparam name="T">The scalar type returned by the query.</typeparam>
		///<returns>A SqlStatement object with an Execute method.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public ISqlStatement<T> Sql<T>(string sqlText) { return new SqlStatement<T>(this, sqlText); }

		class SqlStatement<TReturn> : ISqlStatement<TReturn> {
			public SqlStatement(DBConnector connector, string sql) { Connector = connector; Sql = sql; }

			public DBConnector Connector { get; private set; }
			public string Sql { get; private set; }

			public TReturn Execute() { return Execute<object>(null); }
			public TReturn Execute<TParameters>(TParameters parameters) where TParameters : class { return Connector.CreateCommand(Sql, parameters).Execute<TReturn>(); }
		}

		#region Execute shortcuts
		///<summary>Executes a SQL statement against this database.</summary>
		///<typeparam name="T">The type to return.</typeparam>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The first column of the first row returned by the query.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Avoid casting")]
		public T ExecuteScalar<T>(string sql) { return CreateCommand(sql).Execute<T>(); }

		///<summary>Executes a SQL statement against this database.</summary>
		///<param name="sql">The SQL to execute.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public int ExecuteNonQuery(string sql) { return ExecuteNonQuery<object>(sql, null); }
		///<summary>Executes a SQL statement against this database.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>The number of rows affected by the statement.</returns>
		public int ExecuteNonQuery<TParameters>(string sql, TParameters parameters) where TParameters : class {
			using (var command = CreateCommand(sql, parameters))
			using (command.Connection)
				return command.ExecuteNonQuery();
		}

		///<summary>Executes a SQL statement against this database.</summary>
		///<param name="sql">The SQL to execute.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		public DbDataReader ExecuteReader(string sql) { return ExecuteReader<object>(sql, null); }
		///<summary>Executes a SQL statement against this database.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="sql">The SQL to execute.</param>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>A DbDataReader object, which will close its underlying connection when disposed.</returns>
		public DbDataReader ExecuteReader<TParameters>(string sql, TParameters parameters) where TParameters : class { return CreateCommand(sql, parameters).ExecuteReader(CommandBehavior.CloseConnection); }
		#endregion
	}

	///<summary>Contains a SQL statement and its return type.</summary>
	///<typeparam name="TReturn">The scalar type returned by the SQL statement.</typeparam>
	///<remarks>This interface is returned by DBConnector.Sql and DbConnection.Sql.</remarks>
	public interface ISqlStatement<TReturn> {
		///<summary>Gets the SQL statement.</summary>
		string Sql { get; }
		///<summary>Executes the query.</summary>
		///<returns>The first column of the first row returned by the query.</returns>
		TReturn Execute();
		///<summary>Executes a parameterized query.</summary>
		///<typeparam name="TParameters">A type containing public properties to add as parameters.</typeparam>
		///<param name="parameters">An object containing the values of the parameters to add.</param>
		///<returns>The first column of the first row returned by the query.</returns>
		TReturn Execute<TParameters>(TParameters parameters) where TParameters : class;
	}
}