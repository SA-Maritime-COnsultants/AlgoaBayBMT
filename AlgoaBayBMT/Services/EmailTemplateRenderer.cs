using System.Net;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using Microsoft.Extensions.Options;

namespace AlgoaBayBMT.Services
{
    public class EmailTemplateRenderer(IOptions<EmailSettings> options) : IEmailTemplateRenderer
    {
        private readonly EmailSettings settings = options.Value;

        public string Render(string subject, string heading, string bodyHtml, string? callToActionText = null, string? callToActionUrl = null, string? portalBaseUrl = null)
        {
            var companyName = settings.CompanyName ?? settings.SenderName;
            var logoUrl = ResolveLogoUrl(portalBaseUrl);
            var supportEmail = settings.SupportEmail ?? settings.AdminEmail ?? settings.SenderEmail;

            var ctaHtml = string.IsNullOrWhiteSpace(callToActionText) || string.IsNullOrWhiteSpace(callToActionUrl)
                ? string.Empty
                : $"""
                  <tr>
                      <td style='padding:24px 40px 8px;'>
                          <table role='presentation' cellspacing='0' cellpadding='0' border='0'>
                              <tr>
                                  <td style='border-radius:10px;background:#12b5cb;'>
                                      <a href='{WebUtility.HtmlEncode(callToActionUrl)}' style='display:inline-block;padding:14px 24px;font-family:Segoe UI,Arial,sans-serif;font-size:15px;font-weight:700;color:#ffffff;text-decoration:none;'>
                                          {WebUtility.HtmlEncode(callToActionText)}
                                      </a>
                                  </td>
                              </tr>
                          </table>
                      </td>
                  </tr>
                  """;

            return $"""
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8' />
    <meta http-equiv='x-ua-compatible' content='ie=edge' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>{WebUtility.HtmlEncode(subject)}</title>
</head>
<body style='margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background:#f4f7fb;padding:32px 16px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='max-width:720px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);'>
                    <tr>
                        <td style='background:linear-gradient(135deg,#0b1f33,#12304d);padding:28px 32px;text-align:center;'>
                            <img src='{WebUtility.HtmlEncode(logoUrl)}' alt='{WebUtility.HtmlEncode(companyName)} logo' style='max-width:176px;height:auto;display:block;margin:0 auto 14px;' />
                            <div style='color:#ffffff;font-size:24px;font-weight:700;line-height:1.2;'>{WebUtility.HtmlEncode(companyName)}</div>
                            <div style='color:rgba(255,255,255,.8);font-size:13px;margin-top:6px;'>Bunkering Management &amp; Training</div>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:32px 40px 12px;'>
                            <h1 style='margin:0 0 10px;font-size:24px;line-height:1.25;color:#0b1f33;'>{WebUtility.HtmlEncode(heading)}</h1>
                            <div style='font-size:15px;line-height:1.8;color:#334155;'>
                                {bodyHtml}
                            </div>
                        </td>
                    </tr>
                    {ctaHtml}
                    <tr>
                        <td style='padding:20px 40px 32px;font-size:13px;line-height:1.7;color:#64748b;'>
                            <div>If you need help, contact <a href='mailto:{WebUtility.HtmlEncode(supportEmail)}' style='color:#0e97aa;text-decoration:none;'>{WebUtility.HtmlEncode(supportEmail)}</a>.</div>
                            <div style='margin-top:12px;'>This email was sent by {WebUtility.HtmlEncode(companyName)}.</div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
""";
        }

        private string ResolveLogoUrl(string? portalBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(settings.LogoUrl))
            {
                return settings.LogoUrl;
            }

            if (!string.IsNullOrWhiteSpace(portalBaseUrl) && Uri.TryCreate(portalBaseUrl, UriKind.Absolute, out var baseUri))
            {
                return new Uri(baseUri, "images/logo.png").ToString();
            }

            if (!string.IsNullOrWhiteSpace(settings.BaseUrl) && Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var configuredBaseUri))
            {
                return new Uri(configuredBaseUri, "images/logo.png").ToString();
            }

            return "images/logo.png";
        }
    }
}
