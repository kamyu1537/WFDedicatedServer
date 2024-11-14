# WebFishing Dedicated Server

This is a dedicated server for the game WebFishing.  
It is a simple server that can be used to host a game of WebFishing.

## Game Version
- 1.1

## Prerequisites
- NET 9.0

## How to run the server
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