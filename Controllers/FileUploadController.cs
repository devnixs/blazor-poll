using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Poll.DAL;
using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Utils;

namespace Poll.Controllers;

public class FileUploadController : Controller
{
    [HttpPost("/poll/upload")]
    public async Task<IActionResult> UploadFile([FromServices] ILogger<FileUploadController> logger, [FromServices] DatabaseWriteContextProvider databaseWriteContextProvider)
    {
        // Check if the request contains multipart/form-data.
        if (!Request.HasFormContentType)
        {
            return BadRequest("Unsupported media type.");
        }

        var form = await Request.ReadFormAsync();
        if (form.Files.Count == 0)
        {
            return BadRequest("No file uploaded.");
        }
        var file = form.Files.First();

        if (!MIMEHelper.ValidateContentType(file.ContentType))
        {
            return BadRequest("Invalid file type specified.");
        }
        
        if (file.Length > 1024 * 1024 * 20)
        {
            return BadRequest("File too large.");
        }

        var id = Guid.NewGuid();
        var filePath = Path.GetTempFileName();
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        logger.LogInformation("Uploaded file {file} with id {id}", file.FileName, id);
        await databaseWriteContextProvider.Write<PollContext, int>(db =>
        {
            db.Files.Add(new GameFile()
            {
                Path = filePath,
                Id = id,
                Size = file.Length,
                CreationDate = DateTimeOffset.UtcNow,
                OriginalFilename = file.FileName,
                ContentType = file.ContentType,
            });
            return Task.FromResult(0);
        });

        return Content(id.ToString(), "text/plain");
    }
    
    [Route("file/get/{id:guid}")]
    [HttpGet]
    public async Task GetFile([FromRoute] Guid id, [FromServices] ILogger<FileUploadController> logger, [FromServices] DatabaseReadContextProvider databaseReadContextProvider)
    {
        var gameFile = await databaseReadContextProvider.Read<PollContext, GameFile?>(async db =>
        {
            return await db.Files.SingleOrDefaultAsync(i => i.Id == id);
        });

        if (gameFile is null)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        if (!System.IO.File.Exists(gameFile.Path))
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Gone;
            return;
        }

        await using var stream = System.IO.File.OpenRead(gameFile.Path);
        HttpContext.Response.ContentType = gameFile.ContentType;
        HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        await stream.CopyToAsync(HttpContext.Response.Body);
    }
}