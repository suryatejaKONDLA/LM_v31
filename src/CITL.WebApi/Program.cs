using CITL.Application;
using CITL.Application.Common.Hubs;
using CITL.Application.Common.Interfaces;
using CITL.Infrastructure;
using CITL.Infrastructure.Authentication;
using CITL.WebApi.Attributes;
using CITL.WebApi.Configuration;
using CITL.WebApi.Extensions;
using CITL.WebApi.Filters;
using CITL.WebApi.Hubs;
using CITL.WebApi.Middleware;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// ---------------------------------------------------------------------------
// Bootstrap Serilog early so startup errors are captured
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Service registration
    // -----------------------------------------------------------------------

    // Serilog — structured logging with enrichers (Console sink + OTLP sink for Grafana Loki)
    builder.AddSerilog();

    // OpenTelemetry — tracing (Tempo) + metrics (Prometheus) via OTLP to Grafana LGTM stack
    builder.Services.AddTelemetry(builder.Configuration);

    // Infrastructure layer (multi-tenancy, DB connections, repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Application layer (services, validators, etc.)
    builder.Services.AddApplication();

    // SignalR — real-time hubs (PingHub, NotificationHub)
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<INotificationSender, SignalRNotificationSender>();
    builder.Services.AddSingleton<IHubConnectionTracker, HubConnectionTracker>();

    // Health checks — SQL Server, Redis, R2, Disk Space, Quartz, Grafana, OTLP Collector, SignalR
    builder.Services.AddInfrastructureHealthChecks()
        .AddCheck<CITL.WebApi.HealthChecks.SignalRHealthCheck>(
            "SignalR",
            tags: ["signalr"]);

    // Controllers — global filters apply to every action
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<RequestIdResultFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Use PascalCase for all JSON property names (no naming transformation)
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

    // Override the default Problem Details factory so model-binding / form-reading
    // failures (e.g. "Request body too large") return our ApiResponse envelope.
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Check for "request body too large" buried in ModelState
            var bodyTooLargeEntry = context.ModelState
                .SelectMany(kvp => kvp.Value?.Errors ?? [])
                .FirstOrDefault(e => e.ErrorMessage.Contains("Request body too large", StringComparison.OrdinalIgnoreCase)
                                  || e.Exception?.Message.Contains("Request body too large", StringComparison.OrdinalIgnoreCase) == true);

            if (bodyTooLargeEntry is not null)
            {
                var msg = GlobalExceptionMiddleware.FormatFileSizeMessage(bodyTooLargeEntry.ErrorMessage);
                var response = ApiResponse.Error(msg);
                response.RequestId = context.HttpContext.TraceIdentifier;
                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            }

            // Standard validation errors → ApiValidationResponse
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .Select(kvp => new FieldError
                {
                    Field = kvp.Key,
                    Messages = [.. kvp.Value!.Errors.Select(e => e.ErrorMessage)]
                })
                .ToList();

            var validationResponse = ApiValidationResponse.Create(errors);
            validationResponse.RequestId = context.HttpContext.TraceIdentifier;
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(validationResponse);
        };
    });

    // OpenAPI documents — one per module group
    builder.Services.AddOpenApiDocuments(builder.Configuration);

    // CORS — origins configured in appsettings.json per environment
    var corsSettings = builder.Configuration
        .GetSection(CorsSettings.SectionName)
        .Get<CorsSettings>() ?? new CorsSettings();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsSettings.PolicyName, policy =>
        {
            if (corsSettings.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsSettings.AllowedOrigins);
            }

            if (corsSettings.AllowedMethods.Length > 0)
            {
                policy.WithMethods(corsSettings.AllowedMethods);
            }
            else
            {
                policy.AllowAnyMethod();
            }

            if (corsSettings.AllowedHeaders.Length > 0)
            {
                policy.WithHeaders(corsSettings.AllowedHeaders);
            }
            else
            {
                policy.AllowAnyHeader();
            }

            if (corsSettings.ExposedHeaders.Length > 0)
            {
                policy.WithExposedHeaders(corsSettings.ExposedHeaders);
            }

            if (corsSettings.AllowCredentials)
            {
                policy.AllowCredentials();
            }
        });
    });

    // JWT Authentication
    var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
    var jwtSettings = jwtSection.Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT configuration section is missing.");

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var keyBytes = Convert.FromBase64String(jwtSettings.SecretKey);
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true
            };

            // SignalR WebSocket connections cannot send Authorization headers.
            // The client sends the JWT via ?access_token= query string instead.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;

                    if (path.StartsWithSegments("/Hubs", StringComparison.OrdinalIgnoreCase))
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // -----------------------------------------------------------------------
    // Middleware pipeline (order matters)
    // -----------------------------------------------------------------------

    // 1. Correlation ID — generates/reads X-Correlation-Id, overrides TraceIdentifier,
    //    adds response header, pushes to log scope. Must be FIRST.
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. CORS — must be early so preflight OPTIONS requests are handled before auth/tenant.
    app.UseCors(CorsSettings.PolicyName);

    // 3. Request logging — Stopwatch + structured log for start/end of every request.
    //    Records custom metrics (count, duration, errors).
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 4. Global exception handler — catches everything, returns structured JSON
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 5. API Documentation — must be BEFORE tenant middleware so Swagger/Scalar
    //    static files and OpenAPI JSON endpoints bypass X-Tenant-Id.
    app.UseSwaggerDocumentation(builder.Configuration);
    app.UseScalarDocumentation(builder.Configuration);

    // 6. Tenant resolution — reads X-Tenant-Id header, resolves DB via TenantRegistry
    //    Runs BEFORE auth so login endpoints have tenant context
    app.UseMiddleware<TenantResolutionMiddleware>();

    // 7. Authentication — validates JWT Bearer tokens
    app.UseAuthentication();
    app.UseAuthorization();

    // 8. Tenant guard — cross-validates JWT tenant_id claim vs header (prevents spoofing)
    app.UseMiddleware<TenantGuardMiddleware>();

    // -----------------------------------------------------------------------
    // Endpoints
    // -----------------------------------------------------------------------

    app.MapControllers();

    // SignalR hubs
    app.MapHub<PingHub>("/Hubs/Ping").BypassTenant();
    app.MapHub<NotificationHub>("/Hubs/Notifications");

    // Register hub descriptors for dynamic discovery and health tracking
    var hubTracker = app.Services.GetRequiredService<IHubConnectionTracker>();
    HubRegistration.SeedAll(hubTracker);

    // Health checks — comprehensive system health with structured JSON for frontend.
    // Bypasses tenant resolution and authentication: infrastructure-level endpoint.
    app.MapHealthChecks("/Health", new()
    {
        ResponseWriter = CITL.WebApi.HealthChecks.HealthCheckResponseWriter.WriteAsync
    })
    .BypassTenant()
    .AllowAnonymous();

    // Liveness probe — lightweight check for orchestrators (K8s, Docker).
    // Only verifies the process is alive, no dependency checks.
    app.MapGet("/Health/Live", () => Results.Ok(new { Status = "Alive" }))
        .BypassTenant()
        .AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// End of Program.cs
