# Caching Implementation

## Overview

This document describes the caching implementation for the HackOMania Event Platform API.

## Cached Endpoints

The following endpoints have been optimized with caching:

### Participant Hackathon List
- **Endpoint**: `GET /participants/hackathons`
- **Cache Key**: `hackathon:participants:list`
- **TTL**: 5 minutes
- **Description**: Lists all published hackathons for public access

### Organizer Hackathon List
- **Endpoint**: `GET /organizers/hackathons`
- **Cache Key**: `hackathon:organizers:list:{userId}:v{version}`
- **TTL**: 5 minutes
- **Description**: Lists hackathons accessible to the authenticated organizer

## Cache Invalidation

The cache is automatically invalidated when:

1. **Creating a new hackathon** (`POST /organizers/hackathons`)
   - Invalidates participant list cache
   - Increments cache version for organizer lists

2. **Updating a hackathon** (`PATCH /organizers/hackathons/{id}`)
   - Invalidates participant list cache
   - Increments cache version for organizer lists

## Implementation Details

### Cache Service

The `HackathonCacheService` provides:
- **Versioned caching**: Organizer list caches use a version number to enable bulk invalidation
- **Distributed cache backend**: Uses Redis if configured, falls back to in-memory cache
- **JSON serialization**: All cached data is serialized using System.Text.Json

### Cache Version Strategy

To invalidate all organizer caches without knowing all user IDs:
1. A global version number is stored in the cache
2. Cache keys include the version number (e.g., `hackathon:organizers:list:{userId}:v1`)
3. When invalidating, the version is incremented (v1 → v2)
4. All existing cached entries with the old version become stale and are not retrieved
5. New cache entries use the new version number

### Configuration

Cache configuration is defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "cache": "redis-connection-string"
  }
}
```

If no Redis connection string is provided, the system uses in-memory caching.

## Benefits

1. **Reduced database load**: Frequently accessed hackathon lists are served from cache
2. **Improved response times**: Cached responses are faster than database queries
3. **Automatic invalidation**: Cache is invalidated when data changes
4. **Scalability**: Redis-based caching supports distributed deployments

## Future Enhancements

Potential improvements:
- Add caching for individual hackathon GET requests
- Implement cache warming on startup
- Add cache statistics and monitoring
- Consider caching hackathon-related entities (challenges, teams, etc.)
