# ===========================================
# GM_Buddy.Server - Local Development Dockerfile
# ===========================================
# Multi-stage build for ASP.NET Core backend

# Base stage for Visual Studio debugging
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Copy project files
COPY ["GM_Buddy.Server/GM_Buddy.Server.csproj", "GM_Buddy.Server/"]
COPY ["GM_Buddy.Business/GM_Buddy.Business.csproj", "GM_Buddy.Business/"]
COPY ["GM_Buddy.Data/GM_Buddy.Data.csproj", "GM_Buddy.Data/"]
COPY ["GM_Buddy.Contracts/GM_Buddy.Contracts.csproj", "GM_Buddy.Contracts/"]

# Restore dependencies
RUN dotnet restore "GM_Buddy.Server/GM_Buddy.Server.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/GM_Buddy.Server"
RUN dotnet build "GM_Buddy.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "GM_Buddy.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "GM_Buddy.Server.dll"]
