using Arch.Core.EndpointDefinitions;
using Arch.Core.Pipeline.Models;
using DotNetCore.CAP;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Extensions.Http;

public static class HttpContextExtensions
{
    private const string HttpFactoryName = "arch";

    public static RequestState RequestState(this HttpContext context) => context.ProcessorState<RequestState>();

    public static ICapPublisher EventBus(this HttpContext context) => context.Resolve<ICapPublisher>();

    public static HttpClient HttpClient(this HttpContext context) => context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);

    public static IServiceEndpointResolver LoadBalancer(this HttpContext context) => context.Resolve<IServiceEndpointResolver>();

    public static IEndpointDefinitionResolver EndpointDefinitionResolver(this HttpContext context) => context.Resolve<IEndpointDefinitionResolver>();
}