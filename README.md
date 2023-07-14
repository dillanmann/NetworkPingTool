# Network Ping Tool

## Description
A basis Blazor Server web app leveraging SignalR to enable web addresses to be pinged by a web server and push the results to the front end.

## Installation
### Pre-requisites
- Dotnet 7 SDK (or runtime) https://dotnet.microsoft.com/en-us/download/dotnet/7.0

## Publish the tool for use
### Publish
1. Pull the repo
2. Open a command prompt in the NetworkPingTool project folder
3. Run `dotnet publish -c Release`
4. Navigate to the `/bin/Release/net7.0/publish` folder
5. Run `NetworkPingTool.exe`
6. Make a note of the address the application is running on e.g. `Now listening on: http://localhost:5000`
7. Stop the app

### Configure
1. Open appsettings.json
2. Ensure that the value of the `ApiRootUrl` configuration element matches the address listed above
3. The app is good to run again