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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserFiles", targetFile.FileName);
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
    }

}

