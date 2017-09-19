# Developing
## Prerequisities
* [Git](http://git-scm.com/downloads)
### Frontend
* [node.js](https://nodejs.org/en/)
### Backend
* [Visual Studio](http://go.microsoft.com/fwlink/?LinkId=309297&clcid=0x409&slcid=0x409)

## Continuous integration
* [Jenkinsfile](./Jenkinsfile) build script
# Configuration
Environment is selected using the .NET Core standard **ASPNETCORE_ENVIRONMENT** environment variable.
**nin.json** configuration file will always be loaded.  If a file named **nin.{envionrment}.json** exists, any settings defined in that file will override the default settings in **nin.sjon**.
# REST Services
The backend consists of 3 ASP.NET Core Web API projects.
## Api
All the interesting bits are here.  This is a read-only API.
### Dependencies
  * SQL Server
  * RavenDB
## Api.Proxy
A "dumb" proxy used for map tiles from Statens kartverk.
### Dependencies
  * None
## Api.Document
Functions for uploading new data sets and displaying previous uploads.
### Dependencies
  * RavenDB
# Runtime environment
* Windows Server 2008
* SQL Server
* IIS
* .NET Framework 4.6
## Staging
# Installing in IIS

##Add .geojson MIME type
*Make sure Windows features: Internet Information Service: World Wide Web Service: Common HTTP Features: Static Content is installed
*Open IIS Manager
*Click MIME Types and then add the extension:
**File name extension: .geojson
**MIME type: application/json

##Install the .NET Core Windows Server Hosting bundle

* Install Microsoft .NET Framework 4.6.1 (https://www.microsoft.com/en-us/download/details.aspx?id=49982)
* Install the .NET Core Windows Server Hosting bundle (https://go.microsoft.com/fwlink/?LinkId=817246) on the server. The bundle will install the .NET Core Runtime, .NET Core Library, and the ASP.NET Core Module. The module creates the reverse-proxy between IIS and the Kestrel server.

* Execute iisreset at the command line or restart the server to pickup changes to the system PATH.

* Add a new Application pool, set .NET CLR Version to "No managed code"
* use dotnet publish command to create contents of the virtual directory.
* Add application, choose the app pool, point to publish directory.
** Open Modules in the Application, AspNetCoreModule should be there.
* Grant read access to group "Authenticated Users" in the base Nin directory

## Virtual folder: NinMap
* Add a virtual folder NinMap, point to directory specified in nin.json 'Settings.Map.MapLayersPath'.
* Open directory browsing feature, enable
* Grant read access to group IIS_IUSRS in the map directory

* https://stackoverflow.com/questions/31049152/asp-net-5-publish-to-iis-setting-aspnet-env-variables

# SQL Server

* Create user IIS APPPOOL\Nin, make db_owner of "Config.Settings.ConnectionString"

# TODO

Import:
https://kartkatalog.miljodirektoratet.no/Dataset/Details/0

Testing: web-hook

