using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _pineconeAPIKey;
        private readonly string _pineconeIndex;
        private readonly string _pineconeEnvironment;
        private readonly string _pineconeHost;

        /// <summary>
        /// Initializes the controller using the configuration from appSettings.Development.json.
        /// Initializes the local instances.
        /// </summary>
        /// <param name="configuration" type="IConfiguration"></param>
        public IndexerController(IConfiguration configuration)
        {
            _configuration = configuration;
            _pineconeAPIKey = _configuration["Pinecone:APIKey"];
            _pineconeIndex = _configuration["Pinecone:Index"];
            _pineconeEnvironment = _configuration["Pinecone:Environment"];
            _pineconeHost = _configuration["Pinecone:Host"];
        }

        /// <summary>
        /// Deletes all vectors in a namespace of the Pinecone index.
        /// </summary>
        /// <param name="requestBody" type="PurgePineconeRequestModel"></param>
        /// <returns type="Task<ActionResult>"></returns>
        [HttpPost("purgePinecone")]
        public async Task<ActionResult> PurgePinecone([FromBody] PurgePineconeRequestModel requestBody)
        {
            var responseModel = new APIResponseBodyWrapperModel();
            var purgePinecodeResponseModel = new PurgePineconeResponseModel();
            if (requestBody.Namespace == null)
            {
                responseModel.StatusCode = 400;
                responseModel.StatusMessage = "Bad Request";
                responseModel.StatusMessageText = "The 'namespace' field is missing in the request body.";
                responseModel.Timestamp = DateTime.Now;
                return BadRequest(responseModel);
            }
            ActionResult? pineconeDetailsResult = await PineconeDetails();
            List<string> existingNamespaces = new List<string>();
            int namespaceVectorCount = 0;
            if ((pineconeDetailsResult is OkObjectResult okResult) && (okResult.Value != null) && (okResult.Value is APIResponseBodyWrapperModel apiDetails) && (apiDetails != null) && (apiDetails.Data is PineconeDetailsResponseModel pineconeDetails))
            {
                Dictionary<string, PineconeDetailsResponseModel.NamespaceModel>? namespaces = pineconeDetails.Namespaces;
                if (namespaces != null)
                {
                    List<string> namespaceKeys = new List<string>(namespaces.Keys);
                    foreach (var key in namespaceKeys)
                    {
                        existingNamespaces.Add(key);
                        if (key == requestBody.Namespace)
                        {
                            PineconeDetailsResponseModel.NamespaceModel? vectorCountModel = pineconeDetails?.Namespaces?.GetValueOrDefault(key);
                            if (vectorCountModel != null)
                            {
                                namespaceVectorCount = vectorCountModel.VectorCount;
                            }
                        }
                    }
                } 
            }
            using (var httpClient = new HttpClient())
            {
                var requestUri = _pineconeHost + "/vectors/delete";
                var content = new StringContent(
                    requestBody.ToJson(),
                    Encoding.UTF8,
                    "application/json"
                );
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Api-Key", _pineconeAPIKey);
                if (requestBody.Namespace != null && !existingNamespaces.Contains(requestBody.Namespace))
                {
                    responseModel.StatusCode = 404;
                    responseModel.StatusMessage = "Not Found";
                    responseModel.StatusMessageText = "Unable to find index: " + requestBody.Namespace;
                    responseModel.Timestamp = DateTime.Now;
                    return NotFound(responseModel);
                }
                var response = await httpClient.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    responseModel.StatusCode = 200;
                    responseModel.StatusMessage = "OK";
                    responseModel.StatusMessageText = "Pinecone records deleted successfully.";
                    responseModel.Timestamp = DateTime.Now;
                    purgePinecodeResponseModel.Namespace = requestBody.Namespace;
                    purgePinecodeResponseModel.NumberOfVectorsDeleted = namespaceVectorCount;
                    responseModel.Data = purgePinecodeResponseModel;
                    return Ok(responseModel);
                }
                else
                {
                    responseModel.StatusCode = (int)response.StatusCode;
                    responseModel.StatusMessage = "Unexpected Error";
                    responseModel.StatusMessageText = "An unexpected error occurred, please refer to status code.";
                    responseModel.Timestamp = DateTime.Now;
                    return StatusCode((int)response.StatusCode, responseModel);
                }
            }
        }

        /// <summary>
        /// Retrieves the index fullness, dimension, total vector count, and the vector count of each
        /// namespace in the Pinecone index.
        /// </summary>
        /// <returns type="Task<ActionResult>"></returns>
        [HttpPost("pineconeDetails")]
        public async Task<ActionResult> PineconeDetails()
        {
            using (var httpClient = new HttpClient())
            {
                var requestUri = _pineconeHost + "/describe_index_stats";
                httpClient.DefaultRequestHeaders.Add("Api-Key", _pineconeAPIKey);
                var responseModel = new APIResponseBodyWrapperModel();
                var response = await httpClient.PostAsync(requestUri, null);
                if (response.IsSuccessStatusCode)
                {
                    var responseBodyRaw = await response.Content.ReadAsStringAsync();
                    JsonDocument responseBodyJson = JsonDocument.Parse(responseBodyRaw);
                    JsonElement responseBodyElement = responseBodyJson.RootElement;
                    var namespacesDictionary = new Dictionary<string, PineconeDetailsResponseModel.NamespaceModel>();
                    foreach (var namespaceProperty in responseBodyElement.GetProperty("namespaces").EnumerateObject())
                    {
                        var namespaceModel = new PineconeDetailsResponseModel.NamespaceModel
                        {
                            VectorCount = namespaceProperty.Value.GetProperty("vectorCount").GetInt32()
                        };
                        namespacesDictionary.Add(namespaceProperty.Name, namespaceModel);
                    }
                    PineconeDetailsResponseModel pineconeDetailsResponseModel = new PineconeDetailsResponseModel
                    {
                        Namespaces = namespacesDictionary,
                        Dimension = responseBodyElement.GetProperty("dimension").GetInt32(),
                        IndexFullness = responseBodyElement.GetProperty("indexFullness").GetDouble(),
                        TotalVectorCount = responseBodyElement.GetProperty("totalVectorCount").GetInt32()
                    };
                    responseModel.StatusCode = 200;
                    responseModel.StatusMessage = "Success.";
                    responseModel.StatusMessageText = "Pinecone details retrieved successfully.";
                    responseModel.Timestamp = DateTime.Now;
                    responseModel.Data = pineconeDetailsResponseModel;
                    return Ok(responseModel);
                }
                else
                {
                    responseModel.StatusCode = (int)response.StatusCode;
                    responseModel.StatusMessage = "Unexpected Error";
                    responseModel.StatusMessageText = "An unexpected error occurred, please refer to status code.";
                    responseModel.Timestamp = DateTime.Now;
                    return StatusCode((int)response.StatusCode, responseModel);
                }
            }
        }
    }
}