namespace API.Core.Models;

public record RegisterModel(
    string FirstName,
    string LastName,
    string Email,
    int Age,
    string Login,
    string Password
);