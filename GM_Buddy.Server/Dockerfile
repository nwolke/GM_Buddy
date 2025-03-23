# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GM_Buddy.Server/GM_Buddy.Server.csproj", "GM_Buddy.Server/"]
COPY ["GM_Buddy.Business/GM_Buddy.Business.csproj", "GM_Buddy.Business/"]
COPY ["GM_Buddy.Data/GM_Buddy.Data.csproj", "GM_Buddy.Data/"]
COPY ["GM_Buddy.Contracts/GM_Buddy.Contracts.csproj", "GM_Buddy.Contracts/"]
RUN dotnet restore "./GM_Buddy.Server/GM_Buddy.Server.csproj"
COPY . .
WORKDIR "/src/GM_Buddy.Server"
RUN dotnet build "./GM_Buddy.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GM_Buddy.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GM_Buddy.Server.dll"]