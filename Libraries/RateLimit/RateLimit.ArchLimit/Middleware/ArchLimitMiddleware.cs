using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using RateLimit.ArchLimit.Models;
using RateLimit.ArchLimit.Store;

namespace RateLimit.ArchLimit.Middleware;

public class ArchLimitMiddleware
{
    private readonly RequestDelegate _next;

    public ArchLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.RequestState().RequestInfo.Headers.TryGetValue("version", out var version);
        if (string.IsNullOrEmpty(version) || int.Parse(version) < ArchLimitDefaults.Version)
        {
            await _next(context);
            return;
        }

        var store = context.RequestServices.GetRequiredService<IArchLimitStore>();
        var ip = context.Connection.RemoteIpAddress!.ToString();
        var endpointDef = context.RequestState().Endpoint;
        var requestState = context.RequestState();

        bool isSpecial = endpointDef.Meta.Any(x => x.Key == "rate_limit");
        bool isDefault = !isSpecial;

        int maxRequests = isSpecial
            ? int.Parse(endpointDef.Meta["max_allowd_request_in_window"])
            : ArchLimitDefaults.MaxRequests;
        TimeSpan window = isSpecial
            ? TimeSpan.Parse(endpointDef.Meta["window_size"])
            : ArchLimitDefaults.Window;
        string identifierField = endpointDef.Meta.GetValueOrDefault("identifier_request_body") ?? "userid";

        string? identifier = null;
        if (!isDefault)
        {
            if (!context.Request.HasBody() || !endpointDef.Meta.ContainsKey("identifier_request_body"))
            {
                await context.Response.SendAsync(new Response
                {
                    RequestId = requestState.RequestInfo.RequestId,
                    RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                    Data = "درخواست شما نامعتبر است"
                }, 400);
                return;
            }

            identifier = await ReadIdentifier(context, identifierField);

            if (string.IsNullOrEmpty(identifier))
            {
                await context.Response.SendAsync(new Response
                {
                    RequestId = requestState.RequestInfo.RequestId,
                    RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                    Data = "درخواست شما نامعتبر است"
                }, 400);
                return;
            }
        }

        // Global IP-flood check (1-minute fixed window, same max as defaults)
        string globalKey = $"{ip}_global";
        var globalEntry = store.Get(globalKey);
        bool isLimited = false;
        bool isGlobalBlocked = false;
        RateLimitEntry? activeEntry = globalEntry;

        if (globalEntry is not null && DateTime.UtcNow - globalEntry.LastAccess < TimeSpan.FromMinutes(1))
        {
            if (globalEntry.Count > ArchLimitDefaults.MaxRequests)
            {
                isLimited = true;
                isGlobalBlocked = true;
            }
        }

        if (!isLimited)
        {
            string entryKey = isDefault
                ? globalKey
                : $"{identifier}_{endpointDef.Endpoint}";

            var entry = store.Get(entryKey);
            if (entry is null)
            {
                entry = new RateLimitEntry(DateTime.UtcNow);
                store.Set(entryKey, entry, TimeSpan.FromMinutes(10));
            }
            else if (DateTime.UtcNow - entry.LastAccess >= window)
            {
                entry.Count = 0;
                entry.LastAccess = DateTime.UtcNow;
            }

            entry.Count++;
            activeEntry = entry;

            if (entry.Count > maxRequests)
                isLimited = true;
        }

        if (isLimited)
        {
            var blockWindow = isGlobalBlocked ? ArchLimitDefaults.Window : window;
            var msg = $"به دلیل درخواست های مکرر حساب شما تا {RemainTime(activeEntry!.LastAccess, blockWindow)} دیگر مسدود شده است";
            await context.Response.SendAsync(new Response
            {
                RequestId = requestState.RequestInfo.RequestId,
                RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                Data = new { message = "Blocked", clientMessage = msg }
            }, 429);
            return;
        }

        await _next(context);
    }

    private static async Task<string?> ReadIdentifier(HttpContext context, string field)
    {
        context.Request.EnableBuffering();
        using var document = await JsonDocument.ParseAsync(context.Request.Body);
        context.Request.Body.Position = 0;
        return document.RootElement.ValueKind == JsonValueKind.Object
               && document.RootElement.TryGetProperty(field, out var value)
            ? value.ToString()
            : null;
    }

    private static string RemainTime(DateTime lastAccess, TimeSpan window)
    {
        var end = lastAccess + window;
        var remaining = (int)(end - DateTime.UtcNow).TotalSeconds;
        return $"{remaining / 60}:{remaining % 60}";
    }
}
