using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using SendGrid;
using SendGrid.Helpers.Mail;
using StriveAI.Models;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _sendGridAPIKey;

        /// <summary>
        /// Initializes the controller using the configuration from appSettings.Development.json.
        /// Initializes the local instances.
        /// </summary>
        /// <param name="configuration" type="IConfiguration"></param>
        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridAPIKey = _configuration["SendGrid:APIKey"];
        }

        /// <summary>
        /// Sends an email to a destination using the configuration in the request body. 
        /// Leverages SendGrid's SMTP server.
        /// </summary>
        /// <param name="requestBody" type="SendEmailRequestModel"></param>
        /// <returns type="Task<ActionResult>"></returns>
        [HttpPost("sendEmail")]
        [EnableCors("AllowAll")]
        public async Task<ActionResult> SendEmail(SendEmailRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseModel;
            SendEmailResponseModel sendEmailResponseModel = new();
            try
            {
                var client = new SendGridClient(_sendGridAPIKey);
                var emailMessage = new SendGridMessage
                {
                    From = new EmailAddress(requestBody.FromEmail, requestBody.FromName),
                    Subject = requestBody.Subject,
                    PlainTextContent = requestBody.Body,
                    HtmlContent = requestBody.Body
                };
                emailMessage.AddTo(new EmailAddress(requestBody.ToEmail, requestBody.ToName));
                var response = await client.SendEmailAsync(emailMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    sendEmailResponseModel.FromEmail = requestBody.FromEmail;
                    sendEmailResponseModel.FromName = requestBody.FromName;
                    sendEmailResponseModel.Subject = requestBody.Subject;
                    sendEmailResponseModel.Body = requestBody.Body;
                    sendEmailResponseModel.ToEmail = requestBody.ToEmail;
                    sendEmailResponseModel.ToName = requestBody.ToName;
                    responseModel = createResponseModel(200, "Success", "Email sent successfully.", DateTime.Now, sendEmailResponseModel);
                    return Ok(responseModel);
                }
                else
                {
                    responseModel = createResponseModel(200, "Success", "Email failed to send", DateTime.Now, null);
                    return Ok(responseModel);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending the email: {ex.Message}");
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
            APIResponseBodyWrapperModel responseModel = new();
            responseModel.StatusCode = statusCode;
            responseModel.StatusMessage = statusMessage;
            responseModel.StatusMessageText = statusMessageText;
            responseModel.Timestamp = timestamp;
            if (data is SendEmailResponseModel)
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
