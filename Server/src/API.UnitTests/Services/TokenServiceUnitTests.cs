using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Options;
using API.Core.Services;
using API.Implementation.Services;
using API.UnitTests.Helpers;
using AutoFixture;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace API.UnitTests.Services;

public class TokenServiceUnitTests
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IOptions<TokenOptions> _options;
    private readonly IJwtSecurityTokenProvider _tokenProvider;
    private readonly ISystemClock _systemClock;

    private const string AccessToken = "accessToken";
    private const string NewAccessToken = "newAccessToken";
    private const string RefreshToken = "refreshToken";
    private const string RecoveryToken = "recoveryToken";
    private const string NewRefreshToken = "newRefreshToken";
    private const string AnotherRefreshToken = "RefreshToken1";
    private const string AudienceAccess = "Access";
    private const string AudienceRefresh = "Refresh";
    private const string AudienceRecovery = "Recovery";
    private readonly DateTimeOffset _utcNow = new(2024, 12, 1, 12, 20, 35, TimeZoneInfo.Utc.BaseUtcOffset);
    private readonly CancellationToken _ct = CancellationToken.None;

    private readonly TokenInfo _accessTokenInfo = new()
    {
        Audience = "Access",
        Issuer = "localhost",
        SecretKey = "secret",
        LifeTimeInMinutes = 60
    };

    private readonly TokenInfo _refreshTokenInfo = new()
    {
        Audience = "Refresh",
        Issuer = "localhost",
        SecretKey = "secret",
        LifeTimeInMinutes = 1440
    };

    private readonly TokenInfo _recoveryTokenInfo = new()
    {
        Audience = "Recovery",
        Issuer = "localhost",
        SecretKey = "secret",
        LifeTimeInMinutes = 15
    };

    public TokenServiceUnitTests()
    {
        _tokenHandler = Substitute.For<JwtSecurityTokenHandler>();
        _options = Substitute.For<IOptions<TokenOptions>>();
        _tokenProvider = Substitute.For<IJwtSecurityTokenProvider>();
        _systemClock = Substitute.For<ISystemClock>();
        _systemClock.UtcNow.Returns(_utcNow);

        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, _accessTokenInfo
                },
                {
                    TokenType.Refresh, _refreshTokenInfo
                },
                {
                    TokenType.Recovery, _recoveryTokenInfo
                }
            }
        });
    }

    [Fact]
    public async Task ShouldGenerateNewTokenFromRefreshTokenAsync()
    {
        //Arrange
        var tokenPairModel = new TokenPairModel(NewAccessToken, NewRefreshToken);
        var expected = Result.Ok(tokenPairModel);

        var accessOptions = new JwtSecurityToken();
        _tokenProvider
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
                _utcNow.AddMinutes(_accessTokenInfo.LifeTimeInMinutes)).Returns(accessOptions);

        var refreshOptions = new JwtSecurityToken();
        _tokenProvider.Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
            _utcNow.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)).Returns(refreshOptions);

        _tokenHandler.WriteToken(accessOptions)
            .Returns(NewAccessToken);
        _tokenHandler.WriteToken(refreshOptions)
            .Returns(NewRefreshToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };

        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = _utcNow.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.False(refreshTokenActive);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseMissingTokenAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.InvalidRefreshToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };

        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = AnotherRefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        _tokenHandler.Received(0).WriteToken(Arg.Any<JwtSecurityToken>());
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.True(refreshTokenActive);
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseRefreshTokenInactiveAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.InvalidRefreshToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };

        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            RefreshTokenActive = false,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        _tokenHandler.Received(0).WriteToken(Arg.Any<JwtSecurityToken>());
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.False(refreshTokenActive);
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseRefreshExpiredAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.InvalidRefreshToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };

        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        _tokenHandler.Received(0).WriteToken(Arg.Any<JwtSecurityToken>());
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.True(refreshTokenActive);
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Theory]
    [InlineData(new[] { Role.User })]
    [InlineData(new[] { Role.Admin, Role.User })]
    public async Task ShouldGenerateTokenPairAsync(Role[] roles)
    {
        //Arrange
        var expected = new TokenPairModel(AccessToken, RefreshToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = roles.Select(x => new UserRoleEntity
            {
                RoleId = RoleConstants.RoleIds[x]
            }).ToList()
        };
        var dbContext = DbHelper.CreateSqLiteDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var accessOptions = new JwtSecurityToken();
        _tokenProvider
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
                _utcNow.AddMinutes(_accessTokenInfo.LifeTimeInMinutes)).Returns(accessOptions);

        var refreshOptions = new JwtSecurityToken();
        _tokenProvider.Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
            _utcNow.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)).Returns(refreshOptions);

        _tokenHandler.WriteToken(accessOptions)
            .Returns(AccessToken);
        _tokenHandler.WriteToken(refreshOptions)
            .Returns(RefreshToken);

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateTokenPairAsync(user.Id, roles, _ct);

        //Assert
        var tokenCreated = await dbContext.Tokens.AnyAsync(x => x.User.Id == user.Id
                                                                && x.RefreshTokenActive
                                                                && x.AccessToken == AccessToken
                                                                && x.RefreshToken == RefreshToken, _ct);

        Assert.True(tokenCreated);
        Assert.Equivalent(expected, actual);

        _tokenProvider.Received(1)
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
                _utcNow.AddMinutes(_accessTokenInfo.LifeTimeInMinutes));
        _tokenProvider.Received(1)
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
                _utcNow.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes));

        _tokenHandler.Received(1)
            .WriteToken(accessOptions);
        _tokenHandler.Received(1)
            .WriteToken(refreshOptions);
    }

    [Fact]
    public async Task ShouldGenerateRecoveryToken()
    {
        //Arrange
        var expected = RecoveryToken;

        var user = new UserEntity
        {
            Id = Guid.NewGuid()
        };

        var dbContext = DbHelper.CreateSqLiteDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        var recoveryOptions = new JwtSecurityToken();
        var expireAt = _utcNow.AddMinutes(_recoveryTokenInfo.LifeTimeInMinutes);
        _tokenProvider
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRecovery), expireAt.DateTime)
            .Returns(recoveryOptions);

        _tokenHandler.WriteToken(recoveryOptions).Returns(RecoveryToken);

        //Act
        var actual = tokenService.GenerateRecoveryToken(user.Id);

        //Assert
        _tokenProvider.Received(1)
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRecovery), expireAt.DateTime);
        _tokenHandler.Received(1)
            .WriteToken(recoveryOptions);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ShouldNotGenerateRecoveryToken()
    {
        //Arrange
        var expected = nameof(_options.Value.TokenInfos);

        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, _accessTokenInfo
                }
            }
        });

        var userId = Guid.NewGuid();
        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);

        //Act
        var actual = Assert.Throws<ArgumentNullException>(() => tokenService.GenerateRecoveryToken(userId));

        //Assert
        Assert.Equal(expected, actual.ParamName);
    }

    [Fact]
    public void ShouldValidateRecoveryToken()
    {
        //Arrange
        var expireAt = _utcNow.AddMinutes(_recoveryTokenInfo.LifeTimeInMinutes);
        //todo resolve milliseconds problem
        // expireAt = expireAt.AddMilliseconds(-expireAt.Millisecond);
        // expireAt = expireAt.AddMicroseconds(-expireAt.Microsecond);
        var userId = Guid.NewGuid();

        var userIdAndExpireModel = new UserIdAndExpireModel(userId, expireAt);
        var expected = Result.Ok(userIdAndExpireModel);

        var claims = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });

        var token = new JwtSecurityToken(expires: expireAt.UtcDateTime);

        _tokenHandler.ValidateToken(RecoveryToken,
                Arg.Is<TokenValidationParameters>(x =>
                    x.ValidAudience == _recoveryTokenInfo.Audience
                    && x.ValidIssuer == _recoveryTokenInfo.Issuer),
                out Arg.Any<SecurityToken>())
            .Returns(x =>
            {
                x[2] = token;
                return new ClaimsPrincipal(claims);
            });

        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);

        //Act
        var actual = tokenService.ValidateRecoveryToken(RecoveryToken, _ct);

        //Assert
        Assert.Equivalent(expected, actual);
        _tokenHandler.Received(1)
            .ValidateToken(RecoveryToken, Arg.Is<TokenValidationParameters>(
                x => x.ValidAudience == _recoveryTokenInfo.Audience && x.ValidIssuer == _recoveryTokenInfo.Issuer
            ), out Arg.Any<SecurityToken>());
    }

    [Fact]
    public void ShouldNotValidateRecoveryTokenBecauseOptionsNotFound()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.PasswordChangeFailed);
        ;

        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, _accessTokenInfo
                }
            }
        });
        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);

        //Act
        var actual = tokenService.ValidateRecoveryToken(RecoveryToken, _ct);

        //Assert
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public void ShouldNotValidateRecoveryTokenBecauseClaimsNotFound()
    {
        // Arrange
        var expected = Result.Fail(MessageConstants.PasswordChangeFailed);
        var claims = new ClaimsIdentity();

        _tokenHandler.ValidateToken(RecoveryToken, Arg.Is<TokenValidationParameters>(
                x => x.ValidAudience == _recoveryTokenInfo.Audience
                     && x.ValidIssuer == _recoveryTokenInfo.Issuer), out Arg.Any<SecurityToken>())
            .Returns(new ClaimsPrincipal(claims));

        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);
        
        //Act
        var actual = tokenService.ValidateRecoveryToken(RecoveryToken, _ct);

        //Assert
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }
    
    [Fact]
    public void ShouldNotValidateRecoveryTokenBecauseTokenExpired()
    {
        // Arrange
        var expected = Result.Fail(MessageConstants.TokenExpired);
        
        _tokenHandler.ValidateToken(RecoveryToken, Arg.Is<TokenValidationParameters>(
                x => x.ValidAudience == _recoveryTokenInfo.Audience
                     && x.ValidIssuer == _recoveryTokenInfo.Issuer), out Arg.Any<SecurityToken>())
            .Returns(x =>
            {
                throw new SecurityTokenExpiredException();
            });

        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);
        
        //Act
        var actual = tokenService.ValidateRecoveryToken(RecoveryToken, _ct);

        //Assert
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public void ShouldNotValidateRecoveryTokenBecauseExceptionIsThrown()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.PasswordChangeFailed);
        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, _accessTokenInfo
                }
            }
        });

        var tokenService = new TokenService(_tokenHandler, DbHelper.CreateDbContext(), _options, _tokenProvider,
            _systemClock);

        //Act
        var actual = tokenService.ValidateRecoveryToken(RecoveryToken, _ct);

        //Assert
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldRemoveExpiredTokensAsync()
    {
        //Arrange
        _systemClock.UtcNow.Returns(_utcNow.DateTime);

        var expireNow = _utcNow.AddMinutes(-15);
        var expireIn15Minutes = _utcNow.AddMinutes(15);

        var user = new UserEntity
        {
            Id = Guid.NewGuid()
        };

        var revokedTokensList = new Fixture()
            .Build<RevokedTokenEntity>()
            .With(x => x.TokenExpireAt, expireNow)
            .CreateMany(3);

        var recoveryTokensList = new Fixture()
            .Build<RecoveryTokenEntity>()
            .With(x => x.ExpireAt, expireNow)
            .CreateMany(3);

        var tokensList = new Fixture()
            .Build<TokenEntity>()
            .With(x => x.UserId, user.Id)
            .With(x => x.User, user)
            .With(x => x.RefreshTokenExpireAt, expireNow)
            .CreateMany(3);

        var revokedToken = new Fixture()
            .Build<RevokedTokenEntity>()
            .With(x => x.TokenExpireAt, expireIn15Minutes)
            .Create();

        var recoveryToken = new Fixture()
            .Build<RecoveryTokenEntity>()
            .With(x => x.ExpireAt, expireIn15Minutes)
            .Create();

        var token = new Fixture()
            .Build<TokenEntity>()
            .With(x => x.UserId, user.Id)
            .With(x => x.User, user)
            .With(x => x.RefreshTokenExpireAt, expireIn15Minutes)
            .Create();

        
        var dbContext = DbHelper.CreateSqLiteDbContext();

        dbContext.Users.Add(user);
        
        dbContext.RevokedTokens.AddRange(revokedTokensList);
        dbContext.RevokedTokens.Add(revokedToken);
        
        dbContext.RecoveryTokens.AddRange(recoveryTokensList);
        dbContext.RecoveryTokens.Add(recoveryToken);
        
        dbContext.Tokens.AddRange(tokensList);
        dbContext.Tokens.Add(token);
        
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();
        
        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        await tokenService.RemoveExpiredTokensAsync(_ct);

        //Assert
        var shouldHaveOneRecoveryTokenLeft =await dbContext.RecoveryTokens.CountAsync(_ct);
        var shouldHaveOneRevokedTokenLeft = await dbContext.RevokedTokens.CountAsync(_ct);
        var shouldHaveOneTokenLeft = await dbContext.Tokens.CountAsync(_ct);
        Assert.Equal(1, shouldHaveOneRecoveryTokenLeft);
        Assert.Equal(1, shouldHaveOneRevokedTokenLeft);
        Assert.Equal(1, shouldHaveOneTokenLeft);

        var shouldBeOurRecoveryToken = dbContext.RecoveryTokens
            .Any(x => x.Id == recoveryToken.Id);
        var shouldBeOurRevokedToken = dbContext.RevokedTokens
            .Any(x => x.Id == revokedToken.Id);

        var shouldBeOurToken = dbContext.Tokens
            .Any(x => x.Id == token.Id);

        var result = shouldBeOurToken && shouldBeOurRecoveryToken && shouldBeOurRevokedToken;
        Assert.Equal(true, result);
    }
}