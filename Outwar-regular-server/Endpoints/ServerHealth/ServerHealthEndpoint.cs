namespace Outwar_regular_server.Endpoints.ServerHealth;

public static class ServerHealthEndpoint
{
    public static IEndpointRouteBuilder MapGetServerHealth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/server-health", () => Task.FromResult(Results.Ok()))
            .WithName("GetServerHealth")
            .WithOpenApi();

        return app;
    }
}