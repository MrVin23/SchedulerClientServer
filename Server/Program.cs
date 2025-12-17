using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Server.Authorization;
using Server.Database.Interfaces;
using Server.Database.Repositories;
using Server.Database.Services;
using Server.Interfaces;
using Server.Middleware;
using Server.Services;
using Server.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database Configuration
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<DatabaseContext>());

// Repository Registration
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IEventTypesRepository, EventTypesRepository>();
builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<IUserEventsRepository, UserEventsRepository>();
builder.Services.AddScoped<IEventSettingsRepository, EventSettingsRepository>();

// Seed Service Registration
builder.Services.AddScoped<ISeedService, SeedService>();

// Utility Services Registration
builder.Services.AddScoped<DuplicateChecker>();

// Controller Services Registration
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IEventSettingsService, EventSettingsService>();

// Add Authentication with Cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Will be Always in production
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Name = ".AspNetCore.AuthCookie"; // Explicit cookie name for easier debugging
    
    // Cookie lifetime settings
    // ExpireTimeSpan: How long the authentication ticket is valid (should match AuthController)
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    
    // SlidingExpiration: If true, cookie is re-issued with new expiration when >50% of time has elapsed
    // This keeps active users logged in, but inactive users will be logged out after ExpireTimeSpan
    options.SlidingExpiration = true;
    
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    
    // Handle authentication events for better security
    options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
    {
        // When cookie validation fails (expired, tampered, etc.)
        OnValidatePrincipal = context =>
        {
            // Check if the cookie has expired
            if (context.Properties.ExpiresUtc.HasValue)
            {
                var timeRemaining = context.Properties.ExpiresUtc.Value - DateTimeOffset.UtcNow;
                if (timeRemaining < TimeSpan.Zero)
                {
                    // Cookie has expired - reject the principal
                    // The cookie middleware will handle clearing the cookie
                    context.RejectPrincipal();
                }
            }
            return Task.CompletedTask;
        },
        // Return 401 instead of redirecting to login page for API calls
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

// Add Authorization with Permission-based Policies
builder.Services.AddAuthorization(options =>
{
    // Create policies based on your actual database permissions
    // Match the permission names from your role-permissions data
    options.AddPolicy("Admin", policy =>
        policy.Requirements.Add(new PermissionRequirement("Admin")));

    options.AddPolicy("ActiveUser", policy =>
        policy.Requirements.Add(new PermissionRequirement("ActiveUser")));

    options.AddPolicy("Viewer", policy =>
        policy.Requirements.Add(new PermissionRequirement("Viewer")));

    // Additional policies for future use (commented out since not in current data)
    // options.AddPolicy("CanAccessAdminPanel", policy =>
    //     policy.Requirements.Add(new PermissionRequirement("CanAccessAdminPanel")));

    // options.AddPolicy("CanCreatePosts", policy =>
    //     policy.Requirements.Add(new PermissionRequirement("CanCreatePosts")));
});

// Register the authorization handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5074", "http://localhost", "http://localhost:5173")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Required for cookies
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dotnet Authorization Server API",
        Version = "v1",
        Description = "A REST API server built with .NET 9"
    });
});

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Seed the database only if --seed argument is provided
if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
        await seedService.SeedAsync();
        Console.WriteLine("Database seeding completed!");
    }
    return; // Exit after seeding
}

// Standalone seeding option for developers
if (args.Contains("--seed-standalone"))
{
    await StandaloneSeeder.SeedDatabaseAsync(args);
    return; // Exit after seeding
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet Authorization Server API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Only use HTTPS redirection if HTTPS is configured
var httpsPort = builder.Configuration.GetValue<int?>("HTTPS_PORT");
if (httpsPort.HasValue)
{
    app.UseHttpsRedirection();
}

// Use the global exception handler
app.UseExceptionHandler();

// Use CORS (must be before UseAuthentication and UseAuthorization)
app.UseCors("AllowSpecificOrigins");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
