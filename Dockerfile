# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies first (cached layer if .csproj unchanged)
COPY TaskFlowBackend.csproj ./
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish TaskFlowBackend.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# Render sets PORT at runtime; default to 8080 for local docker run
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["./entrypoint.sh"]
