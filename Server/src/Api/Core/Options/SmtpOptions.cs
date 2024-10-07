namespace API.Core.Options;

public sealed class SmtpOptions
{
    public string Url { get; init; } = string.Empty;
    public string Smtp { get; init; } = string.Empty;
    public string MailFrom { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int Port { get; init; }
}
