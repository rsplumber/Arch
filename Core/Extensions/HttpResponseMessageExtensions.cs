using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arch.Core.Extensions;

public static class HttpResponseMessageExtensions
{
    private const string ApplicationJsonMediaType = "application/json";
    private const string ApplicationProblemJsonMediaType = "application/problem+json";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public static string? MediaType(this HttpResponseMessage httpResponse) => httpResponse.Content.Headers.ContentType?.MediaType;

    public static string? ContentType(this HttpResponseMessage httpResponse) => httpResponse.Content.Headers.ContentType?.ToString();

    public static async ValueTask<dynamic?> ReadBodyAsync(this HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
    {
        if (httpResponse.MediaType() is ApplicationJsonMediaType or ApplicationProblemJsonMediaType)
        {
            return await httpResponse.Content.ReadFromJsonAsync<dynamic>(JsonSerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public static Dictionary<string, string> Headers(this HttpResponseMessage response) => response.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!));
}