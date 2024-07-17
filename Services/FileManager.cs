using Poll.DAL.Entities;

namespace Poll.Services;

public class FileManager
{
    private readonly ILogger<FileManager> _logger;

    public FileManager(ILogger<FileManager> logger)
    {
        _logger = logger;
    }

    public string MoveFileToFinalLocation(GameFile file)
    {
        var now = DateTimeOffset.UtcNow;
        var newPath = Path.Combine("UploadedFiles", $"{now.Year}-{now.Month}-{now.Day}-{file.Id}{Path.GetExtension(file.OriginalFilename)}");
        
        // Ensure the directory exists
        var directoryName = Path.GetDirectoryName(newPath);
        Directory.CreateDirectory(directoryName!);

        _logger.LogInformation("Moving temporary file from {from} to {to}, {original}", file.Path, newPath, file.OriginalFilename);
        File.Move(file.Path, newPath);

        return newPath;
    }
}