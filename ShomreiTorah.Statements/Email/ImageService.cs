using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace ShomreiTorah.Statements.Email {
	///<summary>Resolves images for email statements.</summary>
	public abstract class ImageService {
		///<summary>Gets the folder that contains the images.</summary>
		public string BasePath { get; private set; }

		protected ImageService(string path) { BasePath = path; }

		public string GetUrl(string name) {
			if (!File.Exists(Path.Combine(BasePath, name)))
				throw new FileNotFoundException(name + " does not exist in " + BasePath);
			return CreateUrl(name);
		}
		protected abstract string CreateUrl(string name);
	}

	///<summary>An ImageService that returns local file paths.  (For previews)</summary>
	public class LocalFileImageService : ImageService {
		public LocalFileImageService(string path) : base(path) { }
		protected override string CreateUrl(string name) {
			return Path.Combine(BasePath, name);
		}
	}

	public class EmailAttachmentImageService : ImageService {
		static readonly Dictionary<string, string> MediaTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
			{ ".png",	"image/png"		},
			{ ".gif",	"image/gif"		},
			{ ".jpeg",	"image/jpeg"	},
			{ ".jpg",	"image/jpeg"	},
		};

		public EmailAttachmentImageService(string path) : base(path) { }

		readonly Dictionary<string, LinkedResource> attachments = new Dictionary<string, LinkedResource>();
		public IEnumerable<LinkedResource> Attachments { get { return attachments.Values; } }

		protected override string CreateUrl(string name) {
			if (!attachments.ContainsKey(name))
				attachments.Add(
					name,
					new LinkedResource(Path.Combine(BasePath, name), MediaTypes[Path.GetExtension(name)]) { ContentId = name }
				);
			return "cid:" + name;
		}
	}
}