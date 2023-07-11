using System.Net;
using System.Net.Mail;
using CripexLibrary.Models;

namespace CripexLibrary.Services.EmailService
{
	public class EmailServices : IEmailService
	{
		

		async Task IEmailService.SendConfirmationEmail(string email, int quantity, string bookTitle)
		{
			var body = $@"<p>Thank you, we have received your order to borrow Book Title: {bookTitle}</p>
		   
			We're processing your borrow request.<br\>

			We will contact you if we have questions about your order, Thanks!<br\>";

			using (var smtp = new SmtpClient())
			{
				smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtp.PickupDirectoryLocation = @"C:\librarySystemMailPickup";
				var message = new MailMessage();
				message.To.Add(email);
				message.Subject = "Cripex Library - ";
				message.Body = body;
				message.IsBodyHtml = true;
				message.From = new MailAddress("Manager@Cripex.com");
				await smtp.SendMailAsync(message);
			}		
		}


		public async Task SendOverdueEmail(string toEmail, string username, string title)
		{
			var subject = "Reminder: Please return your borrowed book";
			var body = $@"<p>Dear {username},
							<br/>This is a friendly reminder that your borrowed book '{title}' is overdue. 
							Please return it as soon as possible to avoid any late fees. 
							<br/>Best regards,<br/> CriPex Library</p>";

			using (var smtp = new SmtpClient())
			{
				smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtp.PickupDirectoryLocation = @"C:\libraryMail";
				var msg = new MailMessage();
				msg.To.Add(toEmail);
				msg.Subject = subject;
				msg.Body = body;
				msg.IsBodyHtml = true;
				msg.From = new MailAddress("Manager@Cripex.com");
				await smtp.SendMailAsync(msg);
			}
		}

	}
}
