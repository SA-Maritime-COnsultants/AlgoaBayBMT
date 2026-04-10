namespace AlgoaBayBMT.Services.Interfaces
{
    public interface IEmailTemplateRenderer
    {
        string Render(string subject, string heading, string bodyHtml, string? callToActionText = null, string? callToActionUrl = null, string? portalBaseUrl = null);
    }
}
