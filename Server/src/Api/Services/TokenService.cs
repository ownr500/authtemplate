﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Constants;
using API.Models;
using API.Models.Entities;
using API.Models.enums;
using API.Models.Response;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private const string SecretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler, ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor)
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
    }


    public async Task<(string token, DateTimeOffset expirationDate)> GenerateTokenAsync(Guid userId, List<RoleNames>? roles, JwtAudience audience)
    {
        var expirationDate = roles is null ? DateTimeOffset.UtcNow.AddDays(1) : DateTimeOffset.UtcNow.AddHours(1);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (roles is not null)
        {
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x.ToString()));
            claims.AddRange(roleClaims);
        }
        
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            audience.ToString(),
            claims: claims,
            expires: expirationDate.UtcDateTime,
            signingCredentials: credentials
        );

        var token = _tokenHandler.WriteToken(options);

        if (audience == JwtAudience.Recovery)
        {
            var recoveryTokenEntity = new RecoveryTokenEntity
            {
                Token = token,
                ExpireAt = expirationDate,
                IsActive = true
            };

            await _dbContext.RecoveryTokens.AddAsync(recoveryTokenEntity);
            await _dbContext.SaveChangesAsync();
        }
        
        return (token, expirationDate);
    }

    public async Task<Result<TokenModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct)
    {
        var model = await _dbContext.Tokens
            .Where(x => x.RefreshToken == token && x.RefreshTokenActive && x.RefreshTokenExpireAt > DateTimeOffset.UtcNow)
            .Select(x =>
                new
                {
                    userId = x.User.Id,
                    token = x,
                    roles = x.User.UserRoles
                        .Select(u => u.Role.RoleName)
                        .ToList()
                }
            )
            .FirstOrDefaultAsync(ct);
        if (model is not null)
        {
            model.token.RefreshTokenActive = false;
            var tokenModel = await GenerateNewTokenModelAsync(model.userId, model.roles, ct);
            _dbContext.Tokens.Update(model.token);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Ok(tokenModel);
        }

        return Result.Fail(MessageConstants.InvalidRefreshToken);
    }

    public async Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, List<RoleNames> roles, CancellationToken ct)
    {
        var (accessToken, accessTokenExpireAt) = await GenerateTokenAsync(userId, roles, JwtAudience.ApiKey);
        var (refreshToken, refreshTokenExpireAt) = await GenerateTokenAsync(userId, null, JwtAudience.Refresh);
        
        var token = new TokenEntity
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            AccessTokenExpireAt = accessTokenExpireAt,
            RefreshTokenExpireAt = refreshTokenExpireAt,
            RefreshTokenActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.AddAsync(token, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new TokenModel(accessToken, refreshToken);
    }

    public async Task<bool> CheckRevokedToken(StringValues header)
    {
        var headerArray = header.ToString().Split(' ');
        if (headerArray.Length == 2)
        {
            var tokenRevoked = await _dbContext.RevokedTokens.AnyAsync(x => x.Token == headerArray[1]);
            if (tokenRevoked) return false;
        }

        return true;
    }
    
    public async Task RevokeTokens(Guid userId, CancellationToken ct)
    {
        var revokedTokens = await _dbContext.Tokens
            .Where(x => x.UserId == userId && x.RefreshTokenActive)
            .Select(x => new RevokedTokenEntity[]
                {
                    new RevokedTokenEntity
                    {
                        Token = x.AccessToken,
                        TokenExpireAt = x.AccessTokenExpireAt
                    },
                    new RevokedTokenEntity
                    {
                        Token = x.RefreshToken,
                        TokenExpireAt = x.RefreshTokenExpireAt
                    }
                }
            )
            .ToListAsync(ct);

        if (revokedTokens.Count > 0)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            _dbContext.RevokedTokens.AddRange(revokedTokens
                .SelectMany(x => x));
            await _dbContext.Tokens
                .Where(x => x.UserId == userId && x.RefreshTokenActive)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.RefreshTokenActive, false), ct);

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
    }
}