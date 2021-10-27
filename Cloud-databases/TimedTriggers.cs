using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Cloud_databases
{
    public class TimedTriggers
    {
        [FunctionName("CreateOffersAtMidnight")]
        public async Task CreateOffersAtMidnight([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }        
    }
}
