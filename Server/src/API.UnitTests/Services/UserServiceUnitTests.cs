using System.Security.Claims;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Services;
using API.Implementation.Services;
using API.UnitTests.Helpers;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace API.UnitTests.Services;

public class UserServiceUnitTests
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly CancellationToken _ct = CancellationToken.None;

    private const string FirstUserLogin = "FirstUserLogin";
    private const string SecondUserLogin = "SecondUserLogin";

    private const string AccessToken = "accessToken";
    private const string RefreshToken = "refreshToken";
    private const string RecoveryToken = "recoveryToken";

    private const string Email = "john@email.com";
    private const string AnotherEmail = "anotherjohn@email.com";
    private const int Age = 30;
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Password = "12345";
    private const string NewPassword = "00000";
    private const string UserId = "21C31E4D-2953-450A-91B1-7C4FAC9743C2";
    private const string UserRoleId = "6DBB3F20-3F06-4076-8E9E-8170228276E0";
    private const int RecoveryTokenLifeTimeInMinutes = 15;

    public UserServiceUnitTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _emailService = Substitute.For<IEmailService>();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    [Fact]
    public async Task ShouldDeleteAsync()
    {
        //Arrange
        var firstUser = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
        };

        var secondUser = new UserEntity
        {
            LoginNormalized = SecondUserLogin.ToLower(),
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(firstUser);
        dbContext.Users.Add(secondUser);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.DeleteAsync(FirstUserLogin.ToUpper(), _ct);

        //Assert
        var firstUserExists = dbContext.Users.Any(x => x.LoginNormalized == firstUser.LoginNormalized);
        var secondUserExists = dbContext.Users.Any(x => x.LoginNormalized == secondUser.LoginNormalized);
        Assert.Equal(false, firstUserExists);
        Assert.Equal(true, secondUserExists);
        Assert.Equal(true, actual.IsSuccess);
    }


    [Fact]
    public async Task ShouldNotDeleteAsync()
    {
        //Arrange
        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        var expectedResult = new Result().WithErrors(errors);
        var firstUser = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(firstUser);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.DeleteAsync(SecondUserLogin.ToUpper(), _ct);

        //Assert
        var firstUserExists = dbContext.Users.Any(x => x.LoginNormalized == firstUser.LoginNormalized);
        var secondUserExists = dbContext.Users.Any(x => x.LoginNormalized == SecondUserLogin.ToLower());
        Assert.Equal(true, firstUserExists);
        Assert.Equal(false, secondUserExists);
        Assert.Equivalent(expectedResult, actual);
    }

    [Fact]
    public async Task ShouldRegisterAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var registerModel = new RegisterModel(FirstName, LastName, Email, Age, FirstUserLogin, Password);
        var expected = Result.Ok();
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.RegisterAsync(registerModel, _ct);

        //Assert
        var userCreated = dbContext.Users.Any(x => x.LoginNormalized == FirstUserLogin.ToLower());
        Assert.Equal(true, userCreated);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldRegisterWithUserRoleAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var registerModel = new RegisterModel(FirstName, LastName, Email, Age, FirstUserLogin, Password);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.RegisterAsync(registerModel, _ct);

        //Assert
        var user = await dbContext.Users.Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.LoginNormalized == FirstUserLogin.ToLower(), _ct);
        var a = user.UserRoles.First(x => x.RoleId == RoleConstants.UserRoleId);
        Assert.Equal(RoleConstants.UserRoleId, a.RoleId);
        Assert.Equivalent(Result.Ok(), actual);
    }

    [Fact]
    public async Task ShouldNotRegisterAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var user = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower()
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var registerModel = new RegisterModel(FirstName, LastName, Email, Age, FirstUserLogin, Password);
        var errorMessage = MessageConstants.UserAlreadyRegistered;
        var errors = new List<string> { errorMessage };
        var expected = new Result().WithErrors(errors);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.RegisterAsync(registerModel, _ct);

        //Assert
        var userExists = dbContext.Users.Any(x => x.LoginNormalized == FirstUserLogin.ToLower());
        Assert.Equal(true, userExists);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldUpdateFistLastNameAsync()
    {
        //Arrange
        var updatedFirstName = "Michael";
        var updatedLastName = "Smith";
        var model = new UpdateFirstLastNameModel(updatedFirstName, updatedLastName);

        var userId = Guid.Parse("E5608234-8E55-4122-95AC-6FC514CB5BA0");
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        _contextAccessor.HttpContext.User.Claims.Returns(userClaims);

        var user = new UserEntity
        {
            Id = userId,
            LoginNormalized = FirstUserLogin.ToLower(),
            FirstName = FirstName,
            LastName = LastName
        };
        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.UpdateFirstLastNameAsync(model, _ct);

        //Assert
        var updatedUser = await dbContext.Users.FirstAsync(x => x.Id == userId, _ct);
        Assert.Equal(updatedFirstName, updatedUser.FirstName);
        Assert.Equal(updatedLastName, updatedUser.LastName);
        Assert.Equivalent(Result.Ok(), actual);
    }

    [Fact]
    public async Task ShouldNotUpdateFistLastNameAsync()
    {
        //Arrange
        var updatedFirstName = "Michael";
        var updatedLastName = "Smith";
        var model = new UpdateFirstLastNameModel(updatedFirstName, updatedLastName);

        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        var expected = new Result().WithErrors(errors);

        var firstUserId = Guid.Parse("E5608234-8E55-4122-95AC-6FC514CB5BA0");
        var secondUserId = Guid.Parse("8753F409-C9AD-4525-9A64-C252719E386F");
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, secondUserId.ToString()) };
        _contextAccessor.HttpContext.User.Claims.Returns(userClaims);

        var firstUser = new UserEntity
        {
            Id = firstUserId,
            LoginNormalized = FirstUserLogin.ToLower(),
            FirstName = FirstName,
            LastName = LastName
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(firstUser);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.UpdateFirstLastNameAsync(model, _ct);

        //Assert
        var updatedUser = await dbContext.Users.FirstAsync(x => x.Id == firstUserId, _ct);
        Assert.Equal(FirstName, updatedUser.FirstName);
        Assert.Equal(LastName, updatedUser.LastName);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldChangePasswordAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var model = new ChangePasswordModel(FirstUserLogin, Password, NewPassword);
        var user = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.PasswordChangeAsync(model, _ct);

        //Assert
        var userUpdated = await dbContext.Users.FirstOrDefaultAsync(x => x.LoginNormalized == FirstUserLogin.ToLower());
        Assert.Equal(userUpdated.PasswordHash, PasswordHelper.GeneratePasswordHash(NewPassword));
        Assert.Equivalent(Result.Ok(), actual);
    }

    [Fact]
    public async Task ShouldNotChangePasswordWithUserNotFoundAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var model = new ChangePasswordModel(SecondUserLogin, Password, NewPassword);
        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        var expected = new Result().WithErrors(errors);
        var user = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.PasswordChangeAsync(model, _ct);

        //Assert
        var userUpdated =
            await dbContext.Users.FirstOrDefaultAsync(x => x.LoginNormalized == SecondUserLogin.ToLower(), _ct);
        Assert.Equal(true, userUpdated is null);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldNotChangePasswordWithCurrentPasswordNotMatchAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var model = new ChangePasswordModel(FirstUserLogin, NewPassword, NewPassword);
        var errorMessage = MessageConstants.CurrentPasswordNotMatch;
        var errors = new List<string> { errorMessage };
        var expected = new Result().WithErrors(errors);
        var user = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.PasswordChangeAsync(model, _ct);

        //Assert
        var userUpdated =
            await dbContext.Users.FirstOrDefaultAsync(x => x.LoginNormalized == FirstUserLogin.ToLower(), _ct);
        Assert.Equal(PasswordHelper.GeneratePasswordHash(Password), userUpdated.PasswordHash);
        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(new[] { Role.User })]
    [InlineData(new[] { Role.User, Role.Admin })]
    public async Task ShouldSingInAsync(Role[] roles)
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var tokenPairModel = new TokenPairModel(AccessToken, RefreshToken);
        var singInModel = new SingInModel(FirstUserLogin, Password);
        var expected = new Result<TokenPairModel>().WithValue(tokenPairModel);
        var roleEntities = roles.Select(x => new RoleEntity
        {
            Id = Guid.NewGuid(),
            Role = x
        }).ToList();

        var userRoles = roleEntities.Select(x => new UserRoleEntity
        {
            Id = Guid.NewGuid(),
            RoleId = x.Id
        }).ToList();

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = FirstUserLogin,
            LoginNormalized = FirstUserLogin.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password),
            UserRoles = userRoles
        };
        dbContext.Roles.AddRange(roleEntities);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        _tokenService.GenerateTokenPairAsync(user.Id, Arg.Is<IReadOnlyCollection<Role>>(x =>
                x.Intersect(roles).Count() == roles.Count()
                && x.Count == roles.Count()), _ct)
            .Returns(Task.FromResult(tokenPairModel));

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = await userService.SingInAsync(singInModel, _ct);

        //Assert
        await _tokenService.Received(1)
            .GenerateTokenPairAsync(user.Id, Arg.Is<IReadOnlyCollection<Role>>(x =>
                x.Intersect(roles).Count() == roles.Count()
                && x.Count == roles.Count()), _ct);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task ShouldNotSingInAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var expected = Result.Fail(MessageConstants.InvalidCredentials);
        var singInModel = new SingInModel(SecondUserLogin, Password);

        var role = new RoleEntity
        {
            Id = Guid.Parse(UserRoleId),
            Role = Role.User
        };

        var user = new UserEntity
        {
            Login = FirstUserLogin,
            LoginNormalized = FirstUserLogin.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id
                }
            }
        };
        dbContext.Roles.Add(role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);


        //Act
        var actual = await userService.SingInAsync(singInModel, _ct);

        //Assert
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldGetUsersAsync()
    {
        //Arrange
        var role = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Role = Role.User
        };

        var firstUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = FirstName,
            LastName = LastName,
            Login = FirstUserLogin,
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = role.Id
                }
            }
        };
        var secondUser = new UserEntity
        {
            FirstName = FirstName,
            LastName = LastName,
            Login = SecondUserLogin,
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = role.Id
                }
            }
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Add(role);
        dbContext.Add(firstUser);
        dbContext.Add(secondUser);
        await dbContext.SaveChangesAsync(_ct);

        var expected = new List<UserModel>
        {
            new(
                firstUser.Id, firstUser.FirstName, firstUser.LastName, firstUser.Login,
                firstUser.UserRoles.Select(x => x.Role.Role).ToList()
            ),
            new(
                secondUser.Id, secondUser.FirstName, secondUser.LastName, secondUser.Login,
                secondUser.UserRoles.Select(x => x.Role.Role).ToList()
            )
        };
        
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = userService.GetUsersAsync(_ct);

        //Assert
        Assert.Equivalent(expected, actual.Result);
    }

    [Fact]
    public async Task ShouldAddRoleAsync()
    {
        //Arrange
        var expected = Result.Ok();
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.AddRoleAsync(user.Id, Role.User, _ct);

        //Assert
        await _tokenService.Received(1)
            .RevokeTokensAsync(user.Id, _ct);

        Assert.True(dbContext.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == RoleConstants.UserRoleId));
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task ShouldNotAddRoleAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.UserAlreadyHasRole);
        
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

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.AddRoleAsync(user.Id, Role.User, _ct);

        //Assert
        await _tokenService.Received(0)
            .RevokeTokensAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldRemoveRoleAsync()
    {
        //Arrange
        var expected = Result.Ok();
        
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

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.RemoveRoleAsync(user.Id, Role.User, _ct);

        //Assert
        await _tokenService.Received(1)
            .RevokeTokensAsync(user.Id, _ct);

        Assert.False(dbContext.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == RoleConstants.UserRoleId));
        Assert.Equivalent(expected, actual);
    }    
    
    
    [Fact]
    public async Task ShouldNotRemoveRoleAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.UserHasNoRole);
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>()
        };

        var dbContext = DbHelper.CreateSqLiteDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.RemoveRoleAsync(user.Id, Role.User, _ct);

        //Assert
        await _tokenService.Received(0)
            .RevokeTokensAsync(user.Id, _ct);

        Assert.False(dbContext.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == RoleConstants.UserRoleId));
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public void ShouldGetUserIdFromContext()
    {
        //Arrane
        var expected = Guid.Parse(UserId);
        
        var userClaims = new List<Claim> { new(ClaimTypes.NameIdentifier, UserId) };
        _contextAccessor.HttpContext.User.Claims.Returns(userClaims);

        var dbContext = DbHelper.CreateDbContext();
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = userService.GetUserIdFromContext();

        //Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ShouldNotGetUserIdFromContext()
    {
        //Arrane
        var expected = nameof(ClaimTypes.NameIdentifier);

        var userClaims = new List<Claim>();
        _contextAccessor.HttpContext.User.Claims.Returns(userClaims);

        var dbContext = DbHelper.CreateDbContext();
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);

        //Act
        var actual = Assert.Throws<ArgumentNullException>(() => userService.GetUserIdFromContext());

        //Assert
        Assert.Equal(expected, actual.ParamName);
    }

    [Fact]
    public async Task ShouldSendRecoveryEmailAsync()
    {
        //Assert
        var expected = Result.Ok();
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = Email,
            EmailNormalized = Email.ToLower()
        };
        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        
        _tokenService.GenerateRecoveryToken(user.Id).Returns(RecoveryToken);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.SendRecoveryEmailAsync(Email, _ct);

        //Assert
        _tokenService.Received(1)
            .GenerateRecoveryToken(user.Id);
        _emailService.Received(1)
            .SendRecoveryEmail(Email, RecoveryToken, _ct);
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task ShouldNotSendRecoveryEmailAsync()
    {
        //Assert
        var expected = Result.Ok();
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = Email,
            EmailNormalized = Email.ToLower()
        };
        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.SendRecoveryEmailAsync(AnotherEmail, _ct);

        //Assert
        _tokenService.Received(0)
            .GenerateRecoveryToken(user.Id);
        _emailService.Received(0)
            .SendRecoveryEmail(Email, RecoveryToken, _ct);
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task ShouldValidateTokenAndChangePasswordAsync()
    {
        //Arrange
        var expected = Result.Ok();
        
        var dbContext = DbHelper.CreateSqLiteDbContext();
        var user = new UserEntity
        {
            Id = Guid.Parse(UserId),
            Email = Email,
            EmailNormalized = Email.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var model = new UserIdAndExpireModel(Guid.Parse(UserId), DateTime.UtcNow.AddMinutes(RecoveryTokenLifeTimeInMinutes));
        _tokenService.ValidateRecoveryToken(RecoveryToken, _ct).Returns(model);
        _tokenService.CheckRecoveryTokenExists(RecoveryToken, _ct).Returns(Result.Ok());

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.ValidateTokenAndChangePasswordAsync(RecoveryToken, NewPassword, _ct);

        //Assert
        _tokenService.Received(1)
            .ValidateRecoveryToken(RecoveryToken, _ct);
        await _tokenService.Received(1)
            .AddRecoveryTokenAsync(RecoveryToken, model.ExpireAt, _ct);
        
        var updatedUser = await dbContext.Users.FirstAsync(x => x.EmailNormalized == Email.ToLower(), _ct);
        Assert.Equal(PasswordHelper.GeneratePasswordHash(NewPassword), updatedUser.PasswordHash);
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task ShouldNotValidateTokenAndChangePasswordAsync()
    {
        //Arrange
        var errors = new List<string>
        {
            "Error 1",
            "Error 2"
        };
        
        var model = Result.Fail(errors);
        _tokenService.ValidateRecoveryToken(RecoveryToken, _ct).Returns(model);

        var expected = Result.Fail(model.Errors);

        var dbContext = DbHelper.CreateSqLiteDbContext();
        var user = new UserEntity
        {
            Id = Guid.Parse(UserId),
            Email = Email,
            EmailNormalized = Email.ToLower(),
            PasswordHash = PasswordHelper.GeneratePasswordHash(Password)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.ValidateTokenAndChangePasswordAsync(RecoveryToken, NewPassword, _ct);

        //Assert
        _tokenService.Received(1)
            .ValidateRecoveryToken(RecoveryToken, _ct);
        await _tokenService.Received(0)
            .AddRecoveryTokenAsync(RecoveryToken, DateTime.UtcNow, _ct);
        
        var updatedUser = await dbContext.Users.FirstAsync(x => x.EmailNormalized == Email.ToLower(), _ct);
        Assert.Equal(PasswordHelper.GeneratePasswordHash(Password), updatedUser.PasswordHash);
        Assert.Equivalent(expected, actual);
    }
}