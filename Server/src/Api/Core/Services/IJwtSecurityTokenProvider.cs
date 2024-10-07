using System.IdentityModel.Tokens.Jwt;
using API.Core.Models;

namespace API.Core.Services;

public interface IJwtSecurityTokenProvider
{
    JwtSecurityToken Get(GenerateTokenModel model, DateTimeOffset expireAt);
}