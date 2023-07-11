using CripexLibrary.Models;

namespace CripexLibrary.Services.EmailService
{
	public interface IEmailService
	{
		public Task SendConfirmationEmail(string email, int quantity, string bookTitle);

		public Task SendOverdueEmail(string toEmail, string username, string title);
	}
}
