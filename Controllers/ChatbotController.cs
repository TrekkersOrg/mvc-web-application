using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;
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
    public class ChatbotController : Controller
    {
        private readonly IConfiguration _configuration;
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
            _httpClientFactory = httpClientFactory;
            _domain = _configuration["Hosting:Domain"];
        }

        /// <summary>
        /// Sends a query to chatbot and returns response.
        /// </summary>
        /// <param name="requestBody" type="SendQueryRequestModel"></param>
        /// <returns type="ActionResult"></returns>
        [HttpPost("sendQuery")]
        [EnableCors("AllowAll")]
        async public Task<ActionResult> SendQuery([FromBody] SendQueryRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            SendQueryResponseModel sendQueryResponseModel = new SendQueryResponseModel();
            if (string.IsNullOrEmpty(requestBody.VectorStore) || string.IsNullOrEmpty(requestBody.Query))
            {
                responseModel = createResponseModel(200, "Success", "The 'vectorstore' and/or 'query' field is missing or empty.", DateTime.Now);
                return Ok(responseModel);
            }
            var pineconeDetailsAPIResponse = await GetPineconeDetails();
            var pineconeDetailsAPIJson = JsonConvert.DeserializeObject<APIResponseBodyWrapperModel>(pineconeDetailsAPIResponse);
            var pineconeDetailsResponseString = pineconeDetailsAPIJson?.Data?.ToString();
            if (pineconeDetailsResponseString is not null)
            {
                PineconeDetailsResponseModel? pineconeDetails = JsonConvert.DeserializeObject<PineconeDetailsResponseModel>(pineconeDetailsResponseString);
                if (pineconeDetails?.Namespaces?.ContainsKey(requestBody.VectorStore) == false)
                {
                    responseModel = createResponseModel(200, "Success", "Vector store does not match namespace in Pinecone index.", DateTime.Now);
                    return Ok(responseModel);
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