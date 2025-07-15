FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/LLMSensitiveDataGoverance.Core/LLMSensitiveDataGoverance.Core.csproj", "src/LLMSensitiveDataGoverance.Core/"]
COPY ["src/LLMSensitiveDataGoverance.AgentIntegration/LLMSensitiveDataGoverance.AgentIntegration.csproj", "src/LLMSensitiveDataGoverance.AgentIntegration/"]
COPY ["src/LLMSensitiveDataGoverance.CLI/LLMSensitiveDataGoverance.CLI.csproj", "src/LLMSensitiveDataGoverance.CLI/"]

# Restore dependencies
RUN dotnet restore "src/LLMSensitiveDataGoverance.Core/LLMSensitiveDataGoverance.Core.csproj"
RUN dotnet restore "src/LLMSensitiveDataGoverance.AgentIntegration/LLMSensitiveDataGoverance.AgentIntegration.csproj"
RUN dotnet restore "src/LLMSensitiveDataGoverance.CLI/LLMSensitiveDataGoverance.CLI.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/LLMSensitiveDataGoverance.CLI"
RUN dotnet build "LLMSensitiveDataGoverance.CLI.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "LLMSensitiveDataGoverance.CLI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories for configuration and logs
RUN mkdir -p /app/config /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "LLMSensitiveDataGoverance.CLI.dll"]