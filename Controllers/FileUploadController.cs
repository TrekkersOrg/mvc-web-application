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
            APIResponseBodyWrapperModel responseBody = new APIResponseBodyWrapperModel();
            UploadFileResponseModel uploadFileResponseBody = new UploadFileResponseModel();
            if (targetFile != null && targetFile.Length > 0)
            {
                var formattedFileName = targetFile.FileName.Replace(" ", "_");
                var currentDirectory = Directory.GetCurrentDirectory();
                var filePath = "";
                if (currentDirectory.ToLower().EndsWith("\\scripts") || currentDirectory.ToLower().EndsWith("/scripts"))
                {
                    var parentDirectory = Directory.GetParent(currentDirectory).FullName;
                    filePath = Path.Combine(parentDirectory, "UserFiles", formattedFileName);
                }
                else
                {
                    filePath = Path.Combine(currentDirectory, "UserFiles", formattedFileName);
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    targetFile.CopyTo(fileStream);
                }
                uploadFileResponseBody.FileName = formattedFileName;
                uploadFileResponseBody.FileType = targetFile.ContentType;
                uploadFileResponseBody.FileSize = targetFile.Length;
                responseBody = createResponseModel(200, "Success", "File uploaded successfully.", DateTime.Now, uploadFileResponseBody);
                return Ok(responseBody);
            }
            responseBody = createResponseModel(200, "Success", "File name must not be null or empty.", DateTime.Now, null);
            return Ok(responseBody);
        }

        /// <summary>
        /// Deletes file from "UserFiles".
        /// </summary>
        /// <param name="requestBody" type="DeleteFileRequestModel"></param>
        /// <returns type="ActionResult"></returns>
        [HttpDelete("delete")]
        public ActionResult Delete([FromBody] DeleteFileRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseBody = new APIResponseBodyWrapperModel();
            DeleteFileResponseModel deleteFileResponseBody = new DeleteFileResponseModel();
            var targetFileName = requestBody.FileName;
            if (!string.IsNullOrEmpty(targetFileName))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", targetFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    deleteFileResponseBody.FileName = targetFileName;
                    deleteFileResponseBody.isDeleted = true;
                    responseBody = createResponseModel(200, "Success", "File deleted successfully.", DateTime.Now, deleteFileResponseBody);
                    return Ok(responseBody);
                }
                else
                {
                    deleteFileResponseBody.FileName = targetFileName;
                    deleteFileResponseBody.isDeleted = false;
                    responseBody = createResponseModel(200, "Success", "File not found.", DateTime.Now, deleteFileResponseBody);
                    return Ok(responseBody);
                }
            }
            deleteFileResponseBody.FileName = targetFileName;
            deleteFileResponseBody.isDeleted = false;
            responseBody = createResponseModel(200, "Success", "File name field is null or empty.", DateTime.Now, deleteFileResponseBody);
            return Ok(responseBody);
        }

        /// <summary>
        /// Gets file from "UserFiles".
        /// </summary>
        /// <param name="requestBody" type="GetFileRequestModel"></param>
        /// <returns type="ActionResult"></returns>
        [HttpPost("getFile")]
        public ActionResult Get([FromBody] GetFileRequestModel requestBody)
        {
            APIResponseBodyWrapperModel responseBody = new APIResponseBodyWrapperModel();
            GetFileResponseModel getFileResponseModel = new GetFileResponseModel();
            if (requestBody.FileName == null || requestBody.FileName == "")
            {
                getFileResponseModel.FileName = requestBody.FileName;
                getFileResponseModel.FileExists = false;
                return Ok(createResponseModel(200, "Success", "File name must not be empty.", DateTime.Now, null));
            }
            var targetFileName = requestBody.FileName;
            string userFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", targetFileName);
            bool fileExists = System.IO.File.Exists(userFilesPath);
            getFileResponseModel.FileName = targetFileName;
            getFileResponseModel.FileExists = fileExists;
            responseBody = createResponseModel(200, "Success", "File existence checked successfully.", DateTime.Now, getFileResponseModel);
            return Ok(responseBody);
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
            if (data is GetRecordResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is GetFileResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is DeleteFileResponseModel)
            {
                responseModel.Data = data;
            }
            else if (data is UploadFileResponseModel)
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

