using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class ItemsTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private readonly TestApplicationFactory _factory;

    public ItemsTests(TestSetup setup)
    {
        _client = setup.Client;
        _dbContext = setup.DbContext;
        _factory = setup._factory;
    }

    [Fact]
    public async Task AddItemToUser_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Construction Hat";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var addItemresponse = await _client.PostAsync($"/add-item-to-user?username={username}&itemName={itemName}", content);
        addItemresponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);
        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        Assert.NotNull(item);
        Assert.True(user.Items.Count > 0);
    }

    [Fact]
    public async Task BuyShopItem_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Dead Eye";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        // Check if buying item with 0 points fails
        var buyItemResponse = await _client.PostAsync($"/buy-item-from-shop?username={username}&itemName={itemName}", content);
        buyItemResponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);

        // Add points and try again
        user.Points = 500;
        _dbContext.SaveChanges();

        var buyItemResponse2 = await _client.PostAsync($"/buy-item-from-shop?username={username}&itemName={itemName}", content);
        buyItemResponse2.EnsureSuccessStatusCode();

        // Refetch user
        user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);

        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        Assert.NotNull(item);
        Assert.True(user.Items.Count > 0);
    }


    [Fact]
    public async Task DeleteItem_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Construction Hat";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var addItemresponse = await _client.PostAsync($"/add-item-to-user?username={username}&itemName={itemName}", content);
        addItemresponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);
        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        var deleteItemresponse = await _client.PostAsync($"/delete-item-from-user-by-item-id?username={username}&itemId={item.Id}", content);
        deleteItemresponse.EnsureSuccessStatusCode();

        _dbContext.SaveChanges();

        var exists = _dbContext.Items.Any(i => i.Id == item.Id);

        Assert.True(!exists);
    }

    [Fact]
    public async Task EquipItem_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Construction Hat";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var addItemresponse = await _client.PostAsync($"/add-item-to-user?username={username}&itemName={itemName}", content);
        addItemresponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);

        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        var equipItemResponse = await _client.PostAsync($"/equip-item?username={username}&itemId={item.Id}", content);
        equipItemResponse.EnsureSuccessStatusCode();


        await _dbContext.SaveChangesAsync();

        //Because EF Core doesn’t map ICollection<int> directly. this is workaround to refetch user... and check if equipedItems is updated
        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRefetch = await freshDb.Users.FirstOrDefaultAsync(u => u.Name == username);

        var equipedExists = userRefetch.EquipedItemsId.FirstOrDefault();

        Assert.True(equipedExists == item.Id);
    }


    [Fact]
    public async Task UnEquipItem_Works() //TODO
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Construction Hat";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var addItemresponse = await _client.PostAsync($"/add-item-to-user?username={username}&itemName={itemName}", content);
        addItemresponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);

        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        var equipItemResponse = await _client.PostAsync($"/equip-item?username={username}&itemId={item.Id}", content);
        equipItemResponse.EnsureSuccessStatusCode();

        var unequipItemResponse = await _client.PostAsync($"/unequip-item?username={username}&itemId={item.Id}", content);
        unequipItemResponse.EnsureSuccessStatusCode();

        await _dbContext.SaveChangesAsync();

        //Because EF Core doesn’t map ICollection<int> directly. this is workaround to refetch user... and check if equipedItems is updated
        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRefetch = await freshDb.Users.FirstOrDefaultAsync(u => u.Name == username);

        var equipedItemsCount = userRefetch.EquipedItemsId.Count();

        Assert.True(equipedItemsCount == 0);
    }

    [Fact]
    public async Task UpgradeItem_Works()
    {
        await _factory.ResetDatabaseAsync();

        var username = "testUser";
        var itemName = "Construction Hat";
        var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/create-user?username=" + username, content);
        response.EnsureSuccessStatusCode();

        var addItemresponse = await _client.PostAsync($"/add-item-to-user?username={username}&itemName={itemName}", content);
        addItemresponse.EnsureSuccessStatusCode();

        var user = _dbContext.Users
            .Include(u => u.Items)
            .FirstOrDefault(u => u.Name == username);

        var item = user.Items.FirstOrDefault(i => i.Name == itemName);

        user.Points = 500; //Required to upgrade item

        await _dbContext.SaveChangesAsync();

        var upgradeItemResponse = await _client.PostAsync($"/upgrade-item-level-by-item-id?username={username}&itemId={item.Id}", content);
        upgradeItemResponse.EnsureSuccessStatusCode();

        //Because EF Core doesn’t map ICollection<int> directly. this is workaround to refetch user... and check if equipedItems is updated
        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRefetch = await freshDb.Users.Include(u => u.Items).FirstOrDefaultAsync(u => u.Name == username);

        var upgradedItem = userRefetch.Items.FirstOrDefault();

        Assert.True(upgradedItem.UpgradeLevel > 0);
        Assert.True(upgradedItem.Stats[1] > 15); //Default stat[1] for Construction Hat is 15... if upgraded should be higher
    }
}
