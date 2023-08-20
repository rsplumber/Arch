using System.Text.Json;
using Core.Pipeline;
using Core.Pipeline.Models;
using Microsoft.AspNetCore.Http;
using static System.Net.Http.HttpMethod;

namespace Core.Extensions;

public static class HttpRequestExtensions
{
    public static string? Path(this HttpRequest request) => request.Path.Value?.ToLower();

    public static HttpMethod Method(this HttpRequest request) => request.Method switch
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

    public static bool HasBody(this HttpRequest request) => request.ContentLength > 0;

    public static async Task<dynamic?> ReadAsync(this HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.HasBody()) return null;
        using var streamReader = new StreamReader(request.Body);
        if (request.ContentType == RequestInfo.ApplicationJsonContentType)
        {
            return request.ReadAsJsonAsync(cancellationToken);
        }

        return await request.ReadAsFormAsync(cancellationToken: cancellationToken);
    }

    public static async Task<JsonDocument?> ReadAsJsonAsync(this HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.HasBody()) return null;
        using var streamReader = new StreamReader(request.Body);
        return JsonDocument.Parse(await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
    }

    public static Task<IFormCollection> ReadAsFormAsync(this HttpRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(request.Form);
    }

    public static string? ContentType(this HttpRequest request)
    {
        if (!request.HasBody()) return null;
        if (request.ContentType is not null && request.ContentType.StartsWith(RequestInfo.ApplicationJsonContentType))
        {
            return RequestInfo.ApplicationJsonContentType;
        }

        if (request.HasFormContentType)
        {
            return request.ContentType!.StartsWith("multipart/form-data") ? RequestInfo.MultiPartFormData : RequestInfo.UrlEncodedFormDataContentType;
        }

        return null;
    }

    public static Dictionary<string, string> Headers(this HttpRequest request) => request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!));

    public static string? ReadQueryString(this HttpRequest request) => request.QueryString.Value;
}