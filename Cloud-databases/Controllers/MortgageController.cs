using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Validation;

namespace Cloud_databases.Controllers
{
    public class MortgageController
    {
		private readonly IMortgageService _mortgageService;		
		private readonly ILogger _logger;

        public MortgageController(IMortgageService mortgageService, ILogger<MortgageController> logger)
        {
            _mortgageService = mortgageService;            
            _logger = logger;
        }

        [Function(nameof(MortgageController.NewMortgageRequest))]
		[OpenApiOperation(operationId: "NewMortgageRequest", tags: new[] { "Mortgage" }, Summary = "A request for an mortgae proposal", Description = "Adds a applicant to the mortgage proposal queue and will be handled at midnight ", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Applicant), Summary = "Successful operation", Description = "Successful operation")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Applicant), Required = true, Description = "Applicant that needs to be queued")]
		public async Task<HttpResponseData> NewMortgageRequest([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "mortgage")] HttpRequestData req, FunctionContext executionContext)
		{
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

			Applicant applicant = JsonConvert.DeserializeObject<Applicant>(requestBody);

			var validator = new ApplicantValidator();

			var validationResult = validator.Validate(applicant);

			if (!validationResult.IsValid)
			{
				HttpResponseData error = req.CreateResponse(HttpStatusCode.BadRequest);
				await error.WriteAsJsonAsync(validationResult.Errors.Select(e => new
				{
					Field = e.PropertyName,
					Error = e.ErrorMessage
				}));
				return error;
			}

			await _mortgageService.NewMortgageRequest(applicant);				

			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(applicant);

			return response;
		}		
	}
}
