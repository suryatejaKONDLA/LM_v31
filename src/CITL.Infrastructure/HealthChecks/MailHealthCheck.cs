using CITL.Application.Common.Interfaces;
using Dapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks SMTP connectivity and authentication for every tenant's default mail configuration.
/// Loops through all tenants, queries each DB for the active default SMTP config, and authenticates.
/// </summary>
internal sealed class MailHealthCheck(
    ITenantRegistry tenantRegistry,
    IOptions<MultiTenancy.TenantSettings> options) : IHealthCheck
{
    private const string SmtpConfigSql = """
        SELECT TOP 1
            Mail_From_Address AS MailFromAddress,
            Mail_From_Password AS MailFromPassword,
            Mail_Host AS MailHost,
            Mail_Port AS MailPort,
            Mail_SSL_Enabled AS MailSslEnabled
        FROM citl_sys.Mail_Master
        WHERE Mail_Is_Default = 1 AND Mail_Is_Active = 1
        """;

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var tenantIds = tenantRegistry.GetAllTenantIds();
        var data = new Dictionary<string, object>();
        var allHealthy = true;
        var anyHealthy = false;

        foreach (var tenantId in tenantIds)
        {
            if (!tenantRegistry.TryGetDatabaseName(tenantId, out var dbName))
            {
                data[tenantId] = "Unknown — tenant not mapped";
                allHealthy = false;
                continue;
            }

            var connectionString = options.Value.ConnectionStringTemplate.Replace(
                SharedKernel.Constants.TenantConstants.DatabasePlaceholder,
                dbName,
                StringComparison.OrdinalIgnoreCase);

            try
            {
                SmtpConfigRow? smtpConfig;

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                smtpConfig = await connection.QuerySingleOrDefaultAsync<SmtpConfigRow>(
                    new CommandDefinition(SmtpConfigSql, cancellationToken: cancellationToken))
                    .ConfigureAwait(false);

                if (smtpConfig is null)
                {
                    data[tenantId] = "No default active SMTP configuration";
                    allHealthy = false;
                    continue;
                }

                using var client = new SmtpClient();

                var secureSocketOptions = smtpConfig.MailSslEnabled
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(
                    smtpConfig.MailHost,
                    smtpConfig.MailPort,
                    secureSocketOptions,
                    cancellationToken).ConfigureAwait(false);

                await client.AuthenticateAsync(
                    smtpConfig.MailFromAddress,
                    smtpConfig.MailFromPassword,
                    cancellationToken).ConfigureAwait(false);

                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

                data[tenantId] = "Healthy";
                anyHealthy = true;
            }
            catch (AuthenticationException ex)
            {
                data[tenantId] = $"Auth failed — {ex.Message}";
                allHealthy = false;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                data[tenantId] = $"Unreachable — {ex.Message}";
                allHealthy = false;
            }
        }

        if (allHealthy)
        {
            return HealthCheckResult.Healthy("All tenant SMTP configurations authenticated.", data);
        }

        return anyHealthy ? HealthCheckResult.Degraded("Some tenant SMTP configurations failed.", data: data) : HealthCheckResult.Unhealthy("No tenant SMTP configurations are working.", data: data);
    }

    private sealed class SmtpConfigRow
    {
        public string MailFromAddress { get; init; } = string.Empty;
        public string MailFromPassword { get; init; } = string.Empty;
        public string MailHost { get; init; } = string.Empty;
        public int MailPort { get; init; }
        public bool MailSslEnabled { get; init; }
    }
}
