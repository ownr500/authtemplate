namespace API.Controllers.Dtos;

public record UserListResponseDto(
    List<UserDto> UsersDtos);