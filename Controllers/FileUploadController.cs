using Microsoft.AspNetCore.Mvc;

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
    }
}