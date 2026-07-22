# --- base runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# --- build / restore ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# These paths are relative to the **build context** (repo root), not the Dockerfile
COPY . .

RUN dotnet restore sd.Api/sd.Api.csproj

# Now copy the rest of the source (entire repo)


WORKDIR /src/sd.Api
RUN dotnet build sd.Api.csproj -c $BUILD_CONFIGURATION -o /app/build --no-restore

# --- publish ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/sd.Api
RUN dotnet publish sd.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# --- final ---
FROM base AS final
WORKDIR /app
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sd.Api.dll"]
