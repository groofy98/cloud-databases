using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models;
using Newtonsoft.Json;
using Service;
using Validation;

namespace Cloud_databases.Controllers
{
    public class HouseController
    {		
		private readonly IHouseService _houseService;
		private readonly ILogger _logger;

        public HouseController(IHouseService houseService, ILogger<HouseController> logger)
        {            
            _houseService = houseService;
            _logger = logger;
        }        

		[Function(nameof(HouseController.AddHouse))]
		[OpenApiOperation(operationId: "AddHouse", tags: new[] { "House" }, Summary = "Add a new house", Description = "Adds a new house", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "Successful operation", Description = "Successful operation")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(House), Required = true, Description = "House that needs to be queued")]
		public async Task<HttpResponseData> AddHouse([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "houses")] HttpRequestData req, FunctionContext executionContext)
		{
			string requestBody = await req.ReadAsStringAsync();

			House house = JsonConvert.DeserializeObject<House>(requestBody);

			// Validate input
			var validator = new HouseValidator();

			var validationResult = validator.Validate(house);

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

			// Add house to database
			await _houseService.AddHouse(house);

			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(house);

			return response;
		}

		[Function(nameof(HouseController.AddImage))]
		[OpenApiOperation(operationId: "AddImage", tags: new[] { "House" }, Summary = "Add a new image to a house", Description = "Adds a new image to a house, request needs to have a form-data body with the file", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "Successful operation", Description = "Successful operation")]
		[OpenApiParameter(name: "houseId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Summary = "ID of house to add image to", Description = "ID of house to add image to", Visibility = OpenApiVisibilityType.Important)]
		public async Task<HttpResponseData> AddImage([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "houses/{houseId}/images")] HttpRequestData req, FunctionContext executionContext, string houseId)
		{
			FilePart file;

			try
            {
				// Parse image from body
				var parsedFormBody = await MultipartFormDataParser.ParseAsync(req.Body);
				file = parsedFormBody.Files[0];

				// Input validation
				if (!file.ContentType.Contains("jpg") && !file.ContentType.Contains("png"))
				{
					var error = req.CreateResponse(HttpStatusCode.BadRequest);
					await error.WriteAsJsonAsync(new { Error = "Please submit either a png or jpg" });
					return error;
				}
            } catch (Exception e)
            {
				_ = e;
				var error = req.CreateResponse(HttpStatusCode.BadRequest);
				await error.WriteAsJsonAsync(new { Error = "No valid image" });
				return error;
			}

			var Id = Guid.Parse(houseId);

			Image image = await _houseService.AddImageToHouse(Id, file);

			var response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(image);

			return response;
		}

		[Function(nameof(HouseController.GetHouses))]
		[OpenApiOperation(operationId: "GetHouses", tags: new[] { "House" }, Summary = "Gets a list of all houses", Description = "Gets a list of all houses", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<House>), Summary = "Successful operation", Description = "Successful operation")]		
		public async Task<HttpResponseData> GetHouses([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "houses")] HttpRequestData req, FunctionContext executionContext)
		{
			//TODO add filters

			var houses = await _houseService.GetAllHouses();

			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(houses);

			return response;
		}

		[Function(nameof(HouseController.RemoveHouse))]
		[OpenApiOperation(operationId: "RemoveHouse", tags: new[] { "House" }, Summary = "Removes a house by id", Description = "Removes a house by id", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter(name: "houseId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Summary = "ID of house to remove", Description = "ID of house to remove", Visibility = OpenApiVisibilityType.Important)]
		public async Task<HttpResponseData> RemoveHouse([HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "houses/{houseId}")] HttpRequestData req, FunctionContext executionContext, string houseId)
        {
			await _houseService.RemoveHouse(Guid.Parse(houseId));

			var response = req.CreateResponse(HttpStatusCode.OK);

			return response;
        }
	}
}
