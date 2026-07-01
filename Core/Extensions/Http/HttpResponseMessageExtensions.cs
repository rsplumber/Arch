using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arch.Core.Extensions.Http;

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

    extension(HttpResponseMessage httpResponse)
    {
        public string? MediaType() => httpResponse.Content.Headers.ContentType?.MediaType;

        public string? ContentType() => httpResponse.Content.Headers.ContentType?.ToString();

        public async ValueTask<dynamic?> ReadBodyAsync(CancellationToken cancellationToken = default)
        {
            if (httpResponse.MediaType() is ApplicationJsonMediaType or ApplicationProblemJsonMediaType)
            {
                return await httpResponse.Content.ReadFromJsonAsync<dynamic>(JsonSerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        public Dictionary<string, string> Headers() => httpResponse.Headers.ToDictionary(a => a.Key, a => string.Join(';', a.Value!));
    }
}
