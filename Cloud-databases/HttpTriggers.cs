using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
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
		[OpenApiOperation(operationId: "NewMortgageRequest", tags: new[] { "Mortgage" }, Summary = "A request for an mortgae proposal", Description = "Adds a applicant to the mortgage proposal queue and will be handled at midnight ", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Applicant), Summary = "Successful operation", Description = "Successful operation")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Applicant), Required = true, Description = "Applicant that needs to be queued")]
		public async Task<HttpResponseData> NewMortgageRequest([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "mortgage")] HttpRequestData req, FunctionContext executionContext)
		{
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

			Applicant applicant = JsonConvert.DeserializeObject<Applicant>(requestBody);

			await _service.NewMortgageRequest(applicant);					

			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(applicant);

			return response;
		}
	}
}
