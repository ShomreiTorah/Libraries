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

	public class StatementBuilder {
		readonly DirectoryTemplateResolver resolver;
		public StatementBuilder(string basePath, string imagePath) {
			resolver = new DirectoryTemplateResolver(basePath);
			ImagesPath = imagePath;
			TemplateService = new TemplateService(new TemplateServiceConfiguration {
				Resolver = resolver,
				Namespaces = new HashSet<string> { "ShomreiTorah.Common" }
			});
		}


		public IEnumerable<string> Templates { get { return resolver.Templates; } }
		public ITemplateService TemplateService { get; private set; }
		public string ImagesPath { get; private set; }

		static readonly MailAddress BillingAddress = new MailAddress("Billing@" + Config.DomainName, Config.OrgName + " Billing");
		public StatementMessage CreateMessage(Person person, string templateName, DateTime startDate) {
			var page = (StatementPage)TemplateService.Resolve(templateName);
			page.SetInfo(person, startDate);

			var images = new EmailAttachmentImageService(ImagesPath);
			page.ImageService = images;

			var content = page.RenderPage();
			if (!page.ShouldSend) return null;

			var message = new StatementMessage(page, templateName) { From = BillingAddress, SubjectEncoding = Email.DefaultEncoding };

			var htmlContent = AlternateView.CreateAlternateViewFromString(content, Email.DefaultEncoding, "text/html");

			htmlContent.TransferEncoding = TransferEncoding.QuotedPrintable;
			htmlContent.LinkedResources.AddRange(images.Attachments);
			message.AlternateViews.Add(htmlContent);

			message.Subject = page.EmailSubject;

			return message;
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