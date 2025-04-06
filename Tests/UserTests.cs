using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class UserTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public UserTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }

    [Fact]
    public async Task CreateUser_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var exists = _dbContext.Users.Any(u => u.Name == username);
        Assert.True(exists);
    }

    [Fact]
    public async Task IncreaseUserExp_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var exp = 100;
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var increaseExpRes = await _client.PostAsync($"/increase-exp?username={username}&exp={exp}", content);
        increaseExpRes.EnsureSuccessStatusCode();

        var userInDb = _dbContext.Users.FirstOrDefault(u => u.Name == username);
        Assert.True(userInDb.Experience == 100);
    }

    [Fact]
    public async Task UserRanking_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var rankingRes = await _client.GetAsync($"/get-ranking");
        rankingRes.EnsureSuccessStatusCode();

        var rankingData = await rankingRes.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(rankingData), "Ranking data should not be empty.");
    }

    [Fact]
    public async Task GetUser_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var getUserResponse = await _client.GetAsync("/get-user-by-username?username=" + username);
        getUserResponse.EnsureSuccessStatusCode();

        var responseContent = await getUserResponse.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseContent);

        Assert.NotNull(user);
        Assert.Equal(username, user.Name);
    }


    [Fact]
    public async Task DeleteUser_Works()
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
