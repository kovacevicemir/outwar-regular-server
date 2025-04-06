using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using System.Text.Json;
using System.Text;

[Collection("Sequential Database Tests")]
public class WorldTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public WorldTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }

    [Fact]
    public async Task GetUserLocation_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var userRes = await _client.PostAsync("/create-user?username=" + username, content);
        userRes.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/get-user-location?username=" + username);
        response.EnsureSuccessStatusCode();

        var resData = await response.Content.ReadAsStringAsync();
        int[] resDataArray = JsonSerializer.Deserialize<int[]>(resData);

        var user = await _dbContext.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == username);

        // Compare coordinates x,y
        Assert.Equal(resDataArray[0], user.Location[0]);
        Assert.Equal(resDataArray[1], user.Location[1]);
    }

    [Fact]
    public async Task ChangeUserLocation_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var userRes = await _client.PostAsync("/create-user?username=" + username, content);
        userRes.EnsureSuccessStatusCode();

        var changeLocationRes = await _client.PostAsync($"/change-user-location?username={username}&direction=right", content);
        changeLocationRes.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/get-user-location?username=" + username);
        response.EnsureSuccessStatusCode();

        var resData = await response.Content.ReadAsStringAsync();
        int[] resDataArray = JsonSerializer.Deserialize<int[]>(resData);

        var user = await _dbContext.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == username);

        // Compare coordinates x,y
        Assert.Equal(resDataArray[0], user.Location[0]);
        Assert.Equal(resDataArray[1], user.Location[1]);
        Assert.Equal(user.Location[1], 1); //first location is 0, when move to right should be 1
    }
}
