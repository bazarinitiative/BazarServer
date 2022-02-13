cd src/BazarServer.Api
dotnet build
mkdir -p ~/run/BazarServer
cp bin/Debug/net6.0/* ~/run/BazarServer/ -r
