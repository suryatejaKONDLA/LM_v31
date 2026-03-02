using CITL.Infrastructure.MultiTenancy;

namespace CITL.Infrastructure.Tests.MultiTenancy;

/// <summary>
/// Unit tests for <see cref="TenantContext"/>.
/// Verifies SetTenant behavior, IsResolved state, and guard clauses.
/// </summary>
public sealed class TenantContextTests
{
    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewInstance_IsNotResolved()
    {
        // Arrange
        var context = new TenantContext();

        // Assert
        Assert.False(context.IsResolved);
        Assert.Equal(string.Empty, context.TenantId);
        Assert.Equal(string.Empty, context.DatabaseName);
    }

    // ── SetTenant ─────────────────────────────────────────────────────────────

    [Fact]
    public void SetTenant_WithValidValues_SetsPropertiesAndIsResolved()
    {
        // Arrange
        var context = new TenantContext();

        // Act
        context.SetTenant("tn_company_a", "CITL_CompanyA");

        // Assert
        Assert.True(context.IsResolved);
        Assert.Equal("tn_company_a", context.TenantId);
        Assert.Equal("CITL_CompanyA", context.DatabaseName);
    }

    // ── Guard clauses ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetTenant_WithInvalidTenantId_ThrowsArgumentException(string? tenantId)
    {
        // Arrange
        var context = new TenantContext();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => context.SetTenant(tenantId!, "ValidDb"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetTenant_WithInvalidDatabaseName_ThrowsArgumentException(string? databaseName)
    {
        // Arrange
        var context = new TenantContext();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => context.SetTenant("valid_tenant", databaseName!));
    }

    // ── Overwrite ─────────────────────────────────────────────────────────────

    [Fact]
    public void SetTenant_CalledTwice_OverwritesPreviousValues()
    {
        // Arrange
        var context = new TenantContext();
        context.SetTenant("tn_first", "DB_First");

        // Act
        context.SetTenant("tn_second", "DB_Second");

        // Assert
        Assert.Equal("tn_second", context.TenantId);
        Assert.Equal("DB_Second", context.DatabaseName);
    }
}
