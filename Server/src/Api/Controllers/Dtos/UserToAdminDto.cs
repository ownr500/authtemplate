namespace API.Controllers.Dtos;

public record UserToAdminDto(
    string Login,
    string SuperPassword);