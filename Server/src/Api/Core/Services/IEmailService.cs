using FluentResults;

namespace API.Core.Services;

public interface IEmailService
{
    Result SendRecoveryEmail(string email, string token, CancellationToken ct);
}