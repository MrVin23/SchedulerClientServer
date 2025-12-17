# Authorization Services Documentation

This document explains how the `SecureStorageService` and `TokenServices` work and how to use them in your Blazor application.

## Table of Contents

1. [Overview](#overview)
2. [SecureStorageService](#securestorageservice)
3. [TokenServices](#tokenservices)
4. [Usage Examples](#usage-examples)
5. [Security Considerations](#security-considerations)

---

## Overview

The authorization system consists of two main services:

- **SecureStorageService**: Provides encrypted storage for sensitive data using browser session/local storage
- **TokenServices**: Handles JWT token parsing and role token management

Both services work together to securely store and manage authentication tokens and user data in a Blazor WebAssembly application.

---

## SecureStorageService

### Purpose

The `SecureStorageService` provides a secure way to store sensitive data in the browser using encrypted session storage and local storage. It automatically encrypts data before storing it and decrypts it when retrieving it.

### Key Features

- **Encryption**: All data is encrypted using XOR cipher before storage
- **Session Storage**: Temporary storage that clears when the browser tab is closed
- **Local Storage**: Persistent storage that survives browser restarts
- **Type-Safe**: Generic methods support storing any serializable type

### Architecture

The service uses:
- **JavaScript Interop** (`IJSRuntime`) to access browser storage APIs
- **JSON Serialization** to convert objects to/from strings
- **Simple XOR Encryption** to protect stored data

### Methods

#### Session Storage Methods

**`SetAsync<T>(string key, T value)`**
- Stores encrypted data in session storage
- Data is automatically serialized to JSON and encrypted
- Cleared when browser tab closes

**`GetAsync<T>(string key)`**
- Retrieves and decrypts data from session storage
- Returns `default(T)` if key doesn't exist
- Automatically deserializes JSON back to the original type

**`RemoveAsync(string key)`**
- Removes data from session storage

#### Local Storage Methods

**`SetLocalAsync<T>(string key, T value)`**
- Stores encrypted data in local storage
- Data persists across browser sessions
- Useful for "Remember Me" functionality

**`GetLocalAsync<T>(string key)`**
- Retrieves and decrypts data from local storage
- Returns `default(T)` if key doesn't exist

**`RemoveLocalAsync(string key)`**
- Removes data from local storage

### Encryption Details

The service uses a simple XOR cipher with a hardcoded encryption key:

```csharp
private const string EncryptionKey = "YourHardcodedKeyHere";
```

**Note**: This is a basic encryption method. For production applications, consider upgrading to AES-256 encryption (see TODO comment in code).

**How it works:**
1. Converts the input text to UTF-8 bytes
2. XORs each byte with the corresponding byte from the encryption key (cycling through the key)
3. Converts the result to Base64 for storage

---

## TokenServices

### Purpose

The `TokenServices` handles JWT (JSON Web Token) parsing and provides methods to store, retrieve, and remove role tokens securely.

### Key Features

- **JWT Parsing**: Extracts claims from JWT tokens
- **Role Handling**: Properly handles roles as both single values and arrays
- **Token Storage**: Integrates with `SecureStorageService` for secure token storage
- **Base64Url Support**: Handles JWT Base64Url encoding with proper padding

### Methods

#### JWT Parsing

**`ParseClaimsFromJwt(string jwt)`**
- Parses a JWT token and extracts all claims
- Returns a collection of `Claim` objects
- Handles special cases for role claims (single value or array)
- Throws exceptions for invalid tokens

**How it works:**
1. Splits the JWT into header, payload, and signature parts
2. Decodes the Base64Url-encoded payload
3. Deserializes the JSON payload to extract claims
4. Special handling for roles (can be string or string array)
5. Converts all key-value pairs to `Claim` objects

**Example JWT Structure:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwicm9sZSI6WyJBZG1pbiIsIlVzZXIiXX0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

#### Token Storage Methods

**`StoreRoleToken(string roleToken)`**
- Stores the role token in secure session storage
- Uses the key `"roleToken"`

**`GetRoleToken()`**
- Retrieves the stored role token from secure storage
- Returns `null` if token doesn't exist

**`RemoveRoleToken()`**
- Removes the role token from secure storage
- Typically called during logout

### Base64Url Handling

JWT tokens use Base64Url encoding which may omit padding characters (`=`). The service includes a helper method `ParseBase64WithoutPadding` that automatically adds the required padding before decoding.

---

## Usage Examples

### Example 1: Storing and Retrieving User Data

```csharp
public class UserService
{
    private readonly ISecureStorageService _secureStorage;

    public UserService(ISecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    // Store user data in session storage
    public async Task SaveUserSession(User user)
    {
        await _secureStorage.SetAsync("currentUser", user);
    }

    // Retrieve user data
    public async Task<User?> GetUserSession()
    {
        return await _secureStorage.GetAsync<User>("currentUser");
    }

    // Store user preferences in local storage (persists across sessions)
    public async Task SaveUserPreferences(UserPreferences prefs)
    {
        await _secureStorage.SetLocalAsync("userPreferences", prefs);
    }
}
```

### Example 2: Handling JWT Tokens and Claims

```csharp
public class AuthenticationService
{
    private readonly ITokenServices _tokenServices;
    private readonly ISecureStorageService _secureStorage;

    public AuthenticationService(
        ITokenServices tokenServices,
        ISecureStorageService secureStorage)
    {
        _tokenServices = tokenServices;
        _secureStorage = secureStorage;
    }

    public async Task ProcessLoginResponse(string jwtToken)
    {
        // Parse claims from JWT
        var claims = _tokenServices.ParseClaimsFromJwt(jwtToken);
        
        // Extract user information
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        
        // Store the role token securely
        await _tokenServices.StoreRoleToken(jwtToken);
        
        // Store user claims
        await _secureStorage.SetAsync("userClaims", claims.ToList());
    }

    public async Task<bool> IsUserInRole(string role)
    {
        var roleToken = await _tokenServices.GetRoleToken();
        if (roleToken == null) return false;
        
        var claims = _tokenServices.ParseClaimsFromJwt(roleToken);
        return claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    public async Task Logout()
    {
        // Remove tokens
        await _tokenServices.RemoveRoleToken();
        await _secureStorage.RemoveAsync("userClaims");
    }
}
```

### Example 3: Complete Login Flow

```csharp
public async Task HandleLogin(LoginRequest request)
{
    // 1. Authenticate with API
    var loginResponse = await _authService.LoginAsync(request);
    
    if (loginResponse?.Success == true && loginResponse.Data != null)
    {
        var token = loginResponse.Data.Token;
        
        // 2. Parse and validate token
        var claims = _tokenServices.ParseClaimsFromJwt(token);
        
        // 3. Store token securely
        await _tokenServices.StoreRoleToken(token);
        
        // 4. Store additional user data if needed
        var userInfo = new
        {
            UserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
            Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
        };
        
        await _secureStorage.SetAsync("userInfo", userInfo);
        
        // 5. Navigate to authenticated area
        NavigationManager.NavigateTo("/dashboard");
    }
}
```

### Example 4: Checking Authentication Status

```csharp
public async Task<bool> IsAuthenticatedAsync()
{
    var roleToken = await _tokenServices.GetRoleToken();
    if (string.IsNullOrEmpty(roleToken)) return false;
    
    try
    {
        var claims = _tokenServices.ParseClaimsFromJwt(roleToken);
        // Optionally check token expiration claim
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim != null)
        {
            var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
            return expTime > DateTimeOffset.UtcNow;
        }
        return true;
    }
    catch
    {
        // Invalid token
        await _tokenServices.RemoveRoleToken();
        return false;
    }
}
```

### Example 5: "Remember Me" Functionality

```csharp
public async Task LoginWithRememberMe(LoginRequest request, bool rememberMe)
{
    var loginResponse = await _authService.LoginAsync(request);
    
    if (loginResponse?.Success == true && loginResponse.Data != null)
    {
        var token = loginResponse.Data.Token;
        
        if (rememberMe)
        {
            // Store in local storage (persists across sessions)
            await _secureStorage.SetLocalAsync("rememberedToken", token);
        }
        else
        {
            // Store in session storage (clears on tab close)
            await _tokenServices.StoreRoleToken(token);
        }
    }
}

public async Task CheckRememberedLogin()
{
    var rememberedToken = await _secureStorage.GetLocalAsync<string>("rememberedToken");
    if (!string.IsNullOrEmpty(rememberedToken))
    {
        // Restore session
        await _tokenServices.StoreRoleToken(rememberedToken);
        NavigationManager.NavigateTo("/dashboard");
    }
}
```

---

## Security Considerations

### Current Implementation

1. **Encryption**: Uses XOR cipher with a hardcoded key
   - **Limitation**: Not cryptographically secure for production
   - **Recommendation**: Upgrade to AES-256 encryption

2. **Storage Location**:
   - **Session Storage**: More secure, clears on tab close
   - **Local Storage**: Persists longer but more vulnerable to XSS attacks

3. **Token Handling**:
   - Tokens are encrypted before storage
   - JWT parsing validates token structure
   - No automatic token expiration checking (should be added)

### Best Practices

1. **Use Session Storage for Sensitive Data**: Prefer session storage for tokens and sensitive information
2. **Implement Token Expiration Checks**: Always validate token expiration before use
3. **Clear Storage on Logout**: Always remove tokens and sensitive data on logout
4. **Upgrade Encryption**: Replace XOR cipher with AES-256 for production
5. **HTTPS Only**: Ensure your application only runs over HTTPS in production
6. **Content Security Policy**: Implement CSP headers to prevent XSS attacks

### TODO Items (from code comments)

1. **Encryption Upgrade**: Improve encryption to AES-256 level
2. **Remember Me Feature**: Implement "Remember me" or "Stay signed in" functionality using local storage
3. **Token Refresh**: Consider implementing automatic token refresh for long-lived sessions

---

## Dependency Injection Setup

To use these services, register them in your `Program.cs`:

```csharp
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
```

Both services require `IJSRuntime`, which is automatically available in Blazor WebAssembly applications.

---

## Summary

- **SecureStorageService**: Provides encrypted storage for any serializable data using browser storage APIs
- **TokenServices**: Handles JWT parsing and role token management
- Both services work together to provide secure authentication token storage
- Session storage is recommended for sensitive data
- Local storage can be used for "Remember Me" functionality
- Current encryption is basic and should be upgraded for production use

