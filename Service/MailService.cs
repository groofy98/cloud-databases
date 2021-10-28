using Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IMailService
    {
        public Task SendMail(MortgageOffer mortgageOffer);
    }
    public class MailService : IMailService
    {
        private readonly SendGridClient _sendGridClient;
        public MailService()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            _sendGridClient = new SendGridClient(apiKey);
            
        }

        public async Task SendMail(MortgageOffer mortgageOffer)
        {
            var from = new EmailAddress("634293@student.inholland.nl", "Inholland Mortgage broker");
            var subject = "Your mortgage offer!";
            var to = new EmailAddress(mortgageOffer.Applicant.Email, $"{mortgageOffer.Applicant.FirstName} {mortgageOffer.Applicant.LastName}");
            var plainTextContent = $"Hi {mortgageOffer.Applicant.FirstName} {mortgageOffer.Applicant.LastName},";
            var htmlContent = $"<br> <p>your mortgage offer is ready</p><br><p>You can find it here:</p><a href=\"{mortgageOffer.PDFUrl}\">Download now</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var sendTime = (DateTimeOffset) DateTime.Now.AddHours(7);
            //msg.SendAt = sendTime.ToUnixTimeSeconds();
            var response = await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }
}
