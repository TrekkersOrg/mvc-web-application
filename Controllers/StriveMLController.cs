using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using System.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StriveMLController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _domain;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes the controller using the configuration from appSettings.Development.json.
        /// Initializes the local instances.
        /// </summary>
        /// <param name="configuration" type="IConfiguration"></param>
        public StriveMLController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _domain = _configuration["Hosting:Domain"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://strive-ml-api.azurewebsites.net/");

        }

        [HttpPost("chatbot")]
        async public Task<ActionResult> Chatbot([FromBody] StriveML_Chatbot_Request request)
        {
            APIResponseBodyWrapperModel response = new();
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            try
            {
                var httpResponse = await _httpClient.PostAsync("chatbot", httpContent);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();
                    return Ok(httpResponseContent);
                }
                else
                {
                    response = createResponseModel(200, "Success", "Error generating response.", DateTime.Now, "");
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                response = createResponseModel(500, "Internal Server Error", ex.Message, DateTime.Now, "");
                return StatusCode(500, response);
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
            if (data is object)
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