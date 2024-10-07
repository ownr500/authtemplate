namespace API.Controllers.Dtos;

public sealed record RefreshResponseDto(
    string AccessToken,
    string RefreshToken
);