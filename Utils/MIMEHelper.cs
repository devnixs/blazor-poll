namespace Poll.Utils;

public class MIMEHelper
{
    public static bool ValidateContentType(string contentType)
    {
        var validContenTypes = new HashSet<string>()
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/svg+xml",
            "image/tiff",
            "image/webp",
            "image/apng",
            "image/avif",
            "image/heif",
            "image/ico",
        };

        return validContenTypes.Contains(contentType);
    }
}