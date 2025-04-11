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

### Services
ItemService, QuestService, UserService.. can be used to access common logic such as IncreasePlayerExp...
Not all logic (from endpoints) is moved to separate services yet, because services were added in later stages. Ideally all endpoints logic,
should belong to coresponding service, and then service should be used to do things. Services are great if we need to access and do things
from other endpoints eg. After killing monster, we need to increase exp which basically belongs to User endpoints.. but now since its moved to 
UserService.. we can increase exp right there.

### Docker
Everything is running in docker. Main docker-compose file can be found in /Solution Item/docker-compose.yml -image: outwar-regular-server -image: postgres:17.0 (more about access, setup pgadmin, backup etc in further readings) Connection to postgres is stored in appsettings.json as connection string.

1. Build the image
docker build -t emirkovacevic/backend:latest .

2. Login to Docker Hub
docker login
You’ll be prompted for your Docker Hub username and password.

3. Push the image to Docker Hub
docker push emirkovacevic/backend:latest

### Entity framework
Entity framework uses code first approach. Main entry point for settings is AppDbContext.cs file and all models are in Models folder.

### Endpoints
Structure is simple: all endpoints are in Endpoints folder separated by entity, for example Items actions will be in Items folder. All endpoints are then imported in Program.cs and mapped.

### Connection strings
Connection strings are stored in appsettings.json and this one is used in case of docker (or prod) automatically, while appsettings.Development.json are used for local and debugging