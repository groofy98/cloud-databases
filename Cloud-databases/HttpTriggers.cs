using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Models;
using Service;

namespace Cloud_databases
{
    public class HttpTriggers
    {
		private readonly IMortgageService _service;

        public HttpTriggers(IMortgageService service)
        {
            _service = service;
        }

        [Function(nameof(HttpTriggers.NewMortgageRequest))]
		[OpenApiOperation(operationId: "getImages", tags: new[] { "Images" }, Summary = "Request a list of images", Description = "Starts a job to create images", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dictionary<string, object>), Summary = "Successful operation", Description = "Successful operation")]
		public async Task<HttpResponseData> NewMortgageRequest([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "imagesRequest")] HttpRequestData req, FunctionContext executionContext)
		{

			Guid requestId = Guid.NewGuid();

			var result = new Dictionary<string, object>
			{
				{ "imageRequestId", requestId }
			};

			Applicant applicant = new()
			{
				Id = requestId,
				FirstName = "Sjors",
				LastName = "Grooff",
				Income = 30000,
				Loans = 10000,
				Email = "srojs98@gmail.com"
			};

			await _service.CreatePDF(applicant, 300000);			

			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(result);

			return response;
		}
	}
}
