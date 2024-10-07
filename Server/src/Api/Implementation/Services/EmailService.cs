using API.Constants;
using API.Core.Enums;
using API.Core.Options;
using API.Core.Services;
using FluentResults;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace API.Implementation.Services;

public class EmailService : IEmailService
{
    private readonly ServerOptions _serverOptions;
    private readonly int _tokenLifetimeInMinutes;
    private readonly SmtpOptions _smtpOptions;

private string _mailBody = $@"
        <html>
        <body>
            <h1>Password Recovery</h1>
            <p>To reset your password, please click the link below:</p>
            <p><a href='{{0}}'>Reset Password</a></p>
            <p>This link will expire in {{1}} minutes.</p>
        </body>
        </html>";

    public EmailService(IOptions<SmtpOptions> smtpOptions, IOptions<ServerOptions> serverOptions, IOptions<TokenOptions> tokenOptions)
    {
        _serverOptions = serverOptions.Value;
        _smtpOptions = smtpOptions.Value;
        var recoveryTokenInfo = tokenOptions.Value.TokenInfos.GetValueOrDefault(TokenType.Recovery);
        _tokenLifetimeInMinutes = recoveryTokenInfo?.LifeTimeInMinutes ?? throw new ArgumentNullException(nameof(recoveryTokenInfo.LifeTimeInMinutes));
    }
    public Result SendRecoveryEmail(string email, string token, CancellationToken ct)
    {
        var message = CreateMessage(email, token);
        
        using var client = new SmtpClient();
        client.Connect(_smtpOptions.Smtp, _smtpOptions.Port, true, ct);
        client.Authenticate(_smtpOptions.UserName, _smtpOptions.Password, ct);
        
        client.Send (message);
        client.Disconnect (true, ct);

        return Result.Ok();
    }

    private MimeMessage CreateMessage(string email, string token)
    {
        var message = new MimeMessage();
        var url = _smtpOptions;
        message.From.Add(new MailboxAddress("", _smtpOptions.MailFrom));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = MessageConstants.MailSubjectPasswordRecovery;

        var recoveryLink = $"{_serverOptions.Domain}{_serverOptions.TokenRecoveryPath}{token}";
        var htmlBody = string.Format(_mailBody, recoveryLink, _tokenLifetimeInMinutes);

        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };
        return message;
    }
}