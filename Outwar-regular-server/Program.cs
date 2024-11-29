using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Endpoints.Items;
using Outwar_regular_server.Endpoints.User;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnectionString")));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnectionString")));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular development server
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use the CORS policy
app.UseCors("AllowAngularDev");

// ** IMPORTANT: DONT EVEN THINK ABOUT ADDINT COMBAT SYSTEM ETC WASTE TIME...
// WE WANT 1. SHOW 2. 1 CLICK KILL 3. GET REWARD SO EVERYTHING TO WORK MOCK / MVP
// DONT BOTHER WITH SMALL DETAILS MAKE IT WORKING AND STRONG!

//LEAVE AT THE END
//Login system with maintainable JWT / refresh tokens and similar... / Email confirmation / simple but strong!
//1. send login request -> 2. return token or keep session or something (LEARN) 3. consume other api's 4. protect apis

//Protect apis - eg attack api somehow, cant just pass monster ID etc. it needs to be server (or redis) to client <- encrypted
//attack mob key phrase or something so nobody can use postman to attack...

// ** Performance ** Section
//  Any similar load/reads etc... its well worth it! memory/cpu/bugs 
//Quest system suggestion... temporary table with quests... once finished delete it, and add
//FinishedQuests or something userID, questID... easier for db and cleaner to lookup... maintainable...
//--------------------------------------------------------------------------------------------------------

//At this points start to add script or end to end testing API -> call APi -> call API--> validate...
//Example: create user, add items, upgrade item, add quest, attack monster, finish quest, check exp, item reward etc.

//LESS FUNCTIONALITY AND FEATURES & MORE STRONG / STABLE AND NEAT CODE!



//User api's
app.MapCreateUser();
app.MapDeleteUserByUsername();
app.MapGetUserByUsername();
app.MapIncreaseExp();
app.MapGetRanking();

//Item api's
app.MapAddItemToUserEndpoint();
app.MapDeleteItemFromUserEndpoint();
app.MapUpgradeItemLevelByItemId();
app.MapEquipItemEndpoint();
app.MapUnequipItemEndpoint();
app.MapGetUserLocation();

//Monster api's
app.MapAttackMonsterByName();

//Quest api's
app.MapStartQuestEndpoint();
app.MapGetAllUserQuestsEndpoint();
app.MapAddQuestProgressEndpoint();
app.MapGetQuestRewardEndpoint();
app.MapGetSingleQuestEndpoint();

//World api's
app.MapChangePlayerLocation();

//Crew api's
app.MapCreateCrewEndpoint();
app.MapGetCrewEndpoint();

//Redis api's ---REDIS---
app.MapCreateRaidEndpoint();
app.MapGetRaidDetailsEndpoint();
app.MapAttackRaidEndpoint();
app.MapCrewRaidsEndpoint();

app.Run();