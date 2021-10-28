using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Service;

namespace Cloud_databases
{
    public class TimedTriggers
    {
        private readonly ILogger _logger;
        private readonly IMortgageService _service;

        public TimedTriggers(IMortgageService service, ILogger<TimedTriggers> logger)
        {
            _logger = logger;
            _service = service;
        }

        [Function("CreateOffersAtMidnight")]
        public async Task CreateOffersAtMidnight([TimerTrigger("0 0 * * *")] TimerInfo timerInfo)
        {
            await _service.HandleMortgageRequest();
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
        
    }
}
