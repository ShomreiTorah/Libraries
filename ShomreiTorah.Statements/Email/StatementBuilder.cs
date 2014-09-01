using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ShomreiTorah.Common;
using ShomreiTorah.Data;

namespace ShomreiTorah.Statements.Email {
	using Email = Common.Email;

	public class StatementBuilder : IDisposable {
		readonly DirectoryTemplateResolver resolver;
		public StatementBuilder(string basePath, string imagePath) {
			resolver = new DirectoryTemplateResolver(basePath);
			ImagesPath = imagePath;
			TemplateService = new TemplateService(new TemplateServiceConfiguration {
				Resolver = resolver,
				Namespaces = new HashSet<string> {
					"System",
					"System.Collections.Generic",
					"System.Linq",

					//Any namespaces added here should be mirrored in Config\Email Templates\Web.config for design-time support
					"ShomreiTorah.Common",
					"ShomreiTorah.Statements",
					"ShomreiTorah.Statements.Email",
				}
			});
		}


		public IEnumerable<string> Templates { get { return resolver.Templates; } }
		public ITemplateService TemplateService { get; private set; }
		public string ImagesPath { get; private set; }

		static readonly MailAddress BillingAddress = new MailAddress("Billing@" + Config.DomainName, Config.OrgName + " Billing");
		public StatementMessage CreateMessage(Person person, string templateName, DateTime startDate) {
			var page = (StatementPage)TemplateService.Resolve(templateName, null);
			page.SetInfo(person, startDate);

			var images = new EmailAttachmentImageService(ImagesPath);
			var content = page.RenderPage(images);
			if (!page.ShouldSend) return null;

			var message = new StatementMessage(page, templateName) { From = BillingAddress, SubjectEncoding = Email.DefaultEncoding };

			var htmlContent = AlternateView.CreateAlternateViewFromString(content, Email.DefaultEncoding, "text/html");

			htmlContent.TransferEncoding = TransferEncoding.QuotedPrintable;
			htmlContent.LinkedResources.AddRange(images.Attachments);
			message.AlternateViews.Add(htmlContent);

			message.Subject = page.EmailSubject;

			return message;
		}


		///<summary>Releases all resources used by the StatementBuilder.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the StatementBuilder and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				TemplateService.Dispose();
			}
		}
	}

	public class StatementMessage : MailMessage {
		readonly StatementPage page;
		readonly string template;
		public StatementMessage(StatementPage page, string template) {
			this.page = page;
			this.template = template;
		}
		public void LogStatement(string userName = null) {
			page.Log(template, userName);
		}
	}
}