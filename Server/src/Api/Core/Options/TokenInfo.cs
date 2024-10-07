namespace API.Core.Options;

public sealed class TokenInfo
{
    public string SecretKey { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public int LifeTimeInMinutes { get; init; }
}