using API.Core.Enums;
using API.Core.Models;
using FluentResults;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.Core.Services;

public interface ITokenService
{
    Task<Result<TokenPairModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenPairModel> GenerateTokenPairAsync(Guid userId, IReadOnlyCollection<Role> claims, CancellationToken ct);
    Task<bool> ValidateAuthHeader(StringValues header);
    Task RevokeTokensAsync(Guid userId, CancellationToken ct);
    string GenerateRecoveryToken(Guid userId);
    Result<UserIdAndExpireModel> ValidateRecoveryToken(string token, CancellationToken ct);
    Task AddRecoveryTokenAsync(string token, DateTimeOffset valueExpireAt, CancellationToken ct);
    Task<Result> CheckRecoveryTokenExists(string token, CancellationToken ct);
    Task RemoveExpiredTokensAsync(CancellationToken ct);
}