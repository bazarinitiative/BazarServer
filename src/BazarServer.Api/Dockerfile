#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/BazarServer.Api/BazarServer.Api.csproj", "src/BazarServer.Api/"]
COPY ["src/BazarServer.Infrastructure/BazarServer.Infrastructure.csproj", "src/BazarServer.Infrastructure/"]
COPY ["src/BazarServer.Entity/BazarServer.Entity.csproj", "src/BazarServer.Entity/"]
COPY ["src/Common.Utils/Common.Utils.csproj", "src/Common.Utils/"]
COPY ["src/BazarServer.Application/BazarServer.Application.csproj", "src/BazarServer.Application/"]
RUN dotnet restore "src/BazarServer.Api/BazarServer.Api.csproj"
COPY . .
WORKDIR "/src/src/BazarServer.Api"
RUN dotnet build "BazarServer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BazarServer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BazarServer.Api.dll"]