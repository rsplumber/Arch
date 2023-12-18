using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Logging.Abstractions;

namespace Arch.Logging.Console;

internal sealed class ArchLogger : IArchLogger
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    public Task LogAsync(dynamic message)
    {
        System.Console.WriteLine(JsonSerializer.Serialize(message, JsonSerializerOptions));
        return Task.CompletedTask;
    }
}