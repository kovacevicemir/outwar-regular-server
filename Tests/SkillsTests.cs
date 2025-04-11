using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class SkillsTests : IClassFixture<TestSetup>
{
    private HttpClient _client;
    private AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public SkillsTests(TestSetup setup)
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
    public async Task IncreaseSkillLevel_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var skillName = "Stealth";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var user = await _dbContext.Users.Where(u => u.Name == username).FirstOrDefaultAsync();
        user.SkillPoints = 1;
        await _dbContext.SaveChangesAsync();

        var increaseSkillResponse = await _client.PostAsync($"/increase-skill-level?username={username}&skillName={skillName}", content);
        increaseSkillResponse.EnsureSuccessStatusCode();


        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRefetch = await freshDb.Users.FirstOrDefaultAsync(u => u.Name == username);

        //1 is Stealth... 0 is Ambush - we only have 2 skills at the moment.
        Assert.True(userRefetch.Skills[1] > 0);
    }

    [Fact]
    public async Task IncreaseSkillLevel_No_SkillPoints_Fails()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var skillName = "Stealth";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var increaseSkillResponse = await _client.PostAsync($"/increase-skill-level?username={username}&skillName={skillName}", content);
        increaseSkillResponse.EnsureSuccessStatusCode();

        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRefetch = await freshDb.Users.FirstOrDefaultAsync(u => u.Name == username);

        Assert.True(userRefetch.SkillPoints == 0);
    }
}
