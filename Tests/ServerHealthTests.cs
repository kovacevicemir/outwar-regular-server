using Outwar_regular_server.Data;

[Collection("Sequential Database Tests")]
public class ServerHealthTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public ServerHealthTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }


    [Fact]
    public async Task ServerHealth_Works()
    {
        var response = await _client.GetAsync("/server-health");
        response.EnsureSuccessStatusCode();
    }
}
