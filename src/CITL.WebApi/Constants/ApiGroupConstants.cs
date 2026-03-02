namespace CITL.WebApi.Constants;

/// <summary>
/// OpenAPI document group identifiers for module-based API grouping.
/// Each constant maps to a separate OpenAPI document exposed via Swagger UI and Scalar.
/// </summary>
/// <remarks>
/// To add a new group: (1) add a <c>const string</c> below, (2) add it to <see cref="All"/>.
/// The display name is derived automatically — no separate dictionary needed.
/// </remarks>
internal static class ApiGroupConstants
{
    internal const string Account = "account";
    internal const string Administration = "administration";
    internal const string Authentication = "authentication";
    internal const string Common = "common";
    internal const string Finance = "finance";
    internal const string Inventory = "inventory";
    internal const string Masters = "masters";
    internal const string PointOfSale = "point-of-sale";
    internal const string Purchase = "purchase";
    internal const string Transactions = "transactions";

    /// <summary>
    /// All group IDs in display order. Display names are derived from the ID — no dictionary needed.
    /// </summary>
    internal static readonly IReadOnlyList<string> All =
    [
        Account,
        Administration,
        Authentication,
        Common,
        Finance,
        Inventory,
        Masters,
        PointOfSale,
        Purchase,
        Transactions
    ];
}
