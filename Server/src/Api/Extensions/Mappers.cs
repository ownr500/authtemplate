using System.Security.Claims;
using API.Controllers.Dtos;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Options;
using FluentResults;

namespace API.Extensions;

public static class Mappers
{
    public static RegisterModel ToRequest(this RegisterRequestDto dto)
    {
        return new RegisterModel(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.Age,
            dto.Login,
            dto.Password
        );
    }

    public static UpdateFirstLastNameModel ToModel(this UpdateFirstLastNameRequestDto requestDto)
    {
        return new UpdateFirstLastNameModel(
            requestDto.FirstName,
            requestDto.LastName
        );
    }

    public static ChangePasswordModel ToModel(this PasswordChangeDto dto)
    {
        return new ChangePasswordModel(
            Login: dto.Login,
            CurrentPassword: dto.CurrentPassword,
            NewPassword: dto.NewPassword
        );
    }

    public static SingInModel ToModel(this SigninRequestDto dto)
    {
        return new SingInModel(
            dto.Login,
            dto.Password);
    }

    public static List<string> GetErrors(this Result result)
    {
        return result.Errors.Select(x => x.Message).ToList();
    }

    public static List<string> GetErrors<T>(this Result<T> result)
    {
        return result.Errors.Select(x => x.Message).ToList();
    }

    public static UserModel ToModel(this UserEntity user)
    {
        return new UserModel(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Login,
            user.UserRoles.Select(x => x.Role.Role).ToList()
        );
    }

    public static List<UserDto> ToDtoList(this List<UserModel> list)
    {
        var userDtos = list.Select(x => x.ToDto()).ToList();
        return userDtos;
    }

    public static GenerateTokenModel ToModel(this TokenInfo info, List<Claim> claims)
    {
        return new GenerateTokenModel(claims, info);
    }

    public static List<Claim> ToClaims(this IReadOnlyCollection<Role> roles)
    {
        return roles
            .Select(x => new Claim(ClaimTypes.Role, x.ToString()))
            .ToList();
    }

    private static UserDto ToDto(this UserModel model)
    {
        return new UserDto(
            model.UserId,
            model.FirstName,
            model.LastName,
            model.Login,
            model.Roles
        );
    }
}