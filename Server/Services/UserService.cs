﻿using System.Security.Claims;
using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using API.Constants;
using API.Models;
using API.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(ApplicationDbContext dbContext, ITokenService tokenService, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var userExists = _dbContext.Users.Any(x => x.LoginNormalized == request.Login.ToLower());
        if (userExists) return Result.Fail(MessageConstants.UserAlreadyRegistered);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Age = request.Age,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Login = request.Login,
            LoginNormalized = request.Login.ToLower(),
            PasswordHash = GeneratePasswordHash(request.Password)
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
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

    public async Task<Result> ChangeAsync(ChangeRequest changeRequest)
    {
        var userId = GetUserIdFromContext();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        user.FirstName = changeRequest.FirstName;
        user.LastName = changeRequest.LastName;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> PasswordChangeAsync(PasswordChangeModel model)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.LoginNormalized == model.Login.ToLower());

        if (user == null)
        {
            return Result.Fail(MessageConstants.UserNotFound);
        }

        if (user.PasswordHash != GeneratePasswordHash(model.OldPassword))
            return Result.Fail(MessageConstants.OldPasswordNotMatch);

        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<SinginReponseModel>> SingInAsync(SinginRequestModel requestModel, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(requestModel.Login, ct);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        var passwordMatch = user.PasswordHash == GeneratePasswordHash(requestModel.Password);
        if (!passwordMatch) return Result.Fail(MessageConstants.InvalidCredentials);
        
        var tokenModel = await _tokenService.GenerateNewTokenModelAsync(user.Id, ct);
        
        return Result.Ok(new SinginReponseModel(
            tokenModel.AccessToken,
            tokenModel.RefreshToken
        ));

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

    private Guid GetUserIdFromContext()
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
}