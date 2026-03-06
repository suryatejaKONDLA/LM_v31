using System.Collections.Frozen;
using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Account;
using CITL.Application.Core.Account.Menus;
using CITL.Application.Core.Account.Theme;
using CITL.Application.Core.Admin.AppMaster;
using CITL.Application.Core.Admin.CompanyMaster;
using CITL.Application.Core.Admin.MailMaster;
using CITL.Application.Core.Admin.RoleMaster;
using CITL.Application.Core.Authentication;
using CITL.Application.Core.FileStorage;
using CITL.Application.Core.Notifications.Email;
using CITL.Application.Core.Scheduler;
using CITL.Infrastructure.Authentication;
using CITL.Infrastructure.Caching;
using CITL.Infrastructure.Core.Account;
using CITL.Infrastructure.Core.Admin;
using CITL.Infrastructure.Core.Authentication;
using CITL.Infrastructure.Core.FileStorage;
using CITL.Infrastructure.Core.Notifications.Email;
using CITL.Infrastructure.Core.Scheduler;
using CITL.Infrastructure.Core.Scheduler.Jobs;
using CITL.Infrastructure.HealthChecks;
using CITL.Infrastructure.MultiTenancy;
using CITL.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CITL.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services into the DI container.
/// Called from <c>Program.cs</c> in the WebApi project.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services: multi-tenancy, database connections, caching, etc.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dapper: auto-map SQL column names with underscores to PascalCase C# properties
        // e.g. APP_Code → AppCode, APP_Header1 → AppHeader1
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Multi-tenancy
        services.Configure<TenantSettings>(
            configuration.GetSection(TenantSettings.SectionName));

        services.AddSingleton<ITenantRegistry, TenantRegistry>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IDbExecutor, DbExecutor>();

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<IAppMasterRepository, AppMasterRepository>();
        services.AddScoped<ICompanyMasterRepository, CompanyMasterRepository>();
        services.AddScoped<IRoleMasterRepository, RoleMasterRepository>();
        services.AddScoped<IMailMasterRepository, MailMasterRepository>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IIdentityVerificationRepository, IdentityVerificationRepository>();

        // Notifications
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IBackgroundEmailDispatcher, BackgroundEmailDispatcher>();

        // Authentication — CAPTCHA + JWT
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();

        // Caching — L1 MemoryCache + L2 Redis
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "CITL:";
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        // Scheduler — Quartz.NET with RAMJobStore
        services.AddQuartz(q =>
        {
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddScoped<ISchedulerRepository, SchedulerRepository>();
        services.AddScoped<IScheduledJob, EmailSchedulerJob>();

        services.AddSingleton<SchedulerHostedService>();
        services.AddSingleton<ISchedulerAdmin>(sp => sp.GetRequiredService<SchedulerHostedService>());
        services.AddHostedService(sp => sp.GetRequiredService<SchedulerHostedService>());

        // File Storage — provider selected via config + upload settings
        services.Configure<FileStorageSettings>(
            configuration.GetSection(FileStorageSettings.SectionName));

        // Process memory health check settings
        services.Configure<ProcessMemoryHealthCheckSettings>(
            configuration.GetSection(ProcessMemoryHealthCheckSettings.SectionName));

        var fileStorageSection = configuration.GetSection(FileStorageSettings.SectionName);
        var rawExtensions = fileStorageSection.GetSection("AllowedExtensions").Get<string[]>() ?? [];
        var normalizedExtensions = rawExtensions
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : $".{e.ToLowerInvariant()}")
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        services.AddSingleton(new FileStorageUploadSettings
        {
            AllowedExtensions = normalizedExtensions
        });

        var storageProvider = fileStorageSection["Provider"] ?? "Local";

        if (string.Equals(storageProvider, "R2", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IFileStorageProvider, R2FileStorageProvider>();
        }
        else
        {
            services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();
        }

        return services;
    }
}
