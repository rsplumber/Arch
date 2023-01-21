using Core.RequestDispatcher;
using Data.Sql;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");

builder.Services.AddData(builder.Configuration);
builder.Services.AddCors();

builder.Services.AddFastEndpoints();
builder.Services.AddScoped<IRequestDispatcher, RequestDispatcher>();
builder.Services.AddSwaggerDoc(settings =>
{
    settings.Title = "Arch - WebApi";
    settings.DocumentName = "v1";
    settings.Version = "v1";
}, addJWTBearerAuth: false, maxEndpointVersion: 1);
var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });

// if (app.Environment.IsDevelopment())
// {
app.UseOpenApi();
app.UseSwaggerUi3(s => s.ConfigureDefaults());
// }


await app.RunAsync();