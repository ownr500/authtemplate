using API.Core.Enums;

namespace API.Core.Options;

public sealed class TokenOptions
{
    public Dictionary<TokenType, TokenInfo> TokenInfos { get; init; } = new();
}