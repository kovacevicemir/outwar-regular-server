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

### Redis (how to connect quick)
right click on container or (actions 3 dots) and connect via terminal
type redis-cli this should connect to it in cli context
now you can do something like KEYS * this should give all existing keys
Or even better:

first: redis-cli

for i in $(redis-cli KEYS '*'); do echo $i; redis-cli GET $i; done (this will list key-value pairs)

flushall (deletes everything)

## TODO

Task 6: Game content & balance & Test and make sure there is at least 1 month of content aprox. (in progress)

Task 7: Local play script and tutorial
1. Just make sure average joe install one thing such as docker, and run script oneClick and he is able to play.

BUGS:
Task 8: calculate set bonus frontend fix?
Task 9: Calculate set bonus backend & level bonus when fighting monster fix...

CONTENT:
Task 10: Increase osteology monster quest level * amount (swap with stronger monsters)
Task 11: Increase Goverment monster quest level * amount (swap with stronger monsters)
Task 12: Add quest for around level 16 that gives 10k exp
Task 13: Add quest for around level 18 that gives 20k exp

MULTIPLAYER:
Task 26: Add multiplayer system
 -Add user endpoint already exists use this one, it should generate unique 16char password and return it in response
 -Use this password to store in (what is longer cookies?) outwar-x-info{users:{name,password}, currentUser:name}
 -Use this cookie or whatever in requests
 -Add switcher between users for eg switch between test1 and test2
 -Make frontend and backend work nicely

CREW:
Task 14: Create Join raid endpoint
Task 15: Create Invite to crew endpoint
Task 16: Create Join crew endpoint, and ui invitation list
Task 17: Create leave crew endpoint, and ui button.

SMALL FEATURES:
Task 18: Max inventory items (50)
Task 19: Max Crew members (5)
Task 20: Add Exp list up to 60 Level
Task 21: Add rampage logic (if rampage dont spend any rage on attack, actually increase rage by same amount)
Task 22: Display current points in blacksmith

FEATURES: 
Task 21: Growth yesterday!
-Add growth yesterday field (maybe separate table probably the best) and relate it to User
-Add background task that everyday at 12: current user exp - yesterday growth and store it at growth yesterday
-Display this in UI

Task 22: Add Slot property to item 1-4 slots (15% 4 slots, 25% 3 slots, 25% 2 slots, 35% 1 slot) 
-add augment (augment can have exp,rage,att,hp,cit,rmpg etc)
-each slot can have augment
-item should show +5exp (if any augment have it etc...)
-Ui error handling, messages, and expand blacksmith with this..

Task 23: Add augment drops to every monster with 1% chance and random stats logic
Task 24: Add 2-3 quests with really good augments! (5% block, 5% crit, 20attack)
Task 25: Trade system 

Task 26: Add lucky item drops (pumpkin, heart, box etc), that can drop:
+ random rage 50-500,
+ random exp: 1000-5000
+ random points 1-5 (low chance)
