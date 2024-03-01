using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;

namespace StriveAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : Controller
    {
        public FileUploadController() { }

        /// <summary>
        /// Uploads file to "UserFiles".
        /// </summary>
        /// <param name="targetFile" type="IFormFile"></param>
        /// <returns type="ActionResult"></returns>
        [HttpPost("upload")]
        public ActionResult Upload(IFormFile targetFile)
        {
            if (targetFile != null && targetFile.Length > 0)
            {
                var formattedFileName = targetFile.FileName.Replace(" ", "_");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", formattedFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    targetFile.CopyTo(fileStream);
                }
                return Ok();
            }
            return RedirectToAction("FileUpload");
        }

        [HttpDelete("delete")]

        public ActionResult Delete([FromBody] DeleteFileRequestModel requestBody)
        {
            var targetFileName = requestBody.FileName;
            if (!string.IsNullOrEmpty(targetFileName))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", targetFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok();
                }
                else
                {
                    return NotFound("File not found");
                }
            }
            return BadRequest("File name is required");
        }

        [HttpPost("getFile")]
        public ActionResult Get([FromBody] GetFileRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseBodyWrapperModel = new APIResponseBodyWrapperModel();
            GetFileResponseModel getFileResponseModel = new GetFileResponseModel();
            var targetFileName = requestBody.FileName;
            string userFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", targetFileName);
            bool fileExists = System.IO.File.Exists(userFilesPath);
            getFileResponseModel.FileName = targetFileName;
            getFileResponseModel.FileExists = fileExists;
            responseBodyWrapperModel = createResponseModel(200, "Success", "File existence checked successfully.", DateTime.Now, getFileResponseModel);
            return Ok(responseBodyWrapperModel);
        }


        static APIResponseBodyWrapperModel createResponseModel(int statusCode, string statusMessage, string statusMessageText, DateTime timestamp, object? data = null)
        {
            APIResponseBodyWrapperModel responseModel = new APIResponseBodyWrapperModel();
            responseModel.StatusCode = statusCode;
            responseModel.StatusMessage = statusMessage;
            responseModel.StatusMessageText = statusMessageText;
            responseModel.Timestamp = timestamp;
            if (data is GetRecordResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is GetFileResponseModel)
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

