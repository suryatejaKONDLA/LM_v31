/**
 * API route constants matching backend controller routes.
 * Mirrors CITL.WebApi.Controllers route prefixes.
 * Route values derived from [Route] attributes on each controller.
 */
export const ApiRoutes = {
    // Authentication — [Route("Auth")]
    Auth: "Auth",

    // Account — [Route("Account")]
    Account: "Account",

    // Menu — [Route("[controller]")]
    Menu: "Menu",

    // Admin — [Route("[controller]")]
    AppMaster: "AppMaster",
    CompanyMaster: "CompanyMaster",
    RoleMaster: "RoleMaster",
    MailMaster: "MailMaster",

    // File — [Route("[controller]")] — reserved for future use
    FileStorage: "FileStorage",

    // Notifications — [Route("[controller]")] — reserved for future use
    Email: "Email",

    // Scheduler — [Route("[controller]")] — reserved for future use
    Scheduler: "Scheduler",
} as const;
