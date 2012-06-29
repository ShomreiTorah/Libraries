using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using SysConfig = System.Configuration;

namespace ShomreiTorah.Common {
	///<summary>Reads the common configuration file (ShomreiTorahConfig.xml)</summary>
	///<remarks>This class looks for ShomreiTorahConfig.xml as described below:
	///First, it checks AppSettings for a setting named ShomreiTorahConfig.xml.
	///
	///If that setting does not exist or if its path was not found,
	///it checks HKEY_CURRENT_USER\SOFTWARE\Shomrei Torah\ShomreiTorahConfig.xml.
	///
	///If that registry key does not exist or if its path was not found,
	///it checks HKEY_LOCAL_MACHINE\SOFTWARE\Shomrei Torah\ShomreiTorahConfig.xml.
	///
	///If that registry key does not exist or if its path was not found,
	///it tries L:\Community\Rabbi Weinberger's Shul\ShomreiTorahConfig.xml
	///
	///If that path was not found, it throws an exception.
	///
	///This procedure can be skipped by setting Config.FilePath before the XML
	///file is loaded.  (if Config.FileLoaded is false)
	///</remarks>
	public static class Config {
		///<summary>Gets the XDocument containing the config file.</summary>
		public static XDocument Xml { get { return FileLoader.File; } }

		///<summary>Reads the value at an XPath expression in the config file.</summary>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Return type")]
		public static T ReadValue<T>(string xpath) { return (T)Xml.XPathEvaluate(xpath); }
		///<summary>Reads the value at an XPath expression in the config file.</summary>
		public static string ReadString(string xpath) { return ReadValue<string>(xpath); }

		///<summary>Gets a specific element.</summary>
		///<param name="path">An array of element names leading to the desired element.</param>
		///<example>Config.GetElement("Journal", "CallListInfo") returns the CallListInfo element in the Journal element.</example>
		public static XElement GetElement(params string[] path) {
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length == 0) throw new ArgumentException("Path cannot be empty", "path");

			XElement element = Xml.Root;
			for (int i = 0; i < path.Length; i++) {
				element = element.Element(path[i]);
				if (element == null) throw new ConfigurationException("Config file doesn't have element " + String.Join("/", path));
			}
			return element;
		}
		///<summary>Reads the value of an attribute.</summary>
		///<param name="path">An array of element names leading to the attribute, followed by the name of the attribute.</param>
		///<example>Config.ReadAttribute("Database", "ConnectionString") returns the ConnectionString attribute of the Database element.</example>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Co-constructor call")]
		public static string ReadAttribute(params string[] path) {
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length < 2) throw new ArgumentException("Path must have at least two elements", "path");

			var attributeName = path[path.Length - 1];
			Array.Resize(ref path, path.Length - 1);

			var attribute = GetElement(path).Attribute(attributeName);
			if (attribute == null) throw new ConfigurationException("Config file doesn't have attribute " + attributeName + " in element " + String.Join("/", path));
			return attribute.Value;
		}
		#region Loader
		///<summary>The default path for ShomreiTorahConfig.xml.</summary>
		public const string DefaultPath = @"L:\Community\Rabbi Weinberger's Shul\ShomreiTorahConfig.xml";
		static string pathOverride;
		///<summary>Gets or sets the full path to the config file.</summary>
		///<exception cref="System.IO.FileNotFoundException">The path does not exist</exception>
		///<exception cref="System.InvalidOperationException">The configuration has already been loaded.</exception>
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public static string FilePath {
			get {
				if (!String.IsNullOrEmpty(pathOverride))
					return pathOverride;

				if (File.Exists("ShomreiTorahConfig.xml"))	//First, try the current directory.
					return Path.GetFullPath("ShomreiTorahConfig.xml");

#if DEBUG

#endif

				var appConfigPath = SysConfig.ConfigurationManager.AppSettings["ShomreiTorahConfig.xml"];
				if (!String.IsNullOrEmpty(appConfigPath) && File.Exists(appConfigPath))
					return appConfigPath;

				var regConfigPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Shomrei Torah\", "ShomreiTorahConfig.xml", null)
								 ?? (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Shomrei Torah\", "ShomreiTorahConfig.xml", null);
				if (!String.IsNullOrEmpty(regConfigPath) && File.Exists(regConfigPath))
					return regConfigPath;

				if (File.Exists(DefaultPath))
					return DefaultPath;

				throw new ConfigurationException("Cannot find ShomreiTorahConfig.xml.\r\n" + appConfigPath ?? regConfigPath ?? DefaultPath + " does not exist.\r\nCheck the value of SOFTWARE\\Shomrei Torah\\ShomreiTorahConfig.xml in HKEY_CURRENT_USER or HKEY_LOCAL_MACHINE.");
			}
			set {
				if (pathOverride == value)
					return;
				if (!File.Exists(value)) throw new FileNotFoundException("The file " + value + " does not exist", value);
				if (Loaded)
					throw new InvalidOperationException("ShomreiTorahConfig.xml has already been loaded");

				//Before setting the property, make sure the file is valid.
				//Otherwise, we'll get an unfixable TypeInit exception when
				//reading the Xml property.
				using (var reader = XmlReader.Create(value))
					while (reader.Read()) ;

				pathOverride = value;
			}
		}
		///<summary>Indicates whether the config file has been loaded yet.</summary>
		public static bool Loaded { get; private set; }
		static class FileLoader {

			[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Prevent beforefieldinit")]
			static FileLoader() { }
			public static readonly XDocument File = Load();
			static XDocument Load() {
				try {
					var doc = XDocument.Load(FilePath);
					Loaded = true;
					return doc;
				} catch (ConfigurationException) {
					throw;
				} catch (Exception ex) {
					throw new ConfigurationException("An error occurred while loading ShomreiTorahConfig.xml from " + FilePath + "\r\nCheck that the XML file is accessible and has no syntax errors.", ex);
				}
			}
		}
		#endregion
	}
	///<summary>The exception thrown because of a problem with ShomreiTorahConfig.xml.</summary>
	[Serializable]
	public class ConfigurationException : Exception {
		///<summary>Creates a ConfigurationException with the default message.</summary>
		public ConfigurationException() : this("There was a problem with ShomreiTorahConfig.xml") { }

		///<summary>Creates a ConfigurationException with the given message.</summary>
		public ConfigurationException(string message) : base(message) { }
		///<summary>Creates a ConfigurationException with the given message and inner exception.</summary>
		public ConfigurationException(string message, Exception inner) : base(message, inner) { }
		///<summary>Deserializes a ConfigurationException.</summary>
		protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
