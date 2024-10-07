namespace API.Controllers.Dtos;

internal sealed record BusinessErrorDto(
    List<string> Messages);