# syntax=docker/dockerfile:1

# --- Build stage: restore and publish the API ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files first so dependency restore is cached across source changes.
COPY src/Redington.ProbabilityCalculator.Domain/*.csproj src/Redington.ProbabilityCalculator.Domain/
COPY src/Redington.ProbabilityCalculator.Application/*.csproj src/Redington.ProbabilityCalculator.Application/
COPY src/Redington.ProbabilityCalculator.Api/*.csproj src/Redington.ProbabilityCalculator.Api/
RUN dotnet restore src/Redington.ProbabilityCalculator.Api/Redington.ProbabilityCalculator.Api.csproj

# Copy the remaining source and publish a framework-dependent build.
COPY src/ src/
RUN dotnet publish src/Redington.ProbabilityCalculator.Api/Redington.ProbabilityCalculator.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage: minimal ASP.NET runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl is used by the container health check; keep the layer small.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Listen on a fixed container port; the host port is mapped in docker-compose.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Redington.ProbabilityCalculator.Api.dll"]
