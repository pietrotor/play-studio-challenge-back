using System.Net;
using PlayStudioServer.Services;

namespace PlayStudioServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYelpServices(this IServiceCollection services, IConfiguration configuration)
    {
        string frontendOrigin = configuration["FRONTEND_ORIGIN"] ?? "http://localhost:5173";

        services.AddHttpClient("yelp", client =>
        {
            client.BaseAddress = new Uri("https://api.yelp.com/v3/");
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy
                .WithOrigins(frontendOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        services.AddScoped<YelpProxyService>();

        return services;
    }
}