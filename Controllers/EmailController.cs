using Microsoft.AspNetCore.Mvc;
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

        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridAPIKey = _configuration["SendGrid:APIKey"];
        }

        [HttpPost("sendEmail")]
        public async Task<ActionResult> SendEmail(SendEmailRequestModel requestBody)
        {
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
                    return Ok("Email sent successfully.");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to send email.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending the email: {ex.Message}");
            }
        }
    }
}
