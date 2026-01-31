using HackOMania.Api.Options;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace HackOMania.Api.Services;

public class PostmarkEmailService : IEmailService
{
    private readonly PostmarkClient _client;
    private readonly PostmarkOptions _options;
    private readonly ILogger<PostmarkEmailService> _logger;

    public PostmarkEmailService(
        IOptions<PostmarkOptions> options,
        ILogger<PostmarkEmailService> logger
    )
    {
        _options = options.Value;
        _logger = logger;
        _client = new PostmarkClient(_options.ServerToken);
    }

    public async Task SendParticipantAcceptedEmailAsync(
        string toEmail,
        string toName,
        string hackathonName,
        string? reason = null,
        CancellationToken ct = default
    )
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Email sending is disabled. Skipping email to {Email}",
                toEmail
            );
            return;
        }

        try
        {
            var subject = $"Congratulations! Your {hackathonName} Application Has Been Accepted";
            var htmlBody = BuildAcceptedEmailHtml(toName, hackathonName, reason);
            var textBody = BuildAcceptedEmailText(toName, hackathonName, reason);

            var message = new PostmarkMessage
            {
                From = $"{_options.FromName} <{_options.FromEmail}>",
                To = $"{toName} <{toEmail}>",
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody,
            };

            var response = await _client.SendMessageAsync(message);

            if (response.Status != PostmarkStatus.Success)
            {
                _logger.LogError(
                    "Failed to send acceptance email to {Email}. Status: {Status}, Message: {Message}",
                    toEmail,
                    response.Status,
                    response.Message
                );
            }
            else
            {
                _logger.LogInformation(
                    "Successfully sent acceptance email to {Email}",
                    toEmail
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception occurred while sending acceptance email to {Email}",
                toEmail
            );
            throw;
        }
    }

    public async Task SendParticipantRejectedEmailAsync(
        string toEmail,
        string toName,
        string hackathonName,
        string? reason = null,
        CancellationToken ct = default
    )
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Email sending is disabled. Skipping email to {Email}",
                toEmail
            );
            return;
        }

        try
        {
            var subject = $"Update on Your {hackathonName} Application";
            var htmlBody = BuildRejectedEmailHtml(toName, hackathonName, reason);
            var textBody = BuildRejectedEmailText(toName, hackathonName, reason);

            var message = new PostmarkMessage
            {
                From = $"{_options.FromName} <{_options.FromEmail}>",
                To = $"{toName} <{toEmail}>",
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody,
            };

            var response = await _client.SendMessageAsync(message);

            if (response.Status != PostmarkStatus.Success)
            {
                _logger.LogError(
                    "Failed to send rejection email to {Email}. Status: {Status}, Message: {Message}",
                    toEmail,
                    response.Status,
                    response.Message
                );
            }
            else
            {
                _logger.LogInformation(
                    "Successfully sent rejection email to {Email}",
                    toEmail
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception occurred while sending rejection email to {Email}",
                toEmail
            );
            throw;
        }
    }

    public async Task SendBatchEmailsAsync(
        IEnumerable<(string Email, string Name, string Status, string? Reason)> participants,
        string hackathonName,
        CancellationToken ct = default
    )
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email sending is disabled. Skipping batch emails.");
            return;
        }

        var tasks = participants.Select(async p =>
        {
            if (p.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
            {
                await SendParticipantAcceptedEmailAsync(
                    p.Email,
                    p.Name,
                    hackathonName,
                    p.Reason,
                    ct
                );
            }
            else if (p.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                await SendParticipantRejectedEmailAsync(
                    p.Email,
                    p.Name,
                    hackathonName,
                    p.Reason,
                    ct
                );
            }
        });

        await Task.WhenAll(tasks);
    }

    private static string BuildAcceptedEmailHtml(
        string toName,
        string hackathonName,
        string? reason
    )
    {
        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $"<p><strong>Message from organizers:</strong><br/>{reason}</p>"
            : string.Empty;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Accepted!</h1>
        </div>
        <div class=""content"">
            <p>Dear {toName},</p>
            <p>We are thrilled to inform you that your application for <strong>{hackathonName}</strong> has been <strong>accepted</strong>!</p>
            {reasonSection}
            <p>We look forward to seeing you at the event. Get ready for an amazing experience!</p>
            <p>Best regards,<br/>The {hackathonName} Team</p>
        </div>
        <div class=""footer"">
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string BuildAcceptedEmailText(
        string toName,
        string hackathonName,
        string? reason
    )
    {
        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $"\n\nMessage from organizers:\n{reason}\n"
            : string.Empty;

        return $@"Application Accepted!

Dear {toName},

We are thrilled to inform you that your application for {hackathonName} has been accepted!
{reasonSection}
We look forward to seeing you at the event. Get ready for an amazing experience!

Best regards,
The {hackathonName} Team

---
This is an automated message. Please do not reply to this email.";
    }

    private static string BuildRejectedEmailHtml(
        string toName,
        string hackathonName,
        string? reason
    )
    {
        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $"<p><strong>Reason:</strong><br/>{reason}</p>"
            : string.Empty;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Update</h1>
        </div>
        <div class=""content"">
            <p>Dear {toName},</p>
            <p>Thank you for your interest in <strong>{hackathonName}</strong>. After careful consideration, we regret to inform you that we are unable to accept your application at this time.</p>
            {reasonSection}
            <p>We appreciate your interest and encourage you to apply for future events.</p>
            <p>Best regards,<br/>The {hackathonName} Team</p>
        </div>
        <div class=""footer"">
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string BuildRejectedEmailText(
        string toName,
        string hackathonName,
        string? reason
    )
    {
        var reasonSection = !string.IsNullOrWhiteSpace(reason)
            ? $"\n\nReason:\n{reason}\n"
            : string.Empty;

        return $@"Application Update

Dear {toName},

Thank you for your interest in {hackathonName}. After careful consideration, we regret to inform you that we are unable to accept your application at this time.
{reasonSection}
We appreciate your interest and encourage you to apply for future events.

Best regards,
The {hackathonName} Team

---
This is an automated message. Please do not reply to this email.";
    }
}
