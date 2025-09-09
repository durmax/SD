FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY sd.Api/sd.Api.csproj sd.Api/
COPY sd.Shared/sd.Shared.csproj sd.Shared/
COPY sd.Infrastructure/sd.Infrastructure.csproj sd.Infrastructure/
COPY sd.Application/sd.Application.csproj sd.Application/
RUN dotnet restore sd.Api/sd.Api.csproj

COPY . .
WORKDIR /src/sd.Api
RUN dotnet build sd.Api.csproj -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
WORKDIR /src/sd.Api
RUN dotnet publish sd.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-build

FROM base AS final
WORKDIR /app
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sd.Api.dll"]
