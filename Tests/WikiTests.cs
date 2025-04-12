using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Outwar_regular_server.Data;
using Outwar_regular_server.Models;
using System.Text;

[Collection("Sequential Database Tests")]
public class WikiTests : IClassFixture<TestSetup>
{
    private readonly HttpClient _client;

    public WikiTests(TestSetup setup)
    {
        _client = setup.Client;
    }

    [Fact]
    public async Task GetDropList_Works()
    {
        var response = await _client.GetAsync("/get-droplist");
        response.EnsureSuccessStatusCode();

        //To do more checks - but this is good for now since this is not important feature
    }
}
