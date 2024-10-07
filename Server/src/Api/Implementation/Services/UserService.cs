using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Services;
using API.Extensions;
using API.Infrastructure;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace API.Implementation.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(ApplicationDbContext dbContext, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailService = emailService;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> RegisterAsync(RegisterModel model, CancellationToken ct)
    {
        var userExists = _dbContext.Users.Any(x => x.LoginNormalized == model.Login.ToLower());
        if (userExists) return Result.Fail(MessageConstants.UserAlreadyRegistered);

        var user = new UserEntity
        {
            Age = model.Age,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            EmailNormalized = model.Email.ToLower(),
            Login = model.Login,
            LoginNormalized = model.Login.ToLower(),
            PasswordHash = GeneratePasswordHash(model.Password)
        };

        user.UserRoles.Add(new UserRoleEntity { RoleId = RoleConstants.UserRoleId });
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string login, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(login, ct);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> UpdateFirstLastNameAsync(UpdateFirstLastNameModel updateFirstLastNameModel, CancellationToken ct)
    {
        var userId = GetUserIdFromContext();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        user.FirstName = updateFirstLastNameModel.FirstName;
        user.LastName = updateFirstLastNameModel.LastName;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> PasswordChangeAsync(ChangePasswordModel model, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(model.Login.ToLower(), ct);

        if (user == null)
        {
            return Result.Fail(MessageConstants.UserNotFound);
        }

        if (user.PasswordHash != GeneratePasswordHash(model.CurrentPassword))
            return Result.Fail(MessageConstants.CurrentPasswordNotMatch);

        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<TokenPairModel>> SingInAsync(SingInModel requestModel, CancellationToken ct)
    {
        var passwordHash = GeneratePasswordHash(requestModel.Password);

        var model = await _dbContext.Users
            .Where(x => x.LoginNormalized == requestModel.Login.ToLower()
                        && x.PasswordHash == passwordHash
            )
            .Select(x => new
            {
                userId = x.Id,
                userRoles = x.UserRoles.Select(u => u.Role.Role).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (model is null) return Result.Fail(MessageConstants.InvalidCredentials);

        var tokenModel = await _tokenService.GenerateTokenPairAsync(model.userId, model.userRoles, ct);
        
        return Result.Ok(new TokenPairModel(
            tokenModel.AccessToken,
            tokenModel.RefreshToken
        ));
    }

    public async Task<List<UserModel>> GetUsersAsync(CancellationToken ct)
    {
        var users = await _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(u => u.Role)
            .Select(x => x.ToModel())
            .ToListAsync(ct);
        return users;
    }

    public async Task<Result> AddRoleAsync(Guid userId, Role role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleIds[role];
        var roleExists = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (roleExists) return Result.Fail(MessageConstants.UserAlreadyHasRole);
        var userRole = new UserRoleEntity
        {
            RoleId = roleId,
            UserId = userId
        };

        await _dbContext.UserRoles.AddAsync(userRole, ct);
        await _dbContext.SaveChangesAsync(ct);
        await _tokenService.RevokeTokensAsync(userId, ct);
        return Result.Ok();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, Role role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleIds[role];
        var existingRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (existingRole is null) return Result.Fail(MessageConstants.UserHasNoRole);
        _dbContext.UserRoles.Remove(existingRole);
        await _dbContext.SaveChangesAsync(ct);

        await _tokenService.RevokeTokensAsync(userId, ct);
        return Result.Ok();
    }

    public Guid GetUserIdFromContext()
    {
        var nameIdentifier = _contextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(
                x => string.Equals(x.Type, ClaimTypes.NameIdentifier,
                    StringComparison.InvariantCultureIgnoreCase))?.Value;
        if (!Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new ArgumentNullException(nameof(ClaimTypes.NameIdentifier));
        }

        return userId;
    }

    public async Task<Result> SendRecoveryEmailAsync(string email, CancellationToken ct)
    {
        var userId = await _dbContext.Users
            .Where(x => x.EmailNormalized == email.ToLower())
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
        
        if (userId == Guid.Empty) return Result.Ok();
        
        var token = _tokenService.GenerateRecoveryToken(userId);
        _emailService.SendRecoveryEmail(email, token, ct);
        return Result.Ok();
    }

    public async Task<Result> ValidateTokenAndChangePasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        var result = _tokenService.ValidateRecoveryToken(token, ct);
        var tokenNotExist = await _tokenService.CheckRecoveryTokenExists(token, ct);

        if (!result.IsSuccess || !tokenNotExist.IsSuccess) return Result.Fail(result.Errors);
        await _tokenService.AddRecoveryTokenAsync(token, result.Value.ExpireAt, ct);
        await SetPasswordAsync(result.Value.UserId, newPassword, ct);
        return Result.Ok();
    }

    private async Task SetPasswordAsync(Guid userId, string newPassword, CancellationToken ct)
    {
        var passwordHash = GeneratePasswordHash(newPassword);
        await _dbContext.Users
            .Where(x=> x.Id == userId)
            .ExecuteUpdateAsync(x => 
                x.SetProperty(p => 
                    p.PasswordHash, passwordHash), ct);
    }

    private static string GeneratePasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    private async Task<UserEntity?> GetUserByLoginAsync(string login, CancellationToken ct)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.LoginNormalized == login.ToLower(), ct);
    }
}

