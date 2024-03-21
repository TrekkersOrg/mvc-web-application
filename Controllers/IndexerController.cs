using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

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
        [EnableCors("AllowAll")]
        public async Task<ActionResult> GetRecord([FromBody] GetRecordRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new();
            GetRecordResponseModel? getRecordResponseModel = new();
            const int maximumRequestLimit = 100;
            if (requestBody.Namespace == null || requestBody.Ids == null || requestBody.Ids.Any(string.IsNullOrEmpty) == true || requestBody.Ids.Any() == false)
            {
                responseModel = createResponseModel(200, "Success", "The 'namespace' and/or 'ids' field is missing or empty in the request body.", DateTime.Now);
                return Ok(responseModel);
            }
            if (requestBody.Ids.Count > maximumRequestLimit)
            {
                responseModel = createResponseModel(200, "Success", "The number of IDs entered exceeded the limit of 100.", DateTime.Now);
            }
            ActionResult? pineconeDetailsResult = await PineconeDetails();
            List<string> existingNamespaces = new();
            if ((pineconeDetailsResult is OkObjectResult okResult) && (okResult.Value != null) && (okResult.Value is APIResponseBodyWrapperModel apiDetails) && (apiDetails != null) && (apiDetails.Data is PineconeDetailsResponseModel pineconeDetails))
            {
                Dictionary<string, PineconeDetailsResponseModel.NamespaceModel>? namespaces = pineconeDetails.Namespaces;
                if (namespaces != null)
                {
                    List<string> namespaceKeys = new(namespaces.Keys);
                    foreach (var key in namespaceKeys)
                    {
                        existingNamespaces.Add(key);
                    }
                }
            }
            List<string> idList = requestBody.Ids;
            string idQueryString = BuildQueryString("ids", idList);
            using (HttpClient httpClient = new())
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
                    responseModel = createResponseModel(200, "Success", "Unable to find index: " + string.Join(", ", requestBody.Namespace), DateTime.Now);
                    return Ok(responseModel);
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
                            GetRecordResponseModel.UsageDetails usage = new();
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
                            GetRecordResponseModel.VectorsDetails vectorsDetails = new();
                            vectorsDetails.Vectors = new Dictionary<string, GetRecordResponseModel.VectorDetails>();
                            foreach (var vectorsDetailsProperty in property.Value.EnumerateObject())
                            {
                                GetRecordResponseModel.VectorDetails vectorDetails = new();
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
                                responseModel = createResponseModel(200, "Success", "Unable to find vectors: " + string.Join(", ", requestBody.Ids), DateTime.Now);
                                return Ok(responseModel);
                            }
                            List<String> invalidVectors = new();
                            if (vectorsDetails.Vectors.Count > 0 && vectorsDetails.Vectors.Count < idList.Count)
                            {
                                foreach (string vector in idList)
                                {
                                    if (vectorsDetails.Vectors.ContainsKey(vector) == false)
                                    {
                                        invalidVectors.Add(vector);
                                    }
                                }
                                if (invalidVectors.Count > 0)
                                {
                                    getRecordResponseModel.Vectors = vectorsDetails;
                                    responseModel = createResponseModel(200, "OK", "Some Pinecone records fetched successfully. The following records were not found: " + string.Join(", ", invalidVectors), DateTime.Now, getRecordResponseModel);
                                    return Ok(responseModel);
                                }
                            }
                            getRecordResponseModel.Vectors = vectorsDetails;
                        }
                    }
                    responseModel = createResponseModel(200, "OK", "Pinecone records fetched successfully.", DateTime.Now, getRecordResponseModel);
                    return Ok(responseModel);
                }
                else
                {
                    responseModel = createResponseModel((int)response.StatusCode, "Unexpected Error", "An unexpected error occurred, please refer to status code.", DateTime.Now);
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
        [EnableCors("AllowAll")]
        public async Task<ActionResult> PurgePinecone([FromBody] PurgePineconeRequestModel requestBody)
        {
            var responseModel = new APIResponseBodyWrapperModel();
            var purgePinecodeResponseModel = new PurgePineconeResponseModel();
            if (string.IsNullOrEmpty(requestBody.Namespace))
            {
                responseModel = createResponseModel(200, "Success", "The 'namespace' field is empty in the request body.", DateTime.Now);
                return Ok(responseModel);
            }
            ActionResult? pineconeDetailsResult = await PineconeDetails();
            List<string> existingNamespaces = new();
            int namespaceVectorCount = 0;
            if ((pineconeDetailsResult is OkObjectResult okResult) && (okResult.Value != null) && (okResult.Value is APIResponseBodyWrapperModel apiDetails) && (apiDetails != null) && (apiDetails.Data is PineconeDetailsResponseModel pineconeDetails))
            {
                Dictionary<string, PineconeDetailsResponseModel.NamespaceModel>? namespaces = pineconeDetails.Namespaces;
                if (namespaces != null)
                {
                    List<string> namespaceKeys = new(namespaces.Keys);
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
                    responseModel = createResponseModel(200, "Success", "Unable to find index: " + requestBody.Namespace, DateTime.Now);
                    return Ok(responseModel);
                }
                var response = await httpClient.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    purgePinecodeResponseModel.Namespace = requestBody.Namespace;
                    purgePinecodeResponseModel.NumberOfVectorsDeleted = namespaceVectorCount;
                    responseModel = createResponseModel(200, "OK", "Pinecone records deleted successfully.", DateTime.Now, purgePinecodeResponseModel);
                    return Ok(responseModel);
                }
                else
                {
                    responseModel = createResponseModel((int)response.StatusCode, "Unexpected Error", "An unexpected error occurred, please refer to status code.", DateTime.Now);
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
        [EnableCors("AllowAll")]
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
                    PineconeDetailsResponseModel pineconeDetailsResponseModel = new()
                    {
                        Namespaces = namespacesDictionary,
                        Dimension = responseBodyElement.GetProperty("dimension").GetInt32(),
                        IndexFullness = responseBodyElement.GetProperty("indexFullness").GetDouble(),
                        TotalVectorCount = responseBodyElement.GetProperty("totalVectorCount").GetInt32()
                    };
                    responseModel = createResponseModel(200, "Success", "Pinecone details retrieved successfully.", DateTime.Now, pineconeDetailsResponseModel);
                    return Ok(responseModel);
                }
                else
                {
                    responseModel = createResponseModel((int)response.StatusCode, "Unexpected Error", "An unexpected error occurred, please refer to status code.", DateTime.Now);
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
        [EnableCors("AllowAll")]
        public ActionResult InsertDocument([FromBody] InsertDocumentRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new();
            try
            {
                if (requestBody.Namespace == null || requestBody.Namespace == "" || requestBody.FileName == null || requestBody.FileName == "")
                {
                    responseModel = createResponseModel(200, "Success", "The 'namespace' and/or 'filename' field is missing or empty.", DateTime.Now);
                    return Ok(responseModel);
                }
                if (requestBody.FileName.Contains(" "))
                {
                    responseModel = createResponseModel(200, "Success", "The file name must not contain spaces.", DateTime.Now);
                    return Ok(responseModel);
                }
                string arguments = $"-n {requestBody.Namespace} -f {requestBody.FileName}";
                if (!Directory.GetCurrentDirectory().Contains("scripts", StringComparison.OrdinalIgnoreCase))
                {
                    Directory.SetCurrentDirectory("scripts");
                }
                string finalOutput = RunCommand("py", $"embedder.py {arguments}");

                if (finalOutput.Contains("Finished"))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                    if (rootDirectory != null)
                    {
                        Directory.SetCurrentDirectory(rootDirectory);

                    }
                    responseModel = createResponseModel(200, "OK", $"Document successfully inserted into {requestBody.Namespace} namespace.", DateTime.Now);
                    return Ok(responseModel);
                }
                else if (finalOutput.Contains("File type not supported."))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                    if (rootDirectory != null)
                    {
                        Directory.SetCurrentDirectory(rootDirectory);

                    }
                    responseModel = createResponseModel(200, "Success", $"File type for file {requestBody.FileName} is not supported.", DateTime.Now);
                    return Ok(responseModel);
                }
                else if (finalOutput.Contains("File name must include file type."))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                    if (rootDirectory != null)
                    {
                        Directory.SetCurrentDirectory(rootDirectory);

                    }
                    responseModel = createResponseModel(200, "Success", $"File type must be included for file {requestBody.FileName}.", DateTime.Now);
                    return BadRequest(responseModel);
                }
                else if (finalOutput.Contains("File does not exist."))
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                    if (rootDirectory != null)
                    {
                        Directory.SetCurrentDirectory(rootDirectory);

                    }
                    responseModel = createResponseModel(200, "Success", $"File {requestBody.FileName} does not exist.", DateTime.Now);
                    return Ok(responseModel);
                }
                else
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                    if (rootDirectory != null)
                    {
                        Directory.SetCurrentDirectory(rootDirectory);

                    }
                    responseModel = createResponseModel(500, "Internal Server Error", "Error executing script", DateTime.Now);
                    return StatusCode(500, responseModel);
                }
            }
            catch (Exception ex)
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string? rootDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (rootDirectory != null)
                {
                    Directory.SetCurrentDirectory(rootDirectory);

                }
                responseModel = createResponseModel(500, "Internal Server Error", ex.Message, DateTime.Now);
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
        private static string BuildQueryString(string key, List<string> values)
        {
            var encodedValues = values.Select(v => HttpUtility.UrlEncode(v));
            return $"{key}={string.Join("&" + key + "=", encodedValues)}";
        }

        /// <summary>
        /// Runs and processes a system command and output.
        /// </summary>
        /// <param name="command" type="string"></param>
        /// <param name="arguments" type="string"></param>
        /// <returns type="string"></returns>
        private static string RunCommand(string command, string arguments)
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

        /// <summary>
        /// Initializes response body with APIResponseBodyWrapperModel
        /// type.
        /// </summary>
        /// <param name="statusCode" type="int"></param>
        /// <param name="statusMessage" type="string"></param>
        /// <param name="statusMessageText" type="string"></param>
        /// <param name="timestamp" type="DateTime"></param>
        /// <param name="data" type="Object"></param>
        /// <returns type="APIResponseBodyWrapperModel"></returns>
        private static APIResponseBodyWrapperModel createResponseModel(int statusCode, string statusMessage, string statusMessageText, DateTime timestamp, object? data = null)
        {
            APIResponseBodyWrapperModel responseModel = new();
            responseModel.StatusCode = statusCode;
            responseModel.StatusMessage = statusMessage;
            responseModel.StatusMessageText = statusMessageText;
            responseModel.Timestamp = timestamp;
            if (data is GetRecordResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is PineconeDetailsResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is PurgePineconeResponseModel)
            {
                responseModel.Data = data;
            }
            else
            {
                responseModel.Data = "";
            }
            return responseModel;
        }
    }
}