using CITL.SharedKernel.Helpers;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="FileNameHelper"/>.
/// Validates filename resolution, placeholder replacement, and extension handling.
/// </summary>
public sealed class FileNameHelperTests
{
    // ── ResolveFileName — Custom name ─────────────────────────────────────────

    [Fact]
    public void ResolveFileName_CustomName_ReturnsCustomNameWithExtension()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("photo.jpg", "profile", autoGenerate: false);

        // Assert — should append .jpg since custom name has no extension
        Assert.Equal("profile.jpg", result);
    }

    [Fact]
    public void ResolveFileName_CustomNameWithExtension_KeepsCustomExtension()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("photo.jpg", "profile.png", autoGenerate: false);

        // Assert — custom name already has extension, keep it
        Assert.Equal("profile.png", result);
    }

    [Fact]
    public void ResolveFileName_CustomNameWithIdPlaceholder_ReplacesWithGuid()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("doc.pdf", "{id}", autoGenerate: false);

        // Assert — should be a 32-char GUID + .pdf
        Assert.EndsWith(".pdf", result, StringComparison.Ordinal);
        Assert.Equal(36, result.Length); // 32 hex chars + ".pdf"
    }

    [Fact]
    public void ResolveFileName_CustomNameWithDtPlaceholder_ReplacesWithTimestamp()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("report.xlsx", "report-{dt}", autoGenerate: false);

        // Assert — should contain a 14-char timestamp
        Assert.StartsWith("report-", result, StringComparison.Ordinal);
        Assert.EndsWith(".xlsx", result, StringComparison.Ordinal);
        Assert.Equal(26, result.Length); // "report-" (7) + timestamp (14) + ".xlsx" (5)
    }

    // ── ResolveFileName — Auto-generate ───────────────────────────────────────

    [Fact]
    public void ResolveFileName_AutoGenerate_ReturnsGuidFileName()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("photo.jpg", null, autoGenerate: true);

        // Assert — should be GUID (32 chars) + .jpg
        Assert.EndsWith(".jpg", result, StringComparison.Ordinal);
        Assert.Equal(36, result.Length); // 32 hex + ".jpg"
    }

    [Fact]
    public void ResolveFileName_AutoGenerate_PreservesOriginalExtension()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("document.pdf", null, autoGenerate: true);

        // Assert
        Assert.EndsWith(".pdf", result, StringComparison.Ordinal);
    }

    // ── ResolveFileName — Default (original) ──────────────────────────────────

    [Fact]
    public void ResolveFileName_NoCustomNoAutoGenerate_ReturnsOriginal()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("original-file.txt", null, autoGenerate: false);

        // Assert
        Assert.Equal("original-file.txt", result);
    }

    [Fact]
    public void ResolveFileName_WhitespaceCustomName_ReturnsOriginal()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("original.doc", "   ", autoGenerate: false);

        // Assert — whitespace-only custom name treated as absent
        Assert.Equal("original.doc", result);
    }

    // ── ResolveFileName — Priority ────────────────────────────────────────────

    [Fact]
    public void ResolveFileName_CustomNameTakesPriorityOverAutoGenerate()
    {
        // Act
        var result = FileNameHelper.ResolveFileName("photo.jpg", "custom", autoGenerate: true);

        // Assert — custom name wins even when autoGenerate is true
        Assert.Equal("custom.jpg", result);
    }

    // ── ReplacePlaceholders ───────────────────────────────────────────────────

    [Fact]
    public void ReplacePlaceholders_IdPlaceholder_ReplacedWithGuid()
    {
        // Act
        var result = FileNameHelper.ReplacePlaceholders("file-{id}.txt");

        // Assert
        Assert.DoesNotContain("{id}", result, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(41, result.Length); // "file-" (5) + GUID (32) + ".txt" (4)
    }

    [Fact]
    public void ReplacePlaceholders_DtPlaceholder_ReplacedWithTimestamp()
    {
        // Act
        var result = FileNameHelper.ReplacePlaceholders("backup-{dt}.sql");

        // Assert
        Assert.DoesNotContain("{dt}", result, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(25, result.Length); // "backup-" (7) + timestamp (14) + ".sql" (4) = 25
    }

    [Fact]
    public void ReplacePlaceholders_CaseInsensitive()
    {
        // Act
        var result1 = FileNameHelper.ReplacePlaceholders("{ID}.txt");
        var result2 = FileNameHelper.ReplacePlaceholders("{Id}.txt");

        // Assert
        Assert.DoesNotContain("{ID}", result1, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("{Id}", result2, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReplacePlaceholders_NoPlaceholders_ReturnsSameString()
    {
        // Act
        var result = FileNameHelper.ReplacePlaceholders("plain-file.txt");

        // Assert
        Assert.Equal("plain-file.txt", result);
    }

    [Fact]
    public void ReplacePlaceholders_BothPlaceholders_ReplacedCorrectly()
    {
        // Act
        var result = FileNameHelper.ReplacePlaceholders("{id}-{dt}");

        // Assert
        Assert.DoesNotContain("{id}", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("{dt}", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-", result, StringComparison.Ordinal); // separator preserved
    }
}
