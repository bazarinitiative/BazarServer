cd src/BazarServer.Api
dotnet build --configuration Release
mkdir -p ~/run/BazarServer
cp bin/Release/net6.0/* ~/run/BazarServer/ -r
