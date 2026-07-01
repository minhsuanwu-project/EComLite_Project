# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first (layer cache)
COPY EComLite.Web/EComLite.Web.csproj EComLite.Web/
RUN dotnet restore EComLite.Web/EComLite.Web.csproj

# Copy the rest of the source code
COPY EComLite.Web/ EComLite.Web/

# Publish release build
RUN dotnet publish EComLite.Web/EComLite.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EComLite.Web.dll"]
