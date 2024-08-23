using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using FasonPortal.Models;

public interface IEmailSender
{
    Task SendEmailAsync(List<string> toEmails, string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly MailSettings _mailSettings;

    public EmailSender(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(List<string> toEmails, string subject, string body)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.Subject = subject;

        foreach (var emailAddress in toEmails)
        {
            email.To.Add(MailboxAddress.Parse(emailAddress));
        }

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}
