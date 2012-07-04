using System;
using System.Net;
using RazorEngine.Templating;
using RazorEngine.Text;
using ShomreiTorah.Data;

namespace ShomreiTorah.Statements.Email {
	public abstract class StatementPage : TemplateBase {
		public string EmailSubject {
			get { return ViewBag.EmailSubject; }
			set { ViewBag.EmailSubject = value; }
		}
		public virtual bool ShouldSend {
			get {
				if (Info == null)
					throw new InvalidOperationException("The template host must call SetInfo and render the page, and the template must set Kind, before the host can check ShouldSend");
				return Info.ShouldSend;
			}
		}

		internal void Log(string template, string userName) { Info.LogStatement("Email", template, userName); }

		public void SetInfo(Person person, DateTime startDate) {
			this.person = person;
			this.startDate = startDate;
		}

		private DateTime startDate;
		private Person person;

		protected StatementInfo Info { get; private set; }
		protected StatementKind Kind {
			get {
				if (Info == null)
					throw new InvalidOperationException("Cannot read Kind before setting it");
				return Info.Kind;
			}
			set {
				if (person == null)
					throw new InvalidOperationException("The template host must call SetInfo before the template sets Kind");
				Info = new StatementInfo(person, startDate, value);
			}
		}

		#region HTML Helpers
		protected string ImageUrl(string name) {
			if (ImageService == null)
				throw new InvalidOperationException("The template host must provide an ImageServiceBase implementation.");
			return ImageService.GetUrl(name);
		}
		protected IEncodedString Image(string name, string alt) {
			return base.Raw("<img src=\"" + WebUtility.HtmlEncode(ImageUrl(name)) + "\" alt=\"" + WebUtility.HtmlEncode(alt) + "\" />");
		}
		protected IEncodedString MultiLine(string text) {
			return base.Raw(
				WebUtility.HtmlEncode(text).Replace("\r", "").Replace("\n", "<br />")
			);
		}
		#endregion

		//This property is set by the host on the primary instance.
		//Layout pages will pick up the value from the ViewBag.
		ImageService imageService;
		public ImageService ImageService {
			get { return imageService ?? ViewBag.ImageService; }
			set { imageService = value; }
		}
		public string RenderPage() {
			return ((ITemplate)this).Run(new ExecuteContext() {
				ViewBag = {
					ImageService = imageService
				}
			});
		}
	}
}