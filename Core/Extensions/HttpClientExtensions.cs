using System.Net.Http.Headers;
using System.Net.Http.Json;
using Arch.Core.Pipeline.Models;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Extensions;

public static class HttpClientExtensions
{
    public static async ValueTask<HttpResponseMessage?> SendAsync(this HttpClient client, HttpMethod method, string url, HttpRequest request)
    {
        var disposableObjects = new List<IDisposable>();
        try
        {
            var httpRequest = new HttpRequestMessage(method, url);
            disposableObjects.Add(httpRequest);
            if (!request.HasBody()) return await client.SendAsync(httpRequest).ConfigureAwait(false);
            switch (request.ContentType())
            {
                case RequestInfo.ApplicationJsonContentType:
                    httpRequest.Content = JsonContent.Create(await request.ReadAsJsonAsync());
                    break;
                case RequestInfo.MultiPartFormData:
                {
                    var multiPartFormCollection = await request.ReadAsFormAsync();
                    var multipartFormDataContent = new MultipartFormDataContent();
                    foreach (var keyValuePair in multiPartFormCollection)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value!), keyValuePair.Key);
                    }

                    foreach (var formFile in multiPartFormCollection.Files)
                    {
                        var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream).ConfigureAwait(false);
                        memoryStream.Position = 0;
                        var streamContent = new StreamContent(memoryStream);
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
                        multipartFormDataContent.Add(streamContent, formFile.Name, formFile.FileName);
                        disposableObjects.Add(memoryStream);
                    }

                    httpRequest.Content = multipartFormDataContent;
                    disposableObjects.Add(multipartFormDataContent);
                    break;
                }
                case RequestInfo.UrlEncodedFormDataContentType:
                {
                    var formCollection = await request.ReadAsFormAsync();
                    httpRequest.Content = new FormUrlEncodedContent(formCollection
                        .SelectMany(keyValuePair => keyValuePair.Value
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Select(value => new KeyValuePair<string, string>(keyValuePair.Key, value!)))
                        .ToArray());
                    break;
                }
            }

            return await client.SendAsync(httpRequest).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
        finally
        {
            disposableObjects.ForEach(disposable => disposable.Dispose());
            disposableObjects.Clear();
        }
    }
}