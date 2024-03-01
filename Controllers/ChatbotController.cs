using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _pineconeAPIKey;
        private readonly string _pineconeHost;
        private readonly string _domain;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes the controller using the configuration from appSettings.Development.json.
        /// Initializes the local instances.
        /// </summary>
        /// <param name="configuration" type="IConfiguration"></param>
        public ChatbotController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _pineconeAPIKey = _configuration["Pinecone:APIKey"];
            _pineconeHost = _configuration["Pinecone:Host"];
            _httpClientFactory = httpClientFactory;
            _domain = _configuration["Hosting:Domain"];
        }

        /// <summary>
        /// Sends a query to chatbot and returns response.
        /// </summary>
        /// <param name="requestBody" type="SendQueryRequestModel"></param>
        /// <returns type="ActionResult"></returns>
        [HttpPost("sendQuery")]
        public ActionResult SendQuery([FromBody] SendQueryRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            SendQueryResponseModel sendQueryResponseModel = new SendQueryResponseModel();
            if (requestBody.VectorStore == null || requestBody.VectorStore == "" || requestBody.Query == null || requestBody.Query == "")
            {
                responseModel = createResponseModel(400, "Bad Request", "The 'vectorstore' and/or 'query' field is missing or empty.", DateTime.Now);
                return BadRequest(responseModel);
            }
            var pineconeDetailsAPIResponse = GetPineconeDetails().Result;
            var pineconeDetailsAPI = JsonConvert.DeserializeObject<APIResponseBodyWrapperModel>(pineconeDetailsAPIResponse);
            var pineconeDetailsResponse = pineconeDetailsAPI?.Data?.ToString();
            if (pineconeDetailsResponse is not null)
            {
                PineconeDetailsResponseModel? pineconeDetails = JsonConvert.DeserializeObject<PineconeDetailsResponseModel>(pineconeDetailsResponse);
                if (pineconeDetails?.Namespaces?.ContainsKey(requestBody.VectorStore) == false)
                {
                    responseModel = createResponseModel(404, "Not Found", "Vector store does not match namespace in Pinecone index.", DateTime.Now);
                    return NotFound(responseModel);
                }
            }
            string arguments = $"-v {requestBody.VectorStore} -q \"{requestBody.Query}\"";
            try
            {
                if (!Directory.GetCurrentDirectory().Contains("scripts", StringComparison.OrdinalIgnoreCase))
                {
                    Directory.SetCurrentDirectory("scripts");
                }
                string finalOutput = RunCommand("py", $"llm.py {arguments}");
                sendQueryResponseModel.Response = finalOutput;
                responseModel = createResponseModel(200, "Success", "Response generated successfully.", DateTime.Now, sendQueryResponseModel);
                return Ok(responseModel);
            }
            catch (Exception ex)
            {
                responseModel = createResponseModel(500, "Internal Server Error", ex.Message, DateTime.Now);
                return StatusCode(500, responseModel);
            }
        }

        /// <summary>
        /// Runs and processes a system command and output.
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
                        output = output.Replace("\r\n", "").Trim();
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
        static APIResponseBodyWrapperModel createResponseModel(int statusCode, string statusMessage, string statusMessageText, DateTime timestamp, object? data = null)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            responseModel.StatusCode = statusCode;
            responseModel.StatusMessage = statusMessage;
            responseModel.StatusMessageText = statusMessageText;
            responseModel.Timestamp = timestamp;
            if (data is SendQueryResponseModel)
            {
                responseModel.Data = data;
            }
            else
            {
                responseModel.Data = "";
            }
            return responseModel;
        }

        /// <summary>
        /// Calls /GetPineconeDetails endpoint and returns response.
        /// </summary>
        /// <returns type="Task<string>"></returns>
        private async Task<string> GetPineconeDetails()
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var apiUrl = _domain + "/api/indexer/PineconeDetails";
                var response = await client.PostAsync(apiUrl, null);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}