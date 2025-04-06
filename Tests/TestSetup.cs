using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Outwar_regular_server.Data;
using Outwar_regular_server.Endpoints;

public class TestSetup : IAsyncLifetime
{
    public HttpClient Client { get; private set; }
    public AppDbContext DbContext { get; private set; }

    public TestApplicationFactory _factory;

    public async Task InitializeAsync()
    {
        _factory = new TestApplicationFactory(); //Explanation about this class can be found in class itself

        //Since TestApplicationFactory inherits from WebApplicationFactory<Program>,
        //it automatically has access to the CreateClient() method from WebApplicationFactory
        //This method creates an HttpClient that acts like a real HTTP client,
        //but it is used to send requests to the in-memory version of your application (not a real server).
        Client = _factory.CreateClient();
        Client.BaseAddress = new Uri("http://localhost");

        //Create a scope (like a "lifetime" for services in this test).
        //Get the AppDbContext (AppDbContext is from main project) from the DI container(just like your app would).
        var scope = _factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Run EF migrations
        DbContext.Database.Migrate();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
