using PlayStudioServer.Extensions;
using PlayStudioServer.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddYelpServices(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.MapHealthEndpoints();
app.MapYelpEndpoints();

app.Run();
