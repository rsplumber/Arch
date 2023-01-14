using System.Text.Json;
using Arch;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.MapPost("/", async (HttpRequest request) =>
{
    var client = new HttpClient();
    var req = request.RequestInfoExecutor();
    var httpResponse = await client.PostAsJsonAsync("https://localhost:7023", req.Body);

});

app.Run();