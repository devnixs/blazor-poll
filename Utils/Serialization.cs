using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poll.Utils;

public class Serialization
{
    public static JsonSerializerOptions DefaultSerializationOptions = new JsonSerializerOptions()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };
}