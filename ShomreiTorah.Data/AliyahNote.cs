using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ShomreiTorah.Common;
using System.Collections.ObjectModel;

namespace ShomreiTorah.Data {
	///<summary>Contains the parsed structure of an עליה pledge's Note field.</summary>
	///<remarks>This class is mutable.</remarks>
	public class AliyahNote {
		///<summary>Gets the string that represents an empty relative.</summary>
		public const string EmptyRelative = "(None)";

		///<summary>Creates an empty </summary>
		public AliyahNote() { }
		///<summary>Creates an AliyahNote instance from an existing pledge.</summary>
		public AliyahNote(string text) { Text = text; }

		string relative;
		///<summary>Gets or sets the name of the relative, if any.</summary>
		public string Relative {
			get { return relative; }
			set {
				value = (value ?? "").Trim();

				if (value.Equals(EmptyRelative, StringComparison.OrdinalIgnoreCase))
					value = "";
				else if (!String.IsNullOrEmpty(value)) {	//Try to normalize case and hyphens against the original list
					var index = RelationMatchers.FindIndex(r => r.IsMatch(value));
					if (index >= 0)
						value = Names.RelationNames[index];
					else
						throw new ArgumentException("Unsupported relative " + value, "value");
				}
				relative = value;
			}
		}
		///<summary>Gets or sets whether the pledge is marked as a מתנה.</summary>
		public bool Isמתנה { get; set; }
		///<summary>Gets or sets any additional note for the pledge.</summary>
		public string ExtraNote { get; set; }

		static readonly Regex PartBreak = new Regex(@"[- ]+");
		static readonly ReadOnlyCollection<Regex> RelationMatchers =
			Names.RelationNames
					.Select(Regex.Escape)
					.Select(r => "^" + PartBreak.Replace(r, "[ -]+") + "$")	//Force the entire string to match to prevent son-in-law issues
					.Select(s => new Regex(s, RegexOptions.IgnoreCase))
					.ReadOnlyCopy();

		static Regex CreateParser() {
			StringBuilder text = new StringBuilder(@"^\s*");
			text.Append(@"(");

			text.AppendJoin(
				RelationMatchers
					.Select(r => r.ToString().TrimStart('^').TrimEnd('$'))	//Remove the anchors
					.OrderByDescending(r => r.Length)
					.Concat(new[] { Regex.Escape(EmptyRelative) }),
				"|"
			);
			text.Append(@")?");
			text.Append(@"\s*[,.;/\\]?\s*");
			text.Append(@"(מתנה|Matanah?)?");	//TODO: Allow prefix מתנה
			text.Append(@"\s*[,.;/\\]?\s*");
			text.Append(@"(.*)");
			text.Append(@"\s*$");
			return new Regex(text.ToString(), RegexOptions.IgnoreCase);
		}
		static readonly Regex TextParser = CreateParser();

		///<summary>Gets or sets the full text of the Note field.</summary>
		public string Text {
			set {
				var parts = TextParser.Match(value ?? "");
				Relative = parts.Groups[1].Value;	//Normalized in the setter
				Isמתנה = !String.IsNullOrEmpty(parts.Groups[2].Value);
				ExtraNote = parts.Groups[3].Value.Trim();
			}
			get {
				string text = "";

				if (!String.IsNullOrWhiteSpace(Relative) && !Relative.Equals(EmptyRelative, StringComparison.OrdinalIgnoreCase))
					text = Relative;

				if (Isמתנה) {
					if (text.Length > 0)
						text += ", מתנה";
					else
						text = "מתנה";
				}
				if (!String.IsNullOrWhiteSpace(ExtraNote)) {
					if (text.Length > 0)
						text += ".  ";
					text += ExtraNote;
				}
				return text;
			}
		}

		///<summary>Returns the full text of the Note field.</summary>
		public override string ToString() { return Text; }
	}
}
