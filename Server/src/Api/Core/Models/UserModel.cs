using API.Core.Enums;

namespace API.Core.Models;

public record UserModel(
    Guid UserId,
    string FirstName,
    string LastName,
    string Login,
    List<Role> Roles
);