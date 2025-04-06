@echo off

echo Starting Docker Compose...
echo.

docker-compose up -d
echo.

REM Display message that the installation will take 5 minutes
echo Installation will take up to 5 minutes. Please don't touch anything...
echo.

REM Wait for 10 seconds
timeout /t 10
echo.

REM Navigate to the Outwar-regular-server folder
cd Outwar-regular-server
echo.

REM Once the wait is over, run EF database update
echo Running EF database update...
echo.

dotnet ef database update
echo.

echo Running EF database update for testing database...
timeout /t 7
dotnet ef database update --connection "Host=127.0.0.1;Port=5433;Database=outwar-db-test;Username=postgres;Password=admin;"

REM Wait for a few seconds to ensure the database update is completed
timeout /t 7
echo.

REM Make POST request to create the user
echo Creating user...
echo.

curl -X POST http://localhost:11399/create-user?username=test1
echo.

REM Wait for 7 seconds before verifying the user
timeout /t 7
echo.

REM Make GET request to verify user creation and capture the response
echo Verifying user...
echo.

curl -X GET "http://localhost:11399/get-user-by-username?username=test1"
echo.

REM Display final message with user prompt
echo.
echo.
echo Game is now up and running. You can visit http://localhost:4201 to play the game.
echo.

REM Wait for user input (will show the "don't press anything" message, and wait for user to press a key)
pause >nul
``
