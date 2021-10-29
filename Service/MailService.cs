using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        public MailService(ILogger<MailService> logger)
        {
            // Creating the SendGrid client
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            _sendGridClient = new SendGridClient(apiKey);

            // Create the logger
            _logger = logger;
        }

        // Creates and sends email from mortgage offer.
        public async Task SendMail(MortgageOffer mortgageOffer)
        {
            // Set sender and recipient
            var from = new EmailAddress("634293@student.inholland.nl", "Inholland Mortgage broker");
            var subject = "Your mortgage offer!";
            var to = new EmailAddress(mortgageOffer.Applicant.Email, $"{mortgageOffer.Applicant.FirstName} {mortgageOffer.Applicant.LastName}");
            
            // Set email content.
            var htmlContent = $"Hi {mortgageOffer.Applicant.FirstName} {mortgageOffer.Applicant.LastName}," +
                $"<br> <p>Your mortgage offer is ready</p>" +
                $"<p>You can find it here:</p><a href=\"{mortgageOffer.PDFUrl}\">Download now</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            
            // Set the send time to approximate 7 hours after midnight. (Mail is created at midnight)
            var sendTime = (DateTimeOffset) DateTime.Now.AddHours(7);                     
            msg.SendAt = sendTime.ToUnixTimeSeconds();

            // Send the email.
            await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);

            // Log when email is created.
            _logger.LogInformation($"Email succelsfully created and will be send to {mortgageOffer.Applicant.Email} at : {sendTime}");
        }
    }
}
