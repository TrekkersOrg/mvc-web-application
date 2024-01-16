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
        // Set configuration variables for controller
        private readonly IConfiguration _configuration;
        private readonly string _pineconeAPIKey;
        private readonly string _pineconeIndex;
        private readonly string _pineconeEnvironment;
        private readonly string _pineconeHost;

        // Initialize configuration variables from appSettings.Development.json
        public IndexerController(IConfiguration configuration)
        {
            _configuration = configuration;
            _pineconeAPIKey = _configuration["Pinecone:APIKey"];
            _pineconeIndex = _configuration["Pinecone:Index"];
            _pineconeEnvironment = _configuration["Pinecone:Environment"];
            _pineconeHost = _configuration["Pinecone:Host"];
        }


        // Purge Pinecone: Deletes all records in a namespace.
        [HttpPost("purgePinecone")]
        public async Task<ActionResult> PurgePinecone([FromBody] PurgePineconeRequestModel requestBody)
        {
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

                var responseModel = new APIResponseBodyWrapperModel();
                var purgePinecodeResponseModel = new PurgePineconeResponseModel();
                // Check if namespace exists, if else, 404 error
                var response = await httpClient.PostAsync(requestUri, content);


                if (response.IsSuccessStatusCode)
                {
                    responseModel.StatusCode = 200;
                    responseModel.StatusMessage = "Success.";
                    responseModel.StatusMessageText = "Pinecone records deleted successfully.";
                    responseModel.Timestamp = DateTime.Now;
                    purgePinecodeResponseModel.Namespace = requestBody.Namespace;
                    responseModel.Data = purgePinecodeResponseModel;
                    return Ok(responseModel);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Error purging Pinecone records.");
                }
            }
        }

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
                    return StatusCode((int)response.StatusCode, "Error retrieving Pinecone details.");
                }
            }
        }
    }
}