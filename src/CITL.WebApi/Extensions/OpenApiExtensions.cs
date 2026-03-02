using System.Globalization;
using CITL.WebApi.Attributes;
using CITL.WebApi.Configuration;
using CITL.WebApi.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace CITL.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring OpenAPI documentation with Swagger UI and Scalar.
/// </summary>
internal static class OpenApiExtensions
{
    /// <summary>
    /// Registers OpenAPI documents — one per module group when multi-group is enabled,
    /// or a single combined document otherwise.
    /// </summary>
    internal static IServiceCollection AddOpenApiDocuments(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(ApiDocumentationSettings.SectionName)
            .Get<ApiDocumentationSettings>() ?? new ApiDocumentationSettings();

        if (settings.UseMultipleApiGroups)
        {
            foreach (var groupKey in ApiGroupConstants.All)
            {
                services.AddOpenApi(groupKey, options =>
                {
                    ConfigureOpenApiDocument(options, settings, groupKey, ToDisplayName(groupKey));
                });
            }
        }
        else
        {
            services.AddOpenApi("all", options =>
            {
                ConfigureOpenApiDocument(options, settings, documentName: null, "All APIs");
            });
        }

        return services;
    }

    /// <summary>
    /// Maps OpenAPI JSON endpoints and enables Swagger UI with per-group document switching.
    /// </summary>
    internal static WebApplication UseSwaggerDocumentation(
        this WebApplication app,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(ApiDocumentationSettings.SectionName)
            .Get<ApiDocumentationSettings>() ?? new ApiDocumentationSettings();

        // Bypass tenant middleware for OpenAPI JSON spec endpoints
        app.MapOpenApi().BypassTenant();

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = settings.Title;
            options.DefaultModelsExpandDepth(-1);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnablePersistAuthorization();

            if (settings.UseMultipleApiGroups)
            {
                foreach (var groupKey in ApiGroupConstants.All)
                {
                    options.SwaggerEndpoint(
                        string.Format(CultureInfo.InvariantCulture, "/openapi/{0}.json", groupKey),
                        ToDisplayName(groupKey));
                }
            }
            else
            {
                options.SwaggerEndpoint("/openapi/all.json", "All APIs");
            }
        });

        return app;
    }

    /// <summary>
    /// Maps the Scalar API reference endpoint with a modern theme.
    /// </summary>
    internal static WebApplication UseScalarDocumentation(
        this WebApplication app,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(ApiDocumentationSettings.SectionName)
            .Get<ApiDocumentationSettings>() ?? new ApiDocumentationSettings();

        app.MapScalarApiReference("/scalar", options =>
        {
            options
                .WithTitle(settings.Title)
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                .WithDotNetFlag()
                .EnableDarkMode()
                .EnablePersistentAuthentication();

            options.HideModels = true;

            if (settings.UseMultipleApiGroups)
            {
                foreach (var key in ApiGroupConstants.All)
                {
                    options.AddDocument(key, ToDisplayName(key));
                }
            }
            else
            {
                options.AddDocument("all", settings.Title);
            }
        }).BypassTenant();

        return app;
    }

    private static void ConfigureOpenApiDocument(
        OpenApiOptions options,
        ApiDocumentationSettings settings,
        string? documentName,
        string groupDisplayName)
    {
        // Filter endpoints by their ApiExplorerSettings.GroupName
        if (documentName is not null)
        {
            options.ShouldInclude = description =>
                string.Equals(description.GroupName, documentName, StringComparison.OrdinalIgnoreCase);
        }

        // Document info transformer
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new()
            {
                Title = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} — {1}",
                    settings.Title,
                    groupDisplayName),
                Version = settings.Version,
                Description = settings.Description,
                Contact = BuildContact(settings)
            };

            return Task.CompletedTask;
        });

        // Bearer security scheme transformer
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new();

            if (document.Components.SecuritySchemes is null)
            {
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
            }

            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1Ni...",
            };

            // Global security requirement — all endpoints require Bearer by default.
            // [AllowAnonymous] endpoints are overridden below in the operation transformer.
            var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);
            document.Security ??= [];
            document.Security.Add(new()
            {
                [schemeRef] = []
            });

            return Task.CompletedTask;
        });

        // Override security for [AllowAnonymous] endpoints — clears the global requirement
        // so Swagger UI shows them without a lock icon.
        options.AddOperationTransformer((operation, context, _) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            if (metadata.OfType<AllowAnonymousAttribute>().Any())
            {
                operation.Security = [];
            }

            return Task.CompletedTask;
        });

        // Tenant header operation transformer
        options.AddOperationTransformer((operation, _, _) =>
        {
            operation.Parameters ??= [];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Id",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Tenant identifier for multi-tenant routing.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-Id",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional correlation identifier for distributed tracing. "
                    + "If omitted, the server generates one automatically. "
                    + "Returned in the response header for client reference.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });

            return Task.CompletedTask;
        });

        // Order tags alphabetically and add descriptions for API groups
        options.AddDocumentTransformer((document, _, _) =>
        {
            if (document.Tags is null)
            {
                return Task.CompletedTask;
            }

            // Ensure descriptive tags exist for all groups
            var tagDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Account"] = "User profile management — view and update personal information, change password.",
                ["AppMaster"] = "Application master data — system-wide configuration and module settings.",
                ["Authentication"] = "Login, logout, token refresh, CAPTCHA, forgot password, and email verification.",
                ["Email"] = "Email dispatch — send transactional emails and manage email templates.",
                ["FileStorage"] = "File upload, download, listing, deletion, and signed URL generation.",
                ["MailMaster"] = "Email template management — CRUD operations for mail templates.",
                ["RoleMaster"] = "Role management — create, update, delete, and list user roles.",
                ["Scheduler"] = "Job scheduler management — view status, pause, resume, and trigger jobs."
            };

            // Add descriptions to existing tags or create new ones
            foreach (var (tagName, description) in tagDescriptions)
            {
                var existing = document.Tags.FirstOrDefault(
                    t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    existing.Description ??= description;
                }
                else
                {
                    document.Tags.Add(new OpenApiTag { Name = tagName, Description = description });
                }
            }

            var ordered = document.Tags.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
            document.Tags.Clear();
            foreach (var tag in ordered)
            {
                document.Tags.Add(tag);
            }

            return Task.CompletedTask;
        });

        // Add global 401/403 response definitions for authenticated endpoints.
        // Saves every controller from repeating these on each action.
        options.AddOperationTransformer((operation, context, _) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            var isAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

            if (!isAnonymous && operation.Responses is not null)
            {
                operation.Responses.TryAdd("401", new OpenApiResponse
                {
                    Description = "Unauthorized — missing or invalid JWT token."
                });

                operation.Responses.TryAdd("403", new OpenApiResponse
                {
                    Description = "Forbidden — authenticated but insufficient permissions."
                });
            }

            return Task.CompletedTask;
        });

        // Make the "Data" property in ApiResponse<T> schemas show the concrete type
        // instead of oneOf [null, T]. This gives Swagger UI a proper example preview.
        options.AddDocumentTransformer((document, _, _) =>
        {
            if (document.Components?.Schemas is null)
            {
                return Task.CompletedTask;
            }

            foreach (var (_, schema) in document.Components.Schemas)
            {
                if (schema.Properties is null
                    || !schema.Properties.TryGetValue("Data", out var dataProp)
                    || dataProp.OneOf is not { Count: 2 })
                {
                    continue;
                }

                var concreteSchema = dataProp.OneOf
                    .FirstOrDefault(s => s.Type != JsonSchemaType.Null);

                if (concreteSchema is not null)
                {
                    schema.Properties["Data"] = concreteSchema;
                }
            }

            return Task.CompletedTask;
        });
    }

    private static OpenApiContact? BuildContact(ApiDocumentationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ContactName)
            && string.IsNullOrWhiteSpace(settings.ContactEmail))
        {
            return null;
        }

        var contact = new OpenApiContact
        {
            Name = settings.ContactName,
            Email = settings.ContactEmail
        };

        if (Uri.TryCreate(settings.ContactUrl, UriKind.Absolute, out var uri))
        {
            contact.Url = uri;
        }

        return contact;
    }

    /// <summary>
    /// Derives a display name from a kebab-case group ID.
    /// e.g. "point-of-sale" → "Point Of Sale", "authentication" → "Authentication"
    /// </summary>
    private static string ToDisplayName(string groupId) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(groupId.Replace('-', ' '));
}
