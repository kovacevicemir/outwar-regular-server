using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;

namespace Outwar_regular_server.Endpoints.User;

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