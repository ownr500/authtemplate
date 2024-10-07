namespace API.Core.Models;

public record TokenPairModel(
    string AccessToken,
    string RefreshToken);