using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace UpdateApplier {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			try {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				var parentId = int.Parse(args[0], CultureInfo.InvariantCulture);
				Process opener = Process.GetProcessById(parentId);

				var sourceFolder = args[1];
				var destFolder = args[2];
				var restartCommand = GetCommandLine((uint)parentId);

				//The parent process calls ReadLine, so it will wait for this line before quitting.
				//Otherwise, it might quit before we start.  See UpdateChecker.ApplyUpdate.
				Console.WriteLine("Please quit");
				opener.WaitForExit();

				FileSystem.MoveDirectory(sourceFolder, destFolder, true);

				Launch(restartCommand + " Updated");
			} catch (Exception ex) {
				var message = new StringBuilder();
				message.AppendLine("An error occurred while applying the update.\r\nPlease contact Schabse; you can press Ctrl+C to copy this message.\r\n");
				message.AppendLine(ex.ToString());
				foreach (DictionaryEntry kvp in ex.Data) {
					var subEx = kvp.Value as Exception;

					message.AppendLine().Append(kvp.Key).Append(": ").Append(subEx != null ? subEx.Message : kvp.Value.ToString());
				}

				MessageBox.Show(message.ToString(), "Shomrei Torah", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		static string GetCommandLine(uint id) {
			using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM WIN32_PROCESS where ProcessId=" + id.ToString(CultureInfo.InvariantCulture)))
			using (var results = searcher.Get())
				return (string)results.Cast<ManagementObject>().First()["CommandLine"];
		}
		static void Launch(string commandLine) {
			string fileName, args;

			int pathEnd;
			if (commandLine[0] == '"') {		//For some reason, I decided to handle quoted paths with embedded (doubled) quotes, even at the beginning
				pathEnd = -1;

				do {
					pathEnd++;
					pathEnd = commandLine.IndexOf('"', pathEnd + 1);
				} while (commandLine[pathEnd + 1] == '"');

				fileName = commandLine.Substring(1, pathEnd - 1).Replace("\"\"", "\"");
				args = commandLine.Substring(pathEnd + 1);
			} else {
				pathEnd = commandLine.IndexOf(' ');

				fileName = commandLine.Remove(pathEnd);
				args = commandLine.Substring(pathEnd + 1);
			}

			Process.Start(fileName, args);
		}
	}
}
