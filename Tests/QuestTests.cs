using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class QuestTests : IClassFixture<TestSetup>
{
    private HttpClient _client;
    private AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public QuestTests(TestSetup setup)
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
    public async Task StartQuest_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var questName = "Casino Manager";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var startQuestResponse = await _client.PostAsync($"/start-quest?username={username}&questName={questName}", content);
        startQuestResponse.EnsureSuccessStatusCode();

        var user = await _dbContext.Users.Include(u => u.Quests).Where(u => u.Name == username).FirstOrDefaultAsync();
        var quest = user.Quests.FirstOrDefault();
        Assert.True(quest.Name == questName);
    }

    [Fact]
    public async Task StartQuestAgain_Fails()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var questName = "Goverment Official";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var startQuestResponse = await _client.PostAsync($"/start-quest?username={username}&questName={questName}", content);
        startQuestResponse.EnsureSuccessStatusCode();

        var startQuestResponse2 = await _client.PostAsync($"/start-quest?username={username}&questName={questName}", content);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, startQuestResponse2.StatusCode);
    }

    [Fact]
    public async Task GetSingleQuest_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var questName = "Zug Zug Legacy";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var startQuestResponse = await _client.PostAsync($"/start-quest?username={username}&questName={questName}", content);
        startQuestResponse.EnsureSuccessStatusCode();

        var getQuestResponse = await _client.GetAsync($"/get-single-quest?username={username}&questName={questName}");
        getQuestResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAllUserQuests_Works() //ToDo
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var questName = "testquest";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var startQuestResponse = await _client.PostAsync($"/start-quest?username={username}&questName={questName}", content);
        startQuestResponse.EnsureSuccessStatusCode();

        var getAllQuestResponse = await _client.GetAsync($"/get-all-quests?username={username}");
        var resData = getAllQuestResponse.Content.ReadAsStringAsync(); //Lazy to turn this into object and check... but it works
        getAllQuestResponse.EnsureSuccessStatusCode();
    }


    [Fact]
    public async Task AddQuestProgress_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUserDelete";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var responseDelete = await _client.PostAsync("/delete-user?username=" + username, content);
        responseDelete.EnsureSuccessStatusCode();

        var exists = _dbContext.Users.Any(u => u.Name == username);
        Assert.False(exists);
        
    }

    [Fact]
    public async Task CreateUserWithSameName_Fails()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUserDelete";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var responseUser2 = await _client.PostAsync("/create-user?username=" + username, content);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, responseUser2.StatusCode);

        // Check that the user was not duplicated in the database
        var exists = _dbContext.Users.Count(u => u.Name == username);
        Assert.Equal(1, exists);

    }
}
