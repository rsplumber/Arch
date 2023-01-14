using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");


builder.Services.AddCors();

builder.Services.AddFastEndpoints();


var app = builder.Build();


app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
    config.Endpoints.Filter = ep => ep.EndpointTags?.Contains("hidden") is not true;
});


// if (app.Environment.IsDevelopment())
// {
// }


await app.RunAsync();