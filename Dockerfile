# Use the official .NET 9 SDK as the base image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY TravelRecommendation.csproj ./
RUN dotnet restore

# Copy the entire source code
COPY . ./

# Build the application in Release mode
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-build

# Use the official .NET 9 runtime as the base image for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user
RUN groupadd -r trawell && useradd -r -g trawell trawell

# Set the working directory
WORKDIR /app

# Create directory for database and logs
RUN mkdir -p /app/data /app/logs && chown -R trawell:trawell /app

# Copy the published application
COPY --from=build /app/publish .

# Copy production configuration files
COPY --from=build /app/appsettings.Production.json .

# Set ownership of the application files
RUN chown -R trawell:trawell /app

# Switch to non-root user
USER trawell

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/health/live || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "TraWell.dll"]
