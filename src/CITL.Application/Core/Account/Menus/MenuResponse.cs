using System.Text.Json.Serialization;

namespace CITL.Application.Core.Account.Menus;

/// <summary>
/// Represents a single menu item returned by <c>citltvf.Login_Menus</c>.
/// </summary>
/// <remarks>
/// Dapper maps SQL underscore columns via <c>DefaultTypeMap.MatchNamesWithUnderscores</c>.
/// <c>[JsonPropertyName]</c> ensures JSON output matches DB column names.
/// When requested as a tree, <see cref="Children"/> is populated with child items.
/// When requested flat, <see cref="Children"/> is always empty.
/// </remarks>
public sealed class MenuResponse
{
    /// <summary>Gets the unique menu identifier (hierarchical code, e.g. "01", "0101").</summary>
    [JsonPropertyName("MENU_ID")]
    public string MenuId { get; init; } = string.Empty;

    /// <summary>Gets the display name of the menu item.</summary>
    [JsonPropertyName("MENU_Name")]
    public string MenuName { get; init; } = string.Empty;

    /// <summary>Gets an optional description for the menu item.</summary>
    [JsonPropertyName("MENU_Description")]
    public string? MenuDescription { get; init; }

    /// <summary>Gets the parent menu identifier, or <see langword="null"/> for root items.</summary>
    [JsonPropertyName("MENU_Parent_ID")]
    public string? MenuParentId { get; init; }

    /// <summary>Gets the primary navigation URL.</summary>
    [JsonPropertyName("MENU_URL1")]
    public string? MenuUrl1 { get; init; }

    /// <summary>Gets the secondary navigation URL.</summary>
    [JsonPropertyName("MENU_URL2")]
    public string? MenuUrl2 { get; init; }

    /// <summary>Gets the tertiary navigation URL.</summary>
    [JsonPropertyName("MENU_URL3")]
    public string? MenuUrl3 { get; init; }

    /// <summary>Gets the menu flag indicator.</summary>
    [JsonPropertyName("MENU_Flag")]
    public string? MenuFlag { get; init; }

    /// <summary>Gets the primary icon identifier or CSS class.</summary>
    [JsonPropertyName("MENU_Icon1")]
    public string? MenuIcon1 { get; init; }

    /// <summary>Gets the secondary icon identifier or CSS class.</summary>
    [JsonPropertyName("MENU_Icon2")]
    public string? MenuIcon2 { get; init; }

    /// <summary>Gets a value indicating whether this menu is the startup/default page.</summary>
    [JsonPropertyName("MENU_Startup_Flag")]
    public bool MenuStartupFlag { get; init; }

    /// <summary>
    /// Gets the child menu items.
    /// Populated only when the response is requested as a tree; always empty for flat responses.
    /// </summary>
    public List<MenuResponse> Children { get; } = [];
}
