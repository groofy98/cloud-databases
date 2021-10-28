using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Service;

namespace Cloud_databases
{
    public class QueueTriqqers
    {
        private readonly IMortgageService _mortgageService;
        private readonly IMailService _mailService;

        public QueueTriqqers(IMortgageService mortgageService, IMailService mailService)
        {
            _mortgageService = mortgageService;
            _mailService = mailService;
        }

        [Function("HandleMortgageRequests")]
        public async Task Run([QueueTrigger("request-handle-queue", Connection = "AzureWebJobsStorage")] string myQueueItem,
            FunctionContext context)
        {
            Applicant applicant = JsonConvert.DeserializeObject<Applicant>(myQueueItem);
            Uri uri = await _mortgageService.CreatePDF(applicant);

            MortgageOffer mortgageOffer = new()
            {
                Applicant = applicant,
                PDFUrl = uri,
            };

            await _mailService.SendMail(mortgageOffer);

        }        
    }
}
