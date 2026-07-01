using System.Text.Json;
using Arch.Core.Pipeline.Models;
using Microsoft.AspNetCore.Http;
using static System.Net.Http.HttpMethod;

namespace Arch.Core.Extensions.Http;

public static class HttpRequestExtensions
{
    extension(HttpRequest request)
    {
        public string? Path() => request.Path.Value;

        public HttpMethod Method() => request.Method switch
        {
            "GET" or "get" or "Get" => Get,
            "DELETE" or "delete" or "Delete" => Delete,
            "PATCH" or "patch" or "Patch" => Patch,
            "POST" or "post" or "Post" => Post,
            "PUT" or "put" or "Put" => Put,
            "HEAD" or "head" or "Head" => Head,
            "OPTIONS" or "options" or "Options" => Options,
            _ => Get
        };

        public bool HasBody() => request.ContentLength > 0;

        public async Task<dynamic?> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (!request.HasBody()) return null;
            using var streamReader = new StreamReader(request.Body);
            if (request.ContentType == RequestInfo.ApplicationJsonContentType)
            {
                return request.ReadAsJsonAsync(cancellationToken);
            }

            return await request.ReadAsFormAsync(cancellationToken: cancellationToken);
        }

        public async Task<JsonDocument?> ReadAsJsonAsync(CancellationToken cancellationToken = default)
        {
            if (!request.HasBody()) return null;
            using var streamReader = new StreamReader(request.Body);
            return JsonDocument.Parse(await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
        }

        public Task<IFormCollection> ReadAsFormAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Form);
        }

        public string? ContentType()
        {
            var contentType = request.ContentType;
            if (contentType is not null && contentType.StartsWith(RequestInfo.ApplicationJsonContentType, StringComparison.Ordinal))
            {
                return RequestInfo.ApplicationJsonContentType;
            }

            if (contentType is not null && contentType.StartsWith(RequestInfo.PlainTextContentType, StringComparison.Ordinal))
            {
                return RequestInfo.PlainTextContentType;
            }

            if (request.HasFormContentType)
            {
                return contentType!.StartsWith(RequestInfo.MultiPartFormData, StringComparison.Ordinal) ? RequestInfo.MultiPartFormData : RequestInfo.UrlEncodedFormDataContentType;
            }

            return null;
        }

        public Dictionary<string, string> Headers() => request.Headers.ToDictionary(a => a.Key, a => string.Join(';', a.Value!));

        public string? ReadQueryString() => request.QueryString.Value;
    }
}
