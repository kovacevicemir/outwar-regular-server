using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Outwar_regular_server.Data;

// The purpose of WebApplicationFactory is to set up a test server that can run your application in an in-memory environment
// during testing. It allows you to simulate requests to your app without needing to spin up an actual web server.
// By extending WebApplicationFactory<Program>, you inherit all of the functionality of WebApplicationFactory,
// which includes things like creating an in-memory HTTP client and wiring up services for your app (like DbContext, logging, etc.).
public class TestApplicationFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; private set; } = "Host=localhost;Port=5433;Username=postgres;Password=admin;Database=outwar-db-test";
    private NpgsqlConnection _connection;
    private Respawner _respawner;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Just getting connection string from appsettings.json
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();

        //ConnectionString = configuration.GetConnectionString("PostgresTestDb"); Some bug here... TODO -fix

        //Init Respawner - lib for reseting db state: Calling it here because it required connections string
        InitRespawnerAsync().GetAwaiter().GetResult();

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddConfiguration(configuration);
        });

        builder.ConfigureServices(services =>
        {
            // Remove default DbContext registration if it exists
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register test DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(ConnectionString));
        });

        return base.CreateHost(builder);
    }


    //Respawn library resets db state. Calling it manually for each test: 1.InitRespawnerAsync Init 2. ResetDatabaseAsync
    public async Task InitRespawnerAsync()
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });
    }

    //Respawn library resets db state. Calling it manually for each test: 1.InitRespawnerAsync Init 2. ResetDatabaseAsync
    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });

        await respawner.ResetAsync(conn);
    }
}
