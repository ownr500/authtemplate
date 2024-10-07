namespace API.Core.Models;

public record ChangePasswordModel(
    string Login,
    string CurrentPassword,
    string NewPassword
    );