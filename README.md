Outwar-regular-server is the main and probably the only one server.
It should include all API's and logic for the following actions:
-CRUD User
-CRUD Item
-CRUD User -> Item
-CRUD Monster
-CRUD Quest
-CRUD Game (Level, rage, stats, ranking and similar)

Everything is running in docker. Main docker-compose file can be found in /Solution Item/docker-compose.yml
-image: outwar-regular-server
-image: postgres:17.0 (more about access, setup pgadmin, backup etc in further readings)
Connection to postgres is stored in appsettings.json as connection string.

Entity framework uses code first approach. Main entry point for settings is AppDbContext.cs file and all models are in Models folder.


Structure is simple: all endpoints are in Endpoints folder
separated by entity, for example Items actions will be in Items folder.
All endpoints are then imported in Program.cs and mapped.