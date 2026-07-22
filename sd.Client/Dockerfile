# --- Build stage: compile and publish the WASM app ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BLAZOR_ENVIRONMENT=Production
WORKDIR /src

COPY sd.Client/sd.Client.csproj sd.Client/
COPY sd.Shared/sd.Shared.csproj sd.Shared/

RUN dotnet restore sd.Client/sd.Client.csproj

COPY . .

# Publish to a folder; WASM output ends up under /app/out/wwwroot
WORKDIR /src/sd.Client
RUN dotnet publish sd.Client.csproj -c $BUILD_CONFIGURATION -o /app/out /p:UseAppHost=false

# --- Runtime stage: serve static site with nginx ---
FROM nginx:1.27-alpine AS final
# Copy a minimal nginx config that handles SPA fallback
COPY sd.Client/nginx.conf /etc/nginx/conf.d/default.conf

# Copy the published static files
COPY --from=build /app/out/wwwroot /usr/share/nginx/html

# Expose HTTP (container side)
EXPOSE 80

# (Default CMD is fine: ["nginx", "-g", "daemon off;"])
