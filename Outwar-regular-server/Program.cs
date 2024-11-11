using Microsoft.EntityFrameworkCore;
using Outwar_regular_server.Data;
using Outwar_regular_server.Endpoints.Items;
using Outwar_regular_server.Endpoints.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnectionString")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Login system with maintainable JWT / refresh tokens and similar... / Email confirmation / simple but strong!
//1. send login request -> 2. return token or keep session or something (LEARN) 3. consume other api's 4. protect apis

//Add monsters.json
//Simple attack api (just attack - def, attack - def return true or false for now)
//Protect this api somehow, cant just pass monster ID etc. it needs to be server (or redis) to client <- encrypted
//attack mob key phrase or something so nobody can use postman to attack...

//Quest system suggestion... temporary table with quests... once finished delete it, and add
//FinishedQuests or something userID, questID... easier for db and cleaner to lookup... maintainable...

//At this points start to add script or end to end testing API -> call APi -> call API--> validate...
//Example: create user, add items, upgrade item, add quest, attack monster, finish quest, check exp, item reward etc.

//Add Room/world system

//shncdn & angular create [simple & mobile] clean AF! 
// ---- top bar----
// |menu| action...

//LESS FUNCTIONALITY AND FEATURES & MORE STRONG / STABLE AND NEAT CODE!


app.MapCreateUser();
app.MapDeleteUserByUsername();
app.MapAddItemToUserEndpoint();
app.MapDeleteITemFromUserEndpoint();
app.MapUpgradeItemLevelByItemId();

app.Run();