using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API.Core.Models;
using API.Core.Services;
using Microsoft.IdentityModel.Tokens;

namespace API.Implementation.Providers;

public class JwtSecurityTokenProvider : IJwtSecurityTokenProvider
{
    public JwtSecurityToken Get(GenerateTokenModel model, DateTimeOffset expireAt)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(model.TokenInfo.SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            model.TokenInfo.Issuer,
            model.TokenInfo.Audience,
            claims: model.Claims,
            expires: expireAt.UtcDateTime,
            signingCredentials: credentials
        );
        return options;
    }
}