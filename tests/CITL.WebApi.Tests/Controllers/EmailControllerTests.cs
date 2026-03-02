using CITL.Application.Core.Notifications.Email;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Notifications;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="EmailController"/>.
/// Verifies correct delegation to <see cref="IEmailService"/> and HTTP response mapping.
/// </summary>
public sealed class EmailControllerTests
{
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly EmailController _controller;

    public EmailControllerTests()
    {
        _controller = new(_emailService);
    }

    // ── SendAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "<p>Hello</p>"
        };
        _emailService.SendAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.SendAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task SendAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "<p>Hello</p>"
        };
        _emailService.SendAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(new Error("Email.SmtpError", "SMTP not configured.")));

        // Act
        var actionResult = await _controller.SendAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task SendAsync_DelegatesToService()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "<p>Hello</p>"
        };
        _emailService.SendAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.SendAsync(request, CancellationToken.None);

        // Assert
        await _emailService.Received(1)
            .SendAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>());
    }

    // ── SendTestAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SendTestAsync_WithoutAttachment_DelegatesToSendWithAttachments()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test Email",
            Body = "Plain text body"
        };
        _emailService.SendWithAttachmentsAsync(
            Arg.Any<SendEmailRequest>(),
            Arg.Is<IReadOnlyList<EmailAttachment>?>(a => a == null),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.SendTestAsync(request, false, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse>(okResult.Value);
    }

    [Fact]
    public async Task SendTestAsync_WithDummyFile_PassesAttachmentToService()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "Hello"
        };
        _emailService.SendWithAttachmentsAsync(
            Arg.Any<SendEmailRequest>(),
            Arg.Is<IReadOnlyList<EmailAttachment>?>(a => a != null && a.Count == 1),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.SendTestAsync(request, true, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        await _emailService.Received(1).SendWithAttachmentsAsync(
            Arg.Any<SendEmailRequest>(),
            Arg.Is<IReadOnlyList<EmailAttachment>?>(a => a != null && a.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendTestAsync_WrapsBodyInPreTag()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "Hello <world>"
        };
        _emailService.SendWithAttachmentsAsync(
            Arg.Is<SendEmailRequest>(r => r.Body.StartsWith("<pre>", StringComparison.Ordinal)),
            Arg.Any<IReadOnlyList<EmailAttachment>?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.SendTestAsync(request, false, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendWithAttachmentsAsync(
            Arg.Is<SendEmailRequest>(r => r.Body.Contains("&lt;world&gt;", StringComparison.Ordinal)),
            Arg.Any<IReadOnlyList<EmailAttachment>?>(),
            Arg.Any<CancellationToken>());
    }
}
