using API.Core.Enums;
using API.Core.Models;
using FluentResults;

namespace API.Core.Services;

public interface IUserService
{
    Task<Result> RegisterAsync(RegisterModel model, CancellationToken ct);
    Task<Result> DeleteAsync(string login, CancellationToken ct);
    Task<Result> UpdateFirstLastNameAsync(UpdateFirstLastNameModel updateFirstLastNameModel, CancellationToken ct);
    Task<Result> PasswordChangeAsync(ChangePasswordModel model, CancellationToken ct);
    Task<Result<TokenPairModel>> SingInAsync(SingInModel requestModel, CancellationToken ct);
    Task<List<UserModel>> GetUsersAsync(CancellationToken ct);
    Task<Result> AddRoleAsync(Guid userId, Role role, CancellationToken ct);
    Task<Result> RemoveRoleAsync(Guid userId, Role role, CancellationToken ct);
    Guid GetUserIdFromContext();
    Task<Result> SendRecoveryEmailAsync(string email, CancellationToken ct);
    Task<Result> ValidateTokenAndChangePasswordAsync(string token, string newPassword, CancellationToken ct);
}