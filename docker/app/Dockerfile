# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Install Node.js in build stage
RUN apk add --no-cache nodejs npm

# Copy .csproj and restore dependencies
COPY ["src/MathFlow/MathFlow.csproj", "MathFlow/"]
RUN dotnet restore "MathFlow/MathFlow.csproj"

# Copy source code
COPY src/MathFlow/ MathFlow/

# Install Node.js dependencies (production only)
WORKDIR /src/MathFlow/Infrastructure/Converters/Scripts
RUN npm ci --only=production

# Build application
WORKDIR /src/MathFlow
RUN dotnet build "MathFlow.csproj" -c Release -o /app/build

# Publish application
RUN dotnet publish "MathFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install required dependencies
RUN apk add --no-cache \
    icu-libs \
    icu-data-full \
    tzdata \
    nodejs \
    npm \
    curl \
    && rm -rf /var/cache/apk/*

# Configure environment variables for production
ENV ASPNETCORE_URLS=http://0.0.0.0:5124 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_ENVIRONMENT=Production \
    TZ=America/Sao_Paulo

# Copy published application
COPY --from=build /app/publish .

# Copy node_modules explicitly (required for Node.js scripts)
COPY --from=build /src/MathFlow/Infrastructure/Converters/Scripts/node_modules ./Infrastructure/Converters/Scripts/node_modules

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup && \
    chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5124

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5124 || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MathFlow.dll"]
