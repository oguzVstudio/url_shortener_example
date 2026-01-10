# URL Shortener Application

A high-performance URL shortening service built with .NET 10, featuring distributed caching, rate limiting, and asynchronous URL tracking.

## Features

- ✨ URL shortening with custom or auto-generated codes
- 🔄 Automatic redirection to original URLs
- ⏱️ Optional URL expiration
- 📊 Access tracking and analytics
- 🚀 Redis-based distributed caching with HybridCache
- 🔒 Distributed locking for code generation
- ⚡ Rate limiting to prevent abuse
- 📨 Event-driven architecture with MassTransit
- 🗄️ PostgreSQL database with Entity Framework Core

## Tech Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Core** - Minimal APIs
- **Entity Framework Core 10** - ORM with PostgreSQL
- **Redis** - Distributed caching and locking
- **MassTransit** - In-memory message bus
- **HybridCache** - Multi-level caching

## Architecture

The project follows Clean Architecture principles with the following layers:

- **Domain** - Core business entities and interfaces
- **Application** - Business logic and service implementations
- **Infrastructure** - External concerns (database, caching, messaging)
- **Host** - API endpoints and configuration

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- [Redis 6+](https://redis.io/download)

## Configuration

Update `appsettings.json` in the Host project:

```json
{
  "PostgresOptions": {
    "ConnectionString": "Server=localhost;Port=5432;Database=shorten;Username=postgres;Password=123456",
    "UseInMemory": false
  },
  "RedisOptions": {
    "Host": "localhost",
    "Port": 6379,
    "Password": "redis123"
  },
  "ShortenUrlSettings": {
    "BaseUrl": "http://localhost:5028"
  }
}
```

## Getting Started

### Option 1: Using Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd url_shortener_example
   ```

2. **Run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - PostgreSQL database on port 5432
   - Redis cache on port 6379
   - URL Shortener API on port 8080

3. **Apply database migrations**
   ```bash
   docker-compose exec app dotnet ef database update --project /src/src/Infrastructure/UrlShortener.Infrastructure --startup-project /src/src/Host/UrlShortener.Host
   ```

The API will be available at `http://localhost:8080`

**Stop the application:**
```bash
docker-compose down
```

**Stop and remove volumes:**
```bash
docker-compose down -v
```

### Option 2: Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd url_shortener_example
   ```

2. **Start required services**
   ```bash
   # Start PostgreSQL
   docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=123456 -e POSTGRES_DB=shorten postgres:16
   
   # Start Redis
   docker run -d --name redis -p 6379:6379 redis:7 redis-server --requirepass redis123
   ```

3. **Apply database migrations**
   ```bash
   cd src/Host/UrlShortener.Host
   dotnet ef database update --project ../../Infrastructure/UrlShortener.Infrastructure
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5028`

## API Endpoints

### 1. Create Short URL

Creates a shortened URL from a long URL.

**Endpoint:** `POST /api/urls/shorten`

**Request Body:**
```json
{
  "url": "https://www.example.com/very/long/url/path",
  "isExpiring": false,
  "expiresAt": null
}
```

**Response:** `200 OK`
```json
{
  "shortUrl": "http://localhost:5028/abc123",
  "alias": "abc123",
  "success": true
}
```

**Example (Local):**
```bash
curl -X POST http://localhost:5028/api/urls/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://www.example.com/very/long/url",
    "isExpiring": false
  }'
```

**Example (Docker):**
```bash
curl -X POST http://localhost:8080/api/urls/shorten \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://www.example.com/very/long/url",
    "isExpiring": false
  }'
```

### 2. Get Original URL

Retrieves the original URL for a given short code without redirecting.

**Endpoint:** `GET /api/urls/{code}`

**Parameters:**
- `code` (path) - The short URL code

**Response:** `200 OK`
```json
{
  "originalUrl": "https://www.example.com/very/long/url/path",
  "found": true
}
```

**Respons (Local):**
```bash
curl http://localhost:5028/api/urls/abc123
```

**Example (Docker):**
```bash
curl http://localhost:8080
  "originalUrl": null,
  "found": false
}
```

**Rate Limit:** 5 requests per 10 seconds

**Example:**
```bash
curl http://localhost:5028/api/urls/abc123
```

### 3. Redirect to Original URL

Redirects to the original URL and tracks the access asynchronously.

**Endpoint:** `GET /{code}`

**Parameters:**
- `code` (path) - The short URL code

**Response:** `302 Redirect` to original URL
 (Local):**
```bash
curl -L http://localhost:5028/abc123
```

**Example (Docker):**
```bash
curl -L http://localhost:8080
**Rate Limit:** 5 requests per 10 seconds

**Example:**
```bash
curl -L http://localhost:5028/abc123
```

**Tracking Information:**
- IP Address
- User Agent
- Access Timestamp

## Code Generation

The service automatically generates unique 7-character codes using:
- **Alphabet:** A-Z, a-z, 0-9 (62 characters)
- **Length:** 7 characters (default)
- **Collision handling:** Distributed locking with Redis
- **Total combinations:** 62^7 = ~3.5 trillion possible codes

## Caching Strategy

The application uses a two-level caching approach:

1. **Local Cache:** In-memory cache with 1-minute expiration
2. **Distributed Cache:** Redis cache with 5-minute expiration

This hybrid approach provides:
- Ultra-fast local lookups
- Consistent data across instances
- Reduced database load

## Rate Limiting

Fixed window rate limiting is applied to redirect and lookup endpoints:
- **Limit:** 5 requests per 10 seconds per client
- **Strategy:** Fixed window
- **Response:** HTTP 429 (Too Many Requests)

## Database Schema

### shorten_urls
- `id` (uuid) - Primary key
- `long_url` (varchar) - Original URL
- `short_url` (varchar) - Full shortened URL
- `code` (varchar) - Unique short code
- `created_on_utc` (timestamp) - Creation time
- `is_expiring` (boolean) - Expiration flag
- `expires_at` (timestamp) - Expiration time
- `attempt_count` (int) - Access count
ocker Commands

### Build Docker Image
```bash
docker build -t url-shortener:latest .
```

### Run Docker Container
```bash
docker run -d -p 8080:8080 \
  -e PostgresOptions__ConnectionString="Server=host.docker.internal;Port=5432;Database=shorten;Username=postgres;Password=123456" \
  -e RedisOptions__Host=host.docker.internal \
  -e RedisOptions__Port=6379 \
  -e RedisOptions__Password=redis123 \
  --name url-shortener url-shortener:latest
```

### View Logs
```bash
# Docker Compose
docker-compose logs -f app

# Docker Container
docker logs -f url-shortener
```

### Access Container Shell
```bash
# Docker Compose
docker-compose exec app /bin/bash

# Docker Container
docker exec -it url-shortener /bin/bash
```

## D
### short_url_tracks
- `id` (uuid) - Primary key
- `shorten_url_id` (uuid) - Foreign key to shorten_urls
- `code` (varchar) - Short code
- `ip_address` (varchar) - Client IP
- `user_agent` (varchar) - Client user agent
- `accessed_at` (timestamp) - Access time
- `created_on_utc` (timestamp) - Record creation time

## Development

### Run Tests
```bash
dotnet test
```

### Build Solution
```bash
dotnet build
```

### Create Migration
```bash
cd src/Infrastructure/UrlShortener.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../../Host/UrlShortener.Host
```

## Performance Optimizations

- ✅ `ValueTask<T>` for synchronous code generation
- ✅ `stackalloc` for temporary buffers
- ✅ String interpolation over `string.Format`
- ✅ Query projections to avoid loading unnecessary data
- ✅ HybridCache for multi-level caching
- ✅ Distributed locking for code uniqueness
- ✅ Asynchronous event-driven URL tracking

## License

MIT License - see [LICENSE](LICENSE) file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
