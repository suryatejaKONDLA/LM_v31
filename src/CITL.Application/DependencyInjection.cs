using CITL.Application.Core.Account;
using CITL.Application.Core.Account.Menus;
using CITL.Application.Core.Account.Theme;
using CITL.Application.Core.Admin.AppMaster;
using CITL.Application.Core.Admin.BranchMaster;
using CITL.Application.Core.Admin.CompanyMaster;
using CITL.Application.Core.Admin.FinYearMaster;
using CITL.Application.Core.Admin.LoginMaster;
using CITL.Application.Core.Admin.MailMaster;
using CITL.Application.Core.Admin.Mappings.Mapping;
using CITL.Application.Core.Admin.Mappings.RoleMenuMapping;
using CITL.Application.Core.Admin.RoleMaster;
using CITL.Application.Core.Authentication;
using CITL.Application.Core.Common.GenderMaster;
using CITL.Application.Core.FileStorage;
using CITL.Application.Core.Notifications.Email;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CITL.Application;

/// <summary>
/// Registers Application layer services into the DI container.
/// Called from <c>Program.cs</c> in the WebApi project.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services: application services, FluentValidation validators, etc.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation — auto-register all validators from this assembly
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();

        // Core / Account
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IThemeService, ThemeService>();

        // Core / Admin
        services.AddScoped<IAppMasterService, AppMasterService>();
        services.AddScoped<ICompanyMasterService, CompanyMasterService>();
        services.AddScoped<IRoleMasterService, RoleMasterService>();
        services.AddScoped<IBranchMasterService, BranchMasterService>();
        services.AddScoped<IMailMasterService, MailMasterService>();
        services.AddScoped<IFinYearMasterService, FinYearMasterService>();
        services.AddScoped<ILoginMasterService, LoginMasterService>();
        services.AddScoped<IGenderMasterService, GenderMasterService>();
        services.AddScoped<IRoleMenuMappingService, RoleMenuMappingService>();
        services.AddScoped<IMappingsService, MappingsService>();

        // Core / Authentication
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IIdentityVerificationService, IdentityVerificationService>();

        // Core / Notifications
        services.AddScoped<IEmailService, EmailService>();

        // Core / File Storage
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
