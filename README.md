# WEBFISHING Dedicated Server

This is a dedicated server for the game WebFishing.  
It is a simple server that can be used to host a game of WebFishing.

## Prerequisites
- NET 9.0

## Features
- Host actor management
- Game events (meteor, raincloud, etc.)
- Server management
- Plugins

## Server Management
1. start the server.
2. open http://localhost:18300 url on your browser.

## GameVersion
- 1.1

## How to run the server
> [!WARNING]  
> You need to purchase the webfishing game on a different account and log in with the purchased account on another computer with Steam running for the server to operate properly. 

1. Copy the `appsettings.json` file to create a new file named `appsettings.local.json`
2. Edit `appsettings.local.json` file Server section
3. Run the server using the command `dotnet WFDS.Server.dll` or by executing the `WFDS.Server.exe` file.

### Configuration Rule
#### Room Types
- Public
- CodeOnly
- FriendsOnly
- Private

#### Room Code
- 6 characters (A-Z, 0-9)

## Contribution
Please feel free to contribute.  
Leave a Pull Request or Issue, and I will take a look.