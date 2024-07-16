using Microsoft.AspNetCore.Mvc;

namespace Poll.Controllers;

public class FileUploadController : Controller
{
    [Route("/poll/upload")]
    [HttpPost]
    public async Task<IActionResult> UploadFile()
    {
        // Check if the request contains multipart/form-data.
        if (!Request.HasFormContentType)
            return BadRequest("Unsupported media type");

        var form = await Request.ReadFormAsync();
        var file = form.Files["image"]; // 'filepond' is the name attribute in your FilePond input

        if (file == null)
            return BadRequest("No file uploaded.");

        var filePath = Path.Combine("UploadedFiles", file.FileName);

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { FilePath = filePath });
    }
}