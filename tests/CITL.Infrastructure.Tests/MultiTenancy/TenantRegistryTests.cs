using CITL.Infrastructure.MultiTenancy;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CITL.Infrastructure.Tests.MultiTenancy;

/// <summary>
/// Unit tests for <see cref="TenantRegistry"/>.
/// Verifies lookup, case-insensitivity, unknown tenant handling, and hot-reload.
/// </summary>
public sealed class TenantRegistryTests : IDisposable
{
    private readonly TenantRegistry _registry;
    private readonly IOptionsMonitor<TenantSettings> _optionsMonitor;
    private Action<TenantSettings, string?>? _changeCallback;

    public TenantRegistryTests()
    {
        var settings = new TenantSettings
        {
            ConnectionStringTemplate = "Server=.;Database={dbName};",
            TenantMappings = new()
            {
                ["tn_alpha"] = "DB_Alpha",
                ["tn_beta"] = "DB_Beta"
            }
        };

        _optionsMonitor = Substitute.For<IOptionsMonitor<TenantSettings>>();
        _optionsMonitor.CurrentValue.Returns(settings);
        _optionsMonitor
            .OnChange(Arg.Do<Action<TenantSettings, string?>>(cb => _changeCallback = cb))
            .Returns(Substitute.For<IDisposable>());

        _registry = new(_optionsMonitor);
    }

    public void Dispose() => _registry.Dispose();

    // ── TryGetDatabaseName ────────────────────────────────────────────────────

    [Fact]
    public void TryGetDatabaseName_KnownTenant_ReturnsTrueAndDatabaseName()
    {
        // Act
        var found = _registry.TryGetDatabaseName("tn_alpha", out var dbName);

        // Assert
        Assert.True(found);
        Assert.Equal("DB_Alpha", dbName);
    }

    [Fact]
    public void TryGetDatabaseName_UnknownTenant_ReturnsFalse()
    {
        // Act
        var found = _registry.TryGetDatabaseName("tn_unknown", out var dbName);

        // Assert
        Assert.False(found);
        Assert.Null(dbName);
    }

    [Fact]
    public void TryGetDatabaseName_CaseInsensitiveLookup_ReturnsTrueForDifferentCase()
    {
        // Act
        var found = _registry.TryGetDatabaseName("TN_ALPHA", out var dbName);

        // Assert
        Assert.True(found);
        Assert.Equal("DB_Alpha", dbName);
    }

    // ── GetAllTenantIds ───────────────────────────────────────────────────────

    [Fact]
    public void GetAllTenantIds_ReturnsAllConfiguredTenants()
    {
        // Act
        var tenantIds = _registry.GetAllTenantIds();

        // Assert
        Assert.Equal(2, tenantIds.Count);
    }

    // ── Hot-reload ────────────────────────────────────────────────────────────

    [Fact]
    public void OnChange_UpdatesLookupWithNewTenantMappings()
    {
        // Arrange
        Assert.NotNull(_changeCallback); // Should have been captured in constructor

        var newSettings = new TenantSettings
        {
            TenantMappings = new()
            {
                ["tn_gamma"] = "DB_Gamma"
            }
        };

        // Act
        _changeCallback(newSettings, null);

        // Assert — new tenant available
        Assert.True(_registry.TryGetDatabaseName("tn_gamma", out var dbName));
        Assert.Equal("DB_Gamma", dbName);

        // Assert — old tenant no longer available
        Assert.False(_registry.TryGetDatabaseName("tn_alpha", out _));
    }
}
