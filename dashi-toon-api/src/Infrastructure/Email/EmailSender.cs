using DashiToon.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace DashiToon.Api.Infrastructure.Email;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> options, ILogger<EmailSender> logger)
    {
        Options = options.Value;
        _logger = logger;
    }

    public AuthMessageSenderOptions Options { get; }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        Guard.Against.NullOrEmpty(Options.PostMarkToken, "PostMarkToken not found");

        await Execute(Options.PostMarkToken, "confirm-email", user, confirmationLink.Replace("&amp;", "&"), email);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        Guard.Against.NullOrEmpty(Options.PostMarkToken, "PostMarkToken not found");

        await Execute(Options.PostMarkToken, "password-reset-link", user, resetLink.Replace("&amp;", "&"), email);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        Guard.Against.NullOrEmpty(Options.PostMarkToken, "PostMarkToken not found");

        TemplatedPostmarkMessage message = new()
        {
            To = email,
            From = "system@dashitoon.shutano.com",
            TrackOpens = true,
            TemplateAlias = "password-reset",
            TemplateModel = new
            {
                product_name = "DashiToon",
                name = user.UserName,
                reset_code = resetCode,
                support_url = "",
                company_name = "DashiToon",
                company_address = ""
            },
            MessageStream = "outbound"
        };

        PostmarkClient client = new(Options.PostMarkToken);

        PostmarkResponse? sendResult = await client.SendEmailWithTemplateAsync(message);

        if (sendResult.Status == PostmarkStatus.Success)
        {
            _logger.LogInformation("Message sent to {toEmail}", email);
        }
        else
        {
            _logger.LogError("Message failed to send to {toEmail}", email);
        }
    }

    public async Task Execute(string token, string template, ApplicationUser user, string content, string toEmail)
    {
        TemplatedPostmarkMessage message = new()
        {
            To = toEmail,
            From = "system@dashitoon.shutano.com",
            TrackOpens = true,
            TemplateAlias = template,
            TemplateModel = new
            {
                product_name = "DashiToon",
                name = user.UserName,
                value = $"{content}",
                support_url = "",
                company_name = "DashiToon",
                company_address = ""
            },
            MessageStream = "outbound"
        };

        PostmarkClient client = new(token);

        PostmarkResponse? sendResult = await client.SendEmailWithTemplateAsync(message);

        if (sendResult.Status == PostmarkStatus.Success)
        {
            _logger.LogInformation("Message sent to {toEmail}", toEmail);
        }
        else
        {
            _logger.LogError("Message failed to send to {toEmail}", toEmail);
        }
    }
}
