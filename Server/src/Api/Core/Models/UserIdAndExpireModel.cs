using System.Security.Claims;

namespace API.Core.Models;

public record UserIdAndExpireModel(
    Guid UserId,
    DateTimeOffset ExpireAt);