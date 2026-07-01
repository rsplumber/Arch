using Arch.Core.Pipeline.Models;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using DotNetCore.CAP;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Extensions.Http;

public static class HttpContextExtensions
{
    private const string HttpFactoryName = "arch";

    extension(HttpContext context)
    {
        public RequestState RequestState() => context.ProcessorState<RequestState>();

        public ICapPublisher EventBus() => context.Resolve<ICapPublisher>();

        public HttpClient HttpClient() => context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);

        public IServiceEndpointResolver LoadBalancer() => context.Resolve<IServiceEndpointResolver>();

        public IEndpointDefinitionResolver EndpointDefinitionResolver() => context.Resolve<IEndpointDefinitionResolver>();
    }
}
