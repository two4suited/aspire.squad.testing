# API Documentation

## Overview

The Dog Teams API provides RESTful endpoints for managing teams, owners, dogs, and user authentication. All endpoints return JSON responses and support JWT Bearer authentication for protected routes.

## Authentication

### Register
Create a new user account.

```
POST /api/auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}

Response (201 Created):
{
  "id": "uuid",
  "email": "john@example.com",
  "name": "John Doe",
  "token": "eyJhbGc...",
  "teamId": "uuid"
}
```

### Login
Authenticate and receive access token.

```
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}

Response (200 OK):
{
  "id": "uuid",
  "email": "john@example.com",
  "name": "John Doe",
  "token": "eyJhbGc...",
  "teamId": "uuid"
}
```

### Refresh Token
Get a new access token using refresh token.

```
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh-token-value"
}

Response (200 OK):
{
  "token": "new-access-token",
  "refreshToken": "new-refresh-token"
}
```

### Get Current User
Get authenticated user information.

```
GET /api/auth/me
Authorization: Bearer {token}

Response (200 OK):
{
  "id": "uuid",
  "email": "john@example.com",
  "name": "John Doe",
  "teamId": "uuid"
}
```

### Logout
Invalidate refresh token.

```
POST /api/auth/logout
Authorization: Bearer {token}

Response (200 OK)
```

## Teams

### List Teams
Get all teams for authenticated user.

```
GET /api/teams
Authorization: Bearer {token}

Response (200 OK):
[
  {
    "id": "uuid",
    "name": "City Dog Club",
    "description": "Local dog team",
    "createdAt": "2026-03-07T10:00:00Z"
  }
]
```

### Get Team
Get specific team by ID.

```
GET /api/teams/{id}
Authorization: Bearer {token}

Response (200 OK):
{
  "id": "uuid",
  "name": "City Dog Club",
  "description": "Local dog team",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Create Team
Create a new team.

```
POST /api/teams
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "City Dog Club",
  "description": "Local dog team"
}

Response (201 Created):
{
  "id": "uuid",
  "name": "City Dog Club",
  "description": "Local dog team",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Update Team
Update team information.

```
PUT /api/teams/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Updated Team Name",
  "description": "Updated description"
}

Response (200 OK):
{
  "id": "uuid",
  "name": "Updated Team Name",
  "description": "Updated description",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Delete Team
Delete a team and all associated data.

```
DELETE /api/teams/{id}
Authorization: Bearer {token}

Response (204 No Content)
```

## Owners

### List Owners
Get all owners in a team.

```
GET /api/owners?teamId={teamId}
Authorization: Bearer {token}

Response (200 OK):
[
  {
    "id": "uuid",
    "teamId": "uuid",
    "userId": "uuid",
    "name": "Jane Smith",
    "email": "jane@example.com",
    "dogs": [],
    "createdAt": "2026-03-07T10:00:00Z"
  }
]
```

### Get Owner
Get specific owner by ID.

```
GET /api/owners/{id}
Authorization: Bearer {token}

Response (200 OK):
{
  "id": "uuid",
  "teamId": "uuid",
  "userId": "uuid",
  "name": "Jane Smith",
  "email": "jane@example.com",
  "dogs": [],
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Create Owner
Add owner to a team.

```
POST /api/owners
Authorization: Bearer {token}
Content-Type: application/json

{
  "teamId": "uuid",
  "name": "Jane Smith",
  "email": "jane@example.com"
}

Response (201 Created):
{
  "id": "uuid",
  "teamId": "uuid",
  "userId": "uuid",
  "name": "Jane Smith",
  "email": "jane@example.com",
  "dogs": [],
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Update Owner
Update owner information.

```
PUT /api/owners/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Jane Doe",
  "email": "jane.doe@example.com"
}

Response (200 OK):
{
  "id": "uuid",
  "teamId": "uuid",
  "userId": "uuid",
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "dogs": [],
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Delete Owner
Delete an owner and their dogs.

```
DELETE /api/owners/{id}
Authorization: Bearer {token}

Response (204 No Content)
```

## Dogs

### List Dogs
Get all dogs, optionally filtered by owner.

```
GET /api/dogs?ownerId={ownerId}
Authorization: Bearer {token}

Response (200 OK):
[
  {
    "id": "uuid",
    "ownerId": "uuid",
    "teamId": "uuid",
    "name": "Max",
    "breed": "Golden Retriever",
    "dateOfBirth": "2020-03-15",
    "createdAt": "2026-03-07T10:00:00Z"
  }
]
```

### Get Dog
Get specific dog by ID.

```
GET /api/dogs/{id}
Authorization: Bearer {token}

Response (200 OK):
{
  "id": "uuid",
  "ownerId": "uuid",
  "teamId": "uuid",
  "name": "Max",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Create Dog
Add a dog to an owner.

```
POST /api/dogs
Authorization: Bearer {token}
Content-Type: application/json

{
  "ownerId": "uuid",
  "teamId": "uuid",
  "name": "Max",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15"
}

Response (201 Created):
{
  "id": "uuid",
  "ownerId": "uuid",
  "teamId": "uuid",
  "name": "Max",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Update Dog
Update dog information.

```
PUT /api/dogs/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Maxwell",
  "breed": "Labrador Retriever"
}

Response (200 OK):
{
  "id": "uuid",
  "ownerId": "uuid",
  "teamId": "uuid",
  "name": "Maxwell",
  "breed": "Labrador Retriever",
  "dateOfBirth": "2020-03-15",
  "createdAt": "2026-03-07T10:00:00Z"
}
```

### Delete Dog
Delete a dog.

```
DELETE /api/dogs/{id}
Authorization: Bearer {token}

Response (204 No Content)
```

## Error Responses

All errors return appropriate HTTP status codes with error details:

```json
{
  "error": "Unauthorized",
  "message": "Invalid or expired token",
  "statusCode": 401
}
```

### Common Status Codes
- **200** - OK
- **201** - Created
- **204** - No Content
- **400** - Bad Request (validation error)
- **401** - Unauthorized (authentication required)
- **403** - Forbidden (insufficient permissions)
- **404** - Not Found
- **500** - Server Error

## Rate Limiting

No rate limiting currently implemented. Production deployment should add:
- 100 requests per minute per user
- 1000 requests per minute per IP

## Caching

GET endpoints support caching with following TTLs:
- Teams: 10 minutes
- Owners: 5 minutes
- Dogs: 5 minutes

Caching is automatically invalidated on CREATE, UPDATE, DELETE operations.

## Pagination

Currently not implemented. Future versions will support:
- `?page=1&pageSize=20` query parameters
- Response headers: `X-Total-Count`, `X-Page-Count`

## Versioning

Current API version: v1 (no version prefix required)

Future versions will use `/api/v2/`, `/api/v3/` etc.
