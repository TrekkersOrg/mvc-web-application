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
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes the controller using the configuration from appSettings.Development.json.
        /// Initializes the local instances.
        /// </summary>
        /// <param name="configuration" type="IConfiguration"></param>
        public StriveMLController()
        {
            _httpClient = new()
            {
                BaseAddress = new Uri("https://strive-core.azurewebsites.net/")
            };

        }

        [HttpPost("chatbot")]
        async public Task<ActionResult> Chatbot([FromBody] StriveML_Chatbot_Request request)
        {
            APIResponseBodyWrapperModel response;
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            try
            {
                var httpResponse = await _httpClient.PostAsync("chat", httpContent);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();
                    return Ok(httpResponseContent);
                }
                else
                {
                    response = CreateResponseModel(200, "Success", "Error generating response.", DateTime.Now, "");
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                response = CreateResponseModel(500, "Internal Server Error", ex.Message, DateTime.Now, "");
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
        static APIResponseBodyWrapperModel CreateResponseModel(int statusCode, string statusMessage, string statusMessageText, DateTime timestamp, object? data = null)
        {
            APIResponseBodyWrapperModel responseModel = new()
            {
                StatusCode = statusCode,
                StatusMessage = statusMessage,
                StatusMessageText = statusMessageText,
                Timestamp = timestamp
            };
            if (data is not null)
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