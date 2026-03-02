using CITL.Application.Common.Interfaces;
using CITL.Infrastructure.MultiTenancy;
using CITL.Infrastructure.Persistence;
using CITL.SharedKernel.Exceptions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CITL.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for <see cref="SqlConnectionFactory"/>.
/// Verifies connection string placeholder substitution and tenant guard.
/// </summary>
public sealed class SqlConnectionFactoryTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();

    private SqlConnectionFactory CreateFactory(string template = "Server=.;Database={dbName};Trusted_Connection=True")
    {
        var settings = new TenantSettings { ConnectionStringTemplate = template };
        var options = Options.Create(settings);
        return new(_tenantContext, options);
    }

    // ── Tenant not resolved → TenantException ─────────────────────────────────

    [Fact]
    public void CreateConnection_WhenTenantNotResolved_ThrowsTenantException()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(false);
        var factory = CreateFactory();

        // Act & Assert
        Assert.Throws<TenantException>(factory.CreateConnection);
    }

    // ── Tenant resolved → valid connection ────────────────────────────────────

    [Fact]
    public void CreateConnection_WhenTenantResolved_ReturnsConnectionWithReplacedDbName()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.DatabaseName.Returns("CITL_Prod");
        var factory = CreateFactory();

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        Assert.Contains("CITL_Prod", connection.ConnectionString, StringComparison.Ordinal);
        Assert.DoesNotContain("{dbName}", connection.ConnectionString, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateConnection_PlaceholderSubstitution_IsCaseInsensitive()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.DatabaseName.Returns("MyDB");
        var factory = CreateFactory("Server=.;Database={DBNAME};");

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        Assert.Contains("MyDB", connection.ConnectionString, StringComparison.Ordinal);
    }
}
