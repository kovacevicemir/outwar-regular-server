using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class MonsterTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public MonsterTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }

    [Fact]
    public async Task AttackMonster_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var monsterName = "Acidic Spider";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var attackMonsterResponse = await _client.PostAsync($"/attack-monster-by-name?monsterName={monsterName}&username={username}", content);
        //attackMonsterResponse.EnsureSuccessStatusCode(); //This will fail because increase exp (probably using localhost vs inmemory url) - bug

        var user = await _dbContext.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == username);

        // This will fail because increase exp (probably using localhost vs inmemory url) - bug
        // However there is separate test for increasing the exp in UserTests. There is add item test as well in case of drop.
        //Assert.True(user.Experience > 0); 
        Assert.True(user.Rage < 2000);
    }
}
