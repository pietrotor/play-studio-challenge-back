using PlayStudioServer.Services;

namespace PlayStudioServer.Controllers;

public static class YelpController
{
    public static void MapYelpEndpoints(this WebApplication app)
    {
        app.Map("/api/{**catchall}", async (HttpContext context, YelpProxyService proxyService) =>
        {
            return await proxyService.ProxyRequestAsync(context);
        });
    }
}