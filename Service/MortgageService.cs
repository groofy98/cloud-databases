using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using DAL;
using Helpers;
using HtmlAgilityPack;
using IronPdf;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Service
{
    public interface IMortgageService
    {
        public Task NewMortgageRequest(Applicant applicant);
        public Task HandleMortgageRequest();
        public Task<Uri> CreatePDF(Applicant applicant);
    }

    public class MortgageService : IMortgageService
    {
        private readonly CloudDBContext _context;
        private readonly QueueClient _requestQueue;
        private readonly QueueClient _requestHandleQueue;
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger _logger;

        public MortgageService(CloudDBContext cloudDBContext, ILogger<MortgageService> logger)
        {
            _context = cloudDBContext;

            _context.Database.EnsureCreated();

            _logger = logger;

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Get client for queue that handels new mortgage offers request
            _requestQueue = new QueueClient(connectionString, "request-queue", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            _requestQueue.CreateIfNotExists();
            
            // The queue that handles the requests
            _requestHandleQueue = new QueueClient(connectionString, "request-handle-queue", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            _requestHandleQueue.CreateIfNotExists();

            // Container client where the mortgage offer pdf's are stored
            _containerClient = new BlobContainerClient(connectionString, "mortgages");
            _containerClient.CreateIfNotExists();
        }

        public async Task NewMortgageRequest(Applicant applicant)
        {
            // Add request to database for future reference
            await _context.Applicants.AddAsync(applicant);
            await _context.SaveChangesAsync();

            await _requestQueue.SendMessageAsync(JsonConvert.SerializeObject(applicant));
        }

        // Copy messages to the handle cueue so they can be handled ayncronous.
        public async Task HandleMortgageRequest()
        {
            int count = 0;

            while(true)
            {
                QueueMessage message = await _requestQueue.ReceiveMessageAsync();

                if (message == null)
                    break;

                await _requestHandleQueue.SendMessageAsync(message.Body);
                count++;
            }
            _logger.LogInformation($"Moved {count} messages to handle queue at {DateTime.Now}");
        }

        public double CalculateMortgage(Applicant applicant)
        {
            double result = 0;
            result += applicant.Income * 5;
            result -= applicant.Loans / 5;

            return result;
        }

        public async Task<Uri> CreatePDF(Applicant applicant)
        {
            var html = Resources.ResourceManager.GetString("offer");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            doc.GetElementbyId("dateCreated").InnerHtml = $"Created: {DateTime.Now.ToString("MMMM dd, yyyy")}";
            doc.GetElementbyId("applicant").InnerHtml = $"{applicant.FirstName} {applicant.LastName}<br />{applicant.Email}";
            doc.GetElementbyId("income").InnerHtml = $"€ {applicant.Income:n2}";            
            doc.GetElementbyId("loan").InnerHtml = $"€ {applicant.Loans:n2}";            
            doc.GetElementbyId("mortgageproposal").InnerHtml = $"Mortgage proposal € {CalculateMortgage(applicant):n2}";
            
            // turn html to pdf
            var pdf = ChromePdfRenderer.StaticRenderHtmlAsPdf(doc.DocumentNode.InnerHtml);
            
            // save resulting pdf into file
            pdf.Stream.Seek(0, SeekOrigin.Begin);
            var response = await _containerClient.UploadBlobAsync(applicant.Id.ToString() + ".pdf", pdf.Stream);
            var blobClient = _containerClient.GetBlobClient(applicant.Id.ToString() + ".pdf");

            return SasHelper.GetServiceSasUriForBlob(blobClient);
            
        }
        
        
    }
}
