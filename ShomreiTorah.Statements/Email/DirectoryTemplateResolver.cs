using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RazorEngine.Templating;

namespace ShomreiTorah.Statements.Email {
	///<summary>Resolves templates from a directory on disk.</summary>
	public class DirectoryTemplateResolver : ITemplateResolver {
		///<summary>Creates a DirectoryTemplateResolver that reads from a specific directory.</summary>
		public DirectoryTemplateResolver(string directory) {
			TemplateDirectory = directory;
		}

		///<summary>Gets the names of all of the templates in the directory.</summary>
		public IEnumerable<string> Templates {
			get { return Directory.EnumerateFiles(TemplateDirectory, "*.??html").Select(Path.GetFileNameWithoutExtension).Where(n => !n.StartsWith("_", StringComparison.Ordinal)); }
		}

		///<summary>Gets the directory containing the templates.</summary>
		public string TemplateDirectory { get; private set; }

		///<summary>Gets the text of the template with the given name.</summary>
		public string Resolve(string name) {
			var fileName = Directory.EnumerateFiles(TemplateDirectory, name + ".??html").FirstOrDefault();
			if (fileName == null)
				return null;

			return File.ReadAllText(fileName);
		}
	}
}