using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _pineconeAPIKey;
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
            _pineconeHost = _configuration["Pinecone:Host"];
        }

        /// <summary>
        /// Retrieves a record by id from Pinecone namespace.
        /// </summary>
        /// <param name="requestBody" type="GetRecordRequestModel"></param>
        /// <returns type="Task<ActionResult>"></returns>
        [HttpGet("getRecord")]
        public async Task<ActionResult> GetRecord([FromBody] GetRecordRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            GetRecordResponseModel? getRecordResponseModel = new GetRecordResponseModel();
            if (requestBody.Namespace == null || requestBody.Ids == null)
            {
                responseModel.StatusCode = 400;
                responseModel.StatusMessage = "Bad Request";
                responseModel.StatusMessageText = "The 'namespace' and/or 'ids' field is missing in the request body.";
                responseModel.Timestamp = DateTime.Now;
                return BadRequest(responseModel);
            }
            ActionResult? pineconeDetailsResult = await PineconeDetails();
            List<string> existingNamespaces = new List<string>();
            if ((pineconeDetailsResult is OkObjectResult okResult) && (okResult.Value != null) && (okResult.Value is APIResponseBodyWrapperModel apiDetails) && (apiDetails != null) && (apiDetails.Data is PineconeDetailsResponseModel pineconeDetails))
            {
                Dictionary<string, PineconeDetailsResponseModel.NamespaceModel>? namespaces = pineconeDetails.Namespaces;
                if (namespaces != null)
                {
                    List<string> namespaceKeys = new List<string>(namespaces.Keys);
                    foreach (var key in namespaceKeys)
                    {
                        existingNamespaces.Add(key);
                    }
                }
            }
            List<string> idList = requestBody.Ids;
            string idQueryString = BuildQueryString("ids", idList);
            using (HttpClient httpClient = new HttpClient())
            {
                var requestUri = _pineconeHost + "/vectors/fetch?namespace=" + requestBody.Namespace + '&' + idQueryString;
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
                var response = await httpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var responseBodyString = await response.Content.ReadAsStringAsync();
                    JsonDocument responseBodyJson = JsonDocument.Parse(responseBodyString);
                    JsonElement responseBodyElement = responseBodyJson.RootElement;
                    getRecordResponseModel.Namespace = requestBody.Namespace;
                    foreach (var property in responseBodyElement.EnumerateObject())
                    {
                        if (property.Name == "namespace")
                        {
                            getRecordResponseModel.Namespace = property.Value.GetString();
                        }
                        if (property.Name == "usage")
                        {
                            GetRecordResponseModel.UsageDetails usage = new GetRecordResponseModel.UsageDetails();
                            foreach (var usageProperty in property.Value.EnumerateObject())
                            {
                                if (usageProperty.Name == "readUnits")
                                {
                                    usage.ReadUnits = usageProperty.Value.GetInt32();
                                }
                            }
                            getRecordResponseModel.Usage = usage;
                        }
                        if (property.Name == "vectors")
                        {
                            GetRecordResponseModel.VectorsDetails vectorsDetails= new GetRecordResponseModel.VectorsDetails();
                            vectorsDetails.Vectors = new Dictionary<string, GetRecordResponseModel.VectorDetails>();
                            foreach (var vectorsDetailsProperty in property.Value.EnumerateObject())
                            {
                                GetRecordResponseModel.VectorDetails vectorDetails = new GetRecordResponseModel.VectorDetails();
                                foreach (var vectorDetailsProperty in vectorsDetailsProperty.Value.EnumerateObject())
                                {
                                    if (vectorDetailsProperty.Name == "id")
                                    {
                                        vectorDetails.Id = vectorDetailsProperty.Value.GetString();
                                    }
                                    if (vectorDetailsProperty.Name == "values")
                                    {
                                        vectorDetails.Values = new List<double>();
                                        foreach (var value in vectorDetailsProperty.Value.EnumerateArray())
                                        {
                                            vectorDetails.Values.Add(value.GetDouble());
                                        }
                                    }
                                }
                                vectorsDetails.Vectors.Add(vectorsDetailsProperty.Name, vectorDetails);
                            }
                            if (vectorsDetails.Vectors.Count == 0)
                            {
                                responseModel.StatusCode = 404;
                                responseModel.StatusMessage = "Not Found";
                                responseModel.StatusMessageText = "Unable to find vectors: " + requestBody.Ids;
                                responseModel.Timestamp = DateTime.Now;
                                return NotFound(responseModel);
                            }
                            getRecordResponseModel.Vectors = vectorsDetails;
                        }
                    }
                    responseModel.StatusCode = 200;
                    responseModel.StatusMessage = "OK";
                    responseModel.StatusMessageText = "Pinecone records fetched successfully.";
                    responseModel.Timestamp = DateTime.Now;
                    responseModel.Data = getRecordResponseModel;
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
                    var responseBodyString = await response.Content.ReadAsStringAsync();
                    JsonDocument responseBodyJson = JsonDocument.Parse(responseBodyString);
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

        /// <summary>
        /// Embeds and inserts a document into namespace.
        /// </summary>
        /// <param name="requestBody" type="InsertDocumentRequestModel"></param>
        /// <returns type="ActionResult"></returns>
        [HttpPost("insertDocument")]
        public ActionResult InsertDocument([FromBody] InsertDocumentRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            try
            {
                string arguments = $"-n {requestBody.Namespace}";
                if (Directory.GetCurrentDirectory() != "scripts")
                {
                    Directory.SetCurrentDirectory("scripts");
                }
                string finalOutput = RunCommand("py", $"embedder.py {arguments}");
                
                if (finalOutput.Contains("Finished"))
                {
                    responseModel.StatusCode = 200;
                    responseModel.StatusMessage = "OK";
                    responseModel.StatusMessageText = $"Document successfully inserted into {requestBody.Namespace} namespace.";
                    responseModel.Timestamp = DateTime.Now;
                    return Ok(responseModel);
                }
                else
                {
                    responseModel.StatusCode = 500;
                    responseModel.StatusMessageText = "Error executing script.";
                    responseModel.StatusMessage = "Internal Server Error.";
                    responseModel.Timestamp = DateTime.Now;
                    return StatusCode(500, responseModel);
                }             
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = 500;
                responseModel.StatusMessageText = ex.Message;
                responseModel.StatusMessage = "Internal Server Error.";
                responseModel.Timestamp= DateTime.Now;
                return StatusCode(500, responseModel);
            }
            
        }

        /// <summary>
        /// Creates a query parameter request URI string that assigns
        /// each query parameter in the list the same key.
        /// </summary>
        /// <param name="key" type="string"></param>
        /// <param name="values" type="List<string>"></param>
        /// <returns></returns>
        static string BuildQueryString(string key, List<string> values)
        {
            var encodedValues = values.Select(v => HttpUtility.UrlEncode(v));
            return $"{key}={string.Join("&" + key + "=", encodedValues)}";
        }

        /// <summary>
        /// Runs and processes a system command.
        /// </summary>
        /// <param name="command" type="string"></param>
        /// <param name="arguments" type="string"></param>
        /// <returns type="string"></returns>
        static string RunCommand(string command, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        return output.Trim();
                    }
                    else
                    {
                        return $"{error}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }
    }
}