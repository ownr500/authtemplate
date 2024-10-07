using System.Security.Claims;
using API.Core.Options;

namespace API.Core.Models;

public record GenerateTokenModel(
    List<Claim> Claims,
    TokenInfo TokenInfo
);
