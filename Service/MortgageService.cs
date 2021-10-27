using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using DAL;
using HtmlAgilityPack;
using IronPdf;
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
        public Task CreatePDF(Applicant applicant, double mortgage);
    }

    public class MortgageService : IMortgageService
    {
        private readonly CloudDBContext _context;
        private readonly QueueClient _requestQueue;
        private readonly QueueClient _requestHandleQueue;

        public MortgageService()
        {
            //_context = context;
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            /*_requestQueue = new QueueClient(connectionString, "requestQueue");
            _requestQueue.CreateIfNotExists();

            _requestHandleQueue = new QueueClient(connectionString, "requestHandleQueue");
            _requestHandleQueue.CreateIfNotExists();*/
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
            QueueMessage[] messages = _requestQueue.ReceiveMessages();

            foreach(var message in messages)
            {
                await _requestHandleQueue.SendMessageAsync(message.Body);
            }
            
        }

        public double CalculateMortgage(Applicant applicant)
        {
            double result = 0;
            result += applicant.Income * 5;
            result -= applicant.Loans / 5;

            return result;
        }

        public async Task CreatePDF(Applicant applicant, double mortgage)
        {
            var path = @"C:\Users\groof\source\repos\Cloud-databases\Cloud-databases\Resources\offer.html";
            var doc = new HtmlDocument();
            doc.Load(path);            
            doc.GetElementbyId("applicant").InnerHtml = $"{applicant.FirstName} {applicant.LastName}<br />{applicant.Email}";
            doc.GetElementbyId("income").InnerHtml = $"€ {applicant.Income:n2}";            
            doc.GetElementbyId("loan").InnerHtml = $"€ {applicant.Loans:n2}";            
            doc.GetElementbyId("mortgageproposal").InnerHtml = $"Mortgage proposal € {mortgage:n2}";
            
            // turn html to pdf
            var pdf = ChromePdfRenderer.StaticRenderHtmlAsPdf(doc.DocumentNode.InnerHtml);
            // save resulting pdf into file
            using (var fileStream = File.Create(@"C:\Users\groof\source\repos\Cloud-databases\Cloud-databases\Resources\yolo.pdf"))
            {                
                pdf.Stream.Seek(0, SeekOrigin.Begin);
                pdf.Stream.CopyTo(fileStream);
            }

        }
        
        
    }
}
