namespace API.Controllers.Dtos;

public sealed record SingInResponseDto(
    string AccessToken,
    string RefreshToken
);