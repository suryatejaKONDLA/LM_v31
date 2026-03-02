using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using CITL.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.MultiTenancy;

/// <summary>
/// Provides O(1) lookup from opaque tenant identifiers to database names
/// using a <see cref="FrozenDictionary{TKey, TValue}"/>.
/// Registered as a singleton; supports configuration hot-reload via
/// <see cref="IOptionsMonitor{TOptions}"/> without application restart.
/// </summary>
internal sealed class TenantRegistry : ITenantRegistry, IDisposable
{
    private FrozenDictionary<string, string> _tenantMap;
    private readonly IDisposable? _changeToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantRegistry"/> class.
    /// Builds the frozen lookup from configuration and subscribes to changes.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor for tenant settings hot-reload.</param>
    public TenantRegistry(IOptionsMonitor<TenantSettings> optionsMonitor)
    {
        _tenantMap = BuildMap(optionsMonitor.CurrentValue);

        _changeToken = optionsMonitor.OnChange(settings =>
            Volatile.Write(ref _tenantMap, BuildMap(settings)));
    }

    /// <inheritdoc />
    public bool TryGetDatabaseName(
        string tenantId,
        [NotNullWhen(true)] out string? databaseName)
    {
        var map = Volatile.Read(ref _tenantMap);
        return map.TryGetValue(tenantId, out databaseName);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetAllTenantIds()
    {
        var map = Volatile.Read(ref _tenantMap);
        return map.Keys;
    }

    /// <summary>
    /// Releases the options change subscription.
    /// </summary>
    public void Dispose() => _changeToken?.Dispose();

    private static FrozenDictionary<string, string> BuildMap(TenantSettings settings) =>
        settings.TenantMappings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
