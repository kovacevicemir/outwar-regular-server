using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class CrewTests : IClassFixture<TestSetup>
{
    private HttpClient _client;
    private AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public CrewTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateCrew_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var crewName = "testCrew";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var user = await _dbContext.Users.Where(u => u.Name == username).FirstOrDefaultAsync();

        var createCrewResponse = await _client.PostAsync($"/create-crew?crewLeaderId={user.Id}&crewName={crewName}", content);
        createCrewResponse.EnsureSuccessStatusCode();

        var crew = await _dbContext.Crews.Where(c => c.Name == crewName).FirstOrDefaultAsync();

        Assert.True(crew.Name == crewName);
    }

    [Fact]
    public async Task CreateCrewWithSameName_Fails()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var crewName = "testCrew";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var user = await _dbContext.Users.Where(u => u.Name == username).FirstOrDefaultAsync();

        var createCrewResponse = await _client.PostAsync($"/create-crew?crewLeaderId={user.Id}&crewName={crewName}", content);
        createCrewResponse.EnsureSuccessStatusCode();

        var createCrewResponse2 = await _client.PostAsync($"/create-crew?crewLeaderId={user.Id}&crewName={crewName}", content);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, createCrewResponse2.StatusCode);
    }

    [Fact]
    public async Task GetCrew_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var crewName = "testCrew";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var user = await _dbContext.Users.Where(u => u.Name == username).FirstOrDefaultAsync();

        var createCrewResponse = await _client.PostAsync($"/create-crew?crewLeaderId={user.Id}&crewName={crewName}", content);
        createCrewResponse.EnsureSuccessStatusCode();

        var getCrewResponse = await _client.GetAsync($"/get-crew?&crewName={crewName}");
        getCrewResponse.EnsureSuccessStatusCode();

        var responseContent = await getCrewResponse.Content.ReadAsStringAsync();
        var crew = JsonConvert.DeserializeObject<Crew>(responseContent);

        Assert.Equal(crew.Name, crewName);
    }
}
