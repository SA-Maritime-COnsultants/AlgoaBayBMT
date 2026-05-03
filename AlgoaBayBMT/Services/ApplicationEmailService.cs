using System.Net.Mail;
using System.Net;
using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AlgoaBayBMT.Services
{
    public class ApplicationEmailService(
        IOptions<EmailSettings> options,
        IEmailTemplateRenderer templateRenderer,
        ILogger<ApplicationEmailService> logger) : IApplicationEmailService
    {
        private readonly EmailSettings settings = options.Value;

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            SendHtmlEmailAsync(email, "Confirm your email", "Confirm your email", $"Thank you for registering with {GetCompanyName()}.",
                $"Please confirm your account to activate your login. Click the button below to verify your email address and continue the approval process.",
                "Confirm email", confirmationLink, null);

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            SendHtmlEmailAsync(email, "Reset your password", "Reset your password", "We received a request to reset your password.",
                "Click the button below to choose a new password. If you did not request this change, you can safely ignore this email.",
                "Reset password", resetLink, null);

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            SendHtmlEmailAsync(email, "Reset your password", "Reset your password", "We received a request to reset your password.",
                $"Use the code below to complete the password reset process:<br /><strong style='font-size:20px;letter-spacing:2px;'>{WebUtility.HtmlEncode(resetCode)}</strong>",
                null, null, null);

        public async Task SendWelcomeEmailAsync(ApplicationUser user, string portalBaseUrl, CancellationToken cancellationToken = default)
        {
            _ = await SendWelcomeEmailWithResultAsync(user, portalBaseUrl, cancellationToken);
        }

        public Task<OperationResult> SendWelcomeEmailWithResultAsync(ApplicationUser user, string portalBaseUrl, CancellationToken cancellationToken = default)
        {
            var greetingName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email ?? "there";
            var portalLink = ResolvePortalLink(portalBaseUrl);
            var rankText = user.IsCrew && user.CrewRank.HasValue ? $"<p style='margin:0 0 12px;'>Crew rank on file: <strong>{WebUtility.HtmlEncode(user.CrewRank.Value.GetDisplayName())}</strong>.</p>" : string.Empty;
            var crewText = user.IsCrew
                ? "Your account has been marked as crew, and your vessel or operational assignments will be completed later by the Master or an administrator."
                : "Your account has been created as a customer account for general access.";
            var body = $"""
                <p style='margin:0 0 12px;'>Hello {WebUtility.HtmlEncode(greetingName)},</p>
                <p style='margin:0 0 12px;'>Thank you for registering with {WebUtility.HtmlEncode(GetCompanyName())}. Your account has been created successfully.</p>
                <p style='margin:0 0 12px;'>{WebUtility.HtmlEncode(crewText)}</p>
                {rankText}
                <p style='margin:0 0 12px;'>Access may still be subject to email confirmation and/or approval review depending on the current application workflow.</p>
                <p style='margin:0 0 12px;'>Once approved, you can sign in and continue using the portal.</p>
                <p style='margin:0;'>If you need assistance, please contact our support team.</p>
            """;

            return SendHtmlEmailAsync(user.Email ?? string.Empty, "Welcome to Algoa Bay BMT", "Welcome to Algoa Bay BMT", "Your registration was successful.", body, "Open portal", portalLink, portalBaseUrl);
        }

        public Task<OperationResult> SendCrewWelcomeEmailAsync(ApplicationUser user, string generatedPassword, string portalBaseUrl, CancellationToken cancellationToken = default)
        {
            var greetingName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email ?? "there";
            var portalLink = ResolvePortalLink(portalBaseUrl);
            var rankText = user.IsCrew && user.CrewRank.HasValue
                ? $"<p style='margin:0 0 12px;'>Crew rank on file: <strong>{WebUtility.HtmlEncode(user.CrewRank.Value.GetDisplayName())}</strong>.</p>"
                : string.Empty;
            var body = $"""
                <p style='margin:0 0 12px;'>Hello {WebUtility.HtmlEncode(greetingName)},</p>
                <p style='margin:0 0 12px;'>A crew account has been created for you on the <strong>{WebUtility.HtmlEncode(GetCompanyName())}</strong> training platform.</p>
                <p style='margin:0 0 12px;'>Your account is ready to use. You can sign in with the credentials below:</p>
                <table style='border-collapse:collapse;margin-bottom:16px;'>
                  <tr><td style='padding:4px 12px 4px 0;font-weight:600;'>Login (email):</td><td style='padding:4px 0;'>{WebUtility.HtmlEncode(user.Email ?? string.Empty)}</td></tr>
                  <tr><td style='padding:4px 12px 4px 0;font-weight:600;'>Temporary password:</td><td style='padding:4px 0;letter-spacing:2px;'><strong>{WebUtility.HtmlEncode(generatedPassword)}</strong></td></tr>
                </table>
                <p style='margin:0 0 12px;'><strong>Please change your password after your first login.</strong></p>
                {rankText}
                <p style='margin:0;'>If you have any questions, please contact your Master or the administrator.</p>
            """;
            return SendHtmlEmailAsync(
                user.Email ?? string.Empty,
                "Your Algoa Bay BMT crew account",
                "Your crew training account is ready",
                "You have been registered as a crew member.",
                body,
                "Sign in now",
                portalLink,
                portalBaseUrl);
        }

        private async Task<OperationResult> SendHtmlEmailAsync(string toEmail, string subject, string heading, string intro, string bodyHtml, string? callToActionText, string? callToActionUrl, string? portalBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return OperationResult.Failure("The target email address is missing.");
            }

            var content = templateRenderer.Render(subject, heading, $"<p style='margin:0 0 12px;'>{WebUtility.HtmlEncode(intro)}</p><p style='margin:0;'>{bodyHtml}</p>", callToActionText, callToActionUrl, portalBaseUrl);
            using var message = new MailMessage
            {
                From = new MailAddress(settings.SenderEmail, settings.SenderName),
                Subject = subject,
                Body = content,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            try
            {
                using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
                {
                    EnableSsl = settings.UseSsl,
                    Credentials = string.IsNullOrWhiteSpace(settings.Username)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(settings.Username, settings.Password)
                };

                await client.SendMailAsync(message);
                return OperationResult.Success("Email sent successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", toEmail, subject);
                return OperationResult.Failure("The email could not be sent. Check the email configuration or SMTP connection.");
            }
        }

        private string GetCompanyName() => settings.CompanyName ?? settings.SenderName;

        private string ResolvePortalLink(string? portalBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(portalBaseUrl) && Uri.TryCreate(portalBaseUrl, UriKind.Absolute, out var baseUri))
            {
                return new Uri(baseUri, "/").ToString();
            }

            if (!string.IsNullOrWhiteSpace(settings.BaseUrl) && Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var configuredBaseUri))
            {
                return new Uri(configuredBaseUri, "/").ToString();
            }

            return "/";
        }
    }
}
