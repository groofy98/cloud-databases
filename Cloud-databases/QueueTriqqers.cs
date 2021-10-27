using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Service;

namespace Cloud_databases
{
    public class QueueTriqqers
    {
        private readonly IMortgageService _service;

        [Function("HandleMortgageRequests")]
        public async Task Run([QueueTrigger("requestHandleQueue", Connection = "AzureWebJobsStorage")] string myQueueItem,
            FunctionContext context)
        {
            
        }
    }
}
