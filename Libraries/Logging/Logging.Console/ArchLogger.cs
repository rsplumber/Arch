using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logging.Abstractions;

namespace Logging.Console;

internal sealed class ArchLogger : IArchLogger
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public Task LogAsync(dynamic message)
    {
        System.Console.WriteLine(JsonSerializer.Serialize(message, JsonSerializerOptions));
        return Task.CompletedTask;
    }
}