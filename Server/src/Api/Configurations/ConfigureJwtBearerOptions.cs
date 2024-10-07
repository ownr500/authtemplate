using System.Text;
using API.Core.Enums;
using API.Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Configurations;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly TokenOptions _options;

    public ConfigureJwtBearerOptions(IOptions<TokenOptions> options)
    {
        _options = options.Value;
    }
    
    public void Configure(JwtBearerOptions options)
    {
        var tokenInfo = _options.TokenInfos.GetValueOrDefault(TokenType.Access);
        if (tokenInfo is null) return;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenInfo.SecretKey)),
            ValidAudience = tokenInfo.Audience,
            ValidIssuer = tokenInfo.Issuer,
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}