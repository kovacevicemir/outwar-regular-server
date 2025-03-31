## Outwar backend
Outwar-regular-server is the main server.

### How to run (production)
Double click oneClick_setup_install_game.bat file from root directory. Run micro-outwar (frontend) individually with npm start as usual.

### How to run locally (development)
There are many ways to start this app. It required postgres db with proper database created and redis up and running on specified ports.
For development, the best way to do it is to run redis and postgres in docker, while backend .net can be started from IDE. Run micro-outwar (frontend) individually with npm start as usual.

**Backend**
should include all API's and logic for the following actions:

* CRUD User
* CRUD User -> Item
* CRUD Item
* CRUD Monster
* CRUD Quest
* CRUD Game (Level, rage, stats, ranking and similar)
* And more...

### Docker
Everything is running in docker. Main docker-compose file can be found in /Solution Item/docker-compose.yml -image: outwar-regular-server -image: postgres:17.0 (more about access, setup pgadmin, backup etc in further readings) Connection to postgres is stored in appsettings.json as connection string.

### Entity framework
Entity framework uses code first approach. Main entry point for settings is AppDbContext.cs file and all models are in Models folder.

### Endpoints
Structure is simple: all endpoints are in Endpoints folder separated by entity, for example Items actions will be in Items folder. All endpoints are then imported in Program.cs and mapped.

### Connection strings
Connection strings are stored in appsettings.json and this one is used in case of docker (or prod) automatically, while appsettings.Development.json are used for local and debugging