using API.Core.Enums;

namespace API.Controllers.Dtos;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Login,
    List<Role> Roles
);