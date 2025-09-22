namespace PlayStudioServer.Controllers;

public static class HealthController
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
    }
}