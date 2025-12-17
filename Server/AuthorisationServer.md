# Authorization Server - Frontend Integration Guide

This document explains how authentication, authorization, roles, and permissions work in the .NET Authorization Server, and what frontend developers need to know to integrate with it.

## 🔐 Authentication Overview

The server uses **HTTP Cookie-based authentication** with ASP.NET Core Identity. When a user logs in, an encrypted authentication cookie is set in the browser, which is automatically sent with subsequent requests.

## 📋 Table of Contents

- [Authentication Flow](#authentication-flow)
- [HTTP Cookies](#http-cookies)
- [Login Process](#login-process)
- [Logout Process](#logout-process)
- [Checking Authentication Status](#checking-authentication-status)
- [Roles and Permissions](#roles-and-permissions)
- [CORS Configuration](#cors-configuration)
- [Frontend Integration Examples](#frontend-integration-examples)
- [API Endpoints](#api-endpoints)
- [Error Handling](#error-handling)

---

## Authentication Flow

```
┌─────────────┐
│   Frontend  │
└──────┬──────┘
       │
       │ 1. POST /api/auth/login
       │    { username, password }
       │
       ▼
┌─────────────────────┐
│   AuthController    │
└──────┬──────────────┘
       │
       │ 2. Validate credentials
       │
       ▼
┌─────────────────────┐
│   UserService       │
└──────┬──────────────┘
       │
       │ 3. Create Claims
       │    - User ID
       │    - Username
       │    - Roles (multiple)
       │
       ▼
┌─────────────────────┐
│   SignInAsync       │
│   (Cookie Auth)     │
└──────┬──────────────┘
       │
       │ 4. Set HTTP Cookie
       │    (encrypted, HttpOnly)
       │
       ▼
┌─────────────────────┐
│   LoginResponse     │
│   (JSON body)       │
└──────┬──────────────┘
       │
       │ 5. Return user data
       │
       ▼
┌─────────────┐
│   Frontend  │
└─────────────┘
```

---

## HTTP Cookies

### Cookie Configuration

The authentication cookie is configured with the following settings:

| Property | Value | Description |
|---------|-------|-------------|
| **HttpOnly** | `true` | Cookie cannot be accessed via JavaScript (XSS protection) |
| **SecurePolicy** | `SameAsRequest` | Uses HTTPS if request is HTTPS (production: `Always`) |
| **SameSite** | `Lax` | Cookie sent with same-site requests and top-level navigations |
| **ExpireTimeSpan** | `1 hour` | Cookie expires after 1 hour of inactivity |
| **SlidingExpiration** | `true` | Cookie expiration resets on each request |
| **Name** | `.AspNetCore.Cookies` | Default ASP.NET Core cookie name |

### Cookie Storage

**Important**: The cookie is **HttpOnly**, meaning:
- ✅ Automatically sent with requests to the server
- ✅ Protected from XSS attacks
- ❌ **Cannot be read by JavaScript** (you cannot access it via `document.cookie`)

### Cookie Lifetime

- **Initial Expiration**: 1 hour from login
- **Sliding Expiration**: Cookie expiration extends by 1 hour on each authenticated request
- **After Logout**: Cookie is immediately deleted

---

## Login Process

### Endpoint

```
POST /api/auth/login
Content-Type: application/json
```

### Request Body

```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

### Response (Success - 200 OK)

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "id": 2,
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"],
    "roleDetails": [
      {
        "id": 2,
        "name": "Admin",
        "description": "Administrative privileges"
      }
    ]
  },
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### Response (Error - 401 Unauthorized)

```json
{
  "success": false,
  "message": "Invalid username or password",
  "errorCode": "INVALID_CREDENTIALS",
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### What Happens During Login

1. **Credentials Validation**: Server validates username and password
2. **User Data Retrieval**: Server fetches user with all associated roles
3. **Claims Creation**: Server creates claims containing:
   - `ClaimTypes.NameIdentifier` → User ID (e.g., "2")
   - `ClaimTypes.Name` → Username (e.g., "admin")
   - `ClaimTypes.Role` → Each role name (multiple claims if user has multiple roles)
4. **Cookie Creation**: Server creates encrypted HTTP-only cookie with claims
5. **Response**: Server returns user data in JSON response body

### Claims Stored in Cookie

The cookie contains encrypted claims that the server can read on subsequent requests:

| Claim Type | Value | Example |
|-----------|-------|---------|
| `NameIdentifier` | User ID | `"2"` |
| `Name` | Username | `"admin"` |
| `Role` | Role Name | `"Admin"` (can be multiple) |

**Note**: Permissions are **NOT** stored in the cookie. They are checked server-side by querying the database on each authorization check.

---

## Logout Process

### Endpoint

```
POST /api/auth/logout
```

### Request

No body required. The authentication cookie is automatically sent with the request.

### Response (Success - 200 OK)

```json
{
  "success": true,
  "message": "Logout successful",
  "data": null,
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### What Happens During Logout

1. **Cookie Deletion**: Server deletes the authentication cookie
2. **Session Invalidation**: Server invalidates the user's session
3. **Response**: Server returns success message

**Important**: After logout, the cookie is removed, and subsequent requests will be unauthenticated.

---

## Checking Authentication Status

### Endpoint

```
GET /api/auth/me
```

### Request

No body required. The authentication cookie is automatically sent with the request.

### Response (Authenticated - 200 OK)

```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": 2,
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"],
    "roleDetails": [
      {
        "id": 2,
        "name": "Admin",
        "description": "Administrative privileges"
      }
    ]
  },
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### Response (Unauthenticated - 401 Unauthorized)

```json
{
  "success": false,
  "message": "Not authenticated",
  "errorCode": "UNAUTHORIZED",
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### Usage

Call this endpoint:
- **On app initialization** to check if user is still logged in
- **After page refresh** to restore user session
- **Periodically** to verify session is still valid

---

## Roles and Permissions

### Understanding Roles vs Permissions

- **Roles**: Stored in cookie claims, returned in login response
- **Permissions**: Checked server-side via database queries, NOT in cookie

### Roles in Cookie

Roles are stored as multiple `ClaimTypes.Role` claims in the cookie. A user can have multiple roles:

```csharp
// Example: User with multiple roles
Claims:
  - NameIdentifier: "2"
  - Name: "admin"
  - Role: "Admin"
  - Role: "Moderator"  // Multiple role claims
```

### Permissions Check Flow

When a protected endpoint is called:

```
1. Request arrives with cookie
   │
   ▼
2. Server extracts User ID from cookie claim
   │
   ▼
3. Server queries database:
   Users → UserRoles → Roles → RolePermissions → Permissions
   │
   ▼
4. Server checks if user has required permission
   │
   ▼
5. Allow or deny request
```

### Authorization Policies

Endpoints can be protected with policies:

```csharp
[Authorize(Policy = "CanEditPosts")]
[HttpPut("posts/{id}")]
public async Task<ActionResult> UpdatePost(int id) { ... }
```

The server checks permissions dynamically by:
1. Extracting User ID from cookie
2. Querying database for user's roles
3. Checking if any role has the required permission
4. Allowing or denying the request

### Available Policies

Common policies defined in the server:

- `Admin`
- `Viewer`
- `CanAccessAdminPanel`
- `CanEditPosts`
- `CanDeleteUsers`
- `CanManageRoles`
- `CanViewUsers`

**Note**: Policies are checked server-side. The frontend should handle 403 Forbidden responses gracefully.

---

## CORS Configuration

### Allowed Origins

The server is configured to accept requests from:

- `http://localhost:5237`
- `http://localhost`
- `http://localhost:5173`

### CORS Settings

```csharp
AllowAnyHeader: true
AllowAnyMethod: true
AllowCredentials: true  // Required for cookies!
```

### Important: Credentials

**`AllowCredentials: true`** is required for cookies to work. Make sure your frontend includes credentials in requests:

```javascript
// Fetch API
fetch('http://localhost:5097/api/auth/login', {
  method: 'POST',
  credentials: 'include',  // Required!
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ username, password })
});
```

---

## Frontend Integration Examples

### React/TypeScript Example

```typescript
// types.ts
interface LoginRequest {
  username: string;
  password: string;
}

interface LoginResponse {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  roleDetails: Array<{
    id: number;
    name: string;
    description: string;
  }>;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  timestamp: string;
  traceId: string;
}

// authService.ts
const API_BASE_URL = 'http://localhost:5097/api';

class AuthService {
  async login(username: string, password: string): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      credentials: 'include', // Required for cookies!
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ username, password }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Login failed');
    }

    const result: ApiResponse<LoginResponse> = await response.json();
    return result.data;
  }

  async logout(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      credentials: 'include', // Required for cookies!
    });

    if (!response.ok) {
      throw new Error('Logout failed');
    }
  }

  async getCurrentUser(): Promise<LoginResponse | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/me`, {
        method: 'GET',
        credentials: 'include', // Required for cookies!
      });

      if (response.status === 401) {
        return null; // Not authenticated
      }

      if (!response.ok) {
        throw new Error('Failed to get current user');
      }

      const result: ApiResponse<LoginResponse> = await response.json();
      return result.data;
    } catch (error) {
      return null;
    }
  }

  hasRole(user: LoginResponse | null, role: string): boolean {
    return user?.roles.includes(role) ?? false;
  }

  hasAnyRole(user: LoginResponse | null, roles: string[]): boolean {
    return roles.some(role => this.hasRole(user, role));
  }
}

export const authService = new AuthService();
```

### React Hook Example

```typescript
// useAuth.ts
import { useState, useEffect } from 'react';
import { authService, LoginResponse } from './authService';

export function useAuth() {
  const [user, setUser] = useState<LoginResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check authentication status on mount
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      setLoading(true);
      const currentUser = await authService.getCurrentUser();
      setUser(currentUser);
    } catch (error) {
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  const login = async (username: string, password: string) => {
    const loggedInUser = await authService.login(username, password);
    setUser(loggedInUser);
    return loggedInUser;
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  return {
    user,
    loading,
    isAuthenticated: user !== null,
    login,
    logout,
    checkAuth,
    hasRole: (role: string) => authService.hasRole(user, role),
    hasAnyRole: (roles: string[]) => authService.hasAnyRole(user, roles),
  };
}
```

### Axios Configuration Example

```typescript
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5097/api',
  withCredentials: true, // Required for cookies!
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Vue.js Example

```javascript
// auth.js
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5097/api',
  withCredentials: true, // Required for cookies!
});

export const authService = {
  async login(username, password) {
    const response = await api.post('/auth/login', { username, password });
    return response.data.data;
  },

  async logout() {
    await api.post('/auth/logout');
  },

  async getCurrentUser() {
    try {
      const response = await api.get('/auth/me');
      return response.data.data;
    } catch (error) {
      if (error.response?.status === 401) {
        return null;
      }
      throw error;
    }
  },
};
```

---

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/login` | Login with username/password | No |
| `POST` | `/api/auth/logout` | Logout current user | Yes |
| `GET` | `/api/auth/me` | Get current authenticated user | Yes |

### Protected Endpoints

All other endpoints may require authentication. Check the endpoint documentation or handle 401/403 responses.

---

## Error Handling

### Common HTTP Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| `200` | Success | Process response data |
| `201` | Created | Resource created successfully |
| `400` | Bad Request | Validation error - check error message |
| `401` | Unauthorized | Not authenticated - redirect to login |
| `403` | Forbidden | Authenticated but lacks permission |
| `404` | Not Found | Resource not found |
| `500` | Server Error | Server error - log and show error message |

### Error Response Format

```json
{
  "success": false,
  "message": "Error message here",
  "errorCode": "ERROR_CODE",
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

### Validation Error Format

```json
{
  "success": false,
  "message": "Validation failed",
  "errorCode": "VALIDATION_ERROR",
  "validationErrors": {
    "username": ["Username is required"],
    "email": ["Email format is invalid"]
  },
  "timestamp": "2024-11-15T23:30:00Z",
  "traceId": "0HN1234567890"
}
```

---

## Best Practices

### 1. Always Include Credentials

```javascript
// ✅ Correct
fetch(url, { credentials: 'include' });

// ❌ Wrong
fetch(url); // Cookie won't be sent
```

### 2. Check Authentication on App Start

```typescript
// On app initialization
useEffect(() => {
  authService.getCurrentUser()
    .then(user => {
      if (user) {
        // User is logged in
        setUser(user);
      } else {
        // User is not logged in
        redirectToLogin();
      }
    });
}, []);
```

### 3. Handle 401 Responses

```typescript
// Global error handler
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Clear user state and redirect to login
      setUser(null);
      router.push('/login');
    }
    return Promise.reject(error);
  }
);
```

### 4. Don't Store Sensitive Data in LocalStorage

The server handles authentication via secure HTTP-only cookies. Don't store passwords or tokens in localStorage.

### 5. Check Roles Client-Side (UI Only)

```typescript
// Show/hide UI elements based on roles
{user && authService.hasRole(user, 'Admin') && (
  <AdminPanel />
)}
```

**Remember**: Client-side role checks are for UI only. Server-side permission checks are what actually protect endpoints.

---

## Security Notes

### Cookie Security

- ✅ **HttpOnly**: Cookie cannot be accessed by JavaScript (XSS protection)
- ✅ **Secure**: In production, cookie only sent over HTTPS
- ✅ **SameSite**: Prevents CSRF attacks
- ✅ **Encrypted**: Cookie contents are encrypted by ASP.NET Core

### What Frontend Should NOT Do

- ❌ Don't try to read the cookie via JavaScript (it's HttpOnly)
- ❌ Don't store authentication tokens in localStorage
- ❌ Don't send credentials in query parameters
- ❌ Don't trust client-side role checks for security

### What Frontend Should Do

- ✅ Always include `credentials: 'include'` in requests
- ✅ Handle 401/403 responses gracefully
- ✅ Check authentication status on app initialization
- ✅ Use roles for UI display only (server enforces permissions)

---

## Troubleshooting

### Cookie Not Being Sent

**Problem**: Requests return 401 even after login.

**Solutions**:
1. Ensure `credentials: 'include'` is set in fetch/axios
2. Check CORS configuration allows your origin
3. Verify cookie domain matches your frontend domain
4. Check browser console for CORS errors

### Session Expired

**Problem**: User gets logged out after 1 hour.

**Solution**: This is expected behavior. Implement automatic session refresh or redirect to login.

### CORS Errors

**Problem**: Browser blocks requests with CORS errors.

**Solutions**:
1. Ensure your frontend origin is in the allowed origins list
2. Ensure `AllowCredentials: true` is set (server-side)
3. Ensure `credentials: 'include'` is set (client-side)

---

## Summary

### Key Points for Frontend Developers

1. **Authentication**: Uses HTTP-only cookies (automatic, secure)
2. **Login**: POST to `/api/auth/login` with username/password
3. **Logout**: POST to `/api/auth/logout`
4. **Check Auth**: GET `/api/auth/me` to verify authentication
5. **Credentials**: Always include `credentials: 'include'` in requests
6. **Roles**: Returned in login response, stored in cookie claims
7. **Permissions**: Checked server-side, not in cookie
8. **Error Handling**: Handle 401 (unauthorized) and 403 (forbidden) responses

### Quick Reference

```typescript
// Login
const user = await fetch('/api/auth/login', {
  method: 'POST',
  credentials: 'include',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
}).then(r => r.json());

// Logout
await fetch('/api/auth/logout', {
  method: 'POST',
  credentials: 'include'
});

// Check Auth
const user = await fetch('/api/auth/me', {
  credentials: 'include'
}).then(r => r.json());
```

---

For more information, see the main [README.md](./README.md) file.

