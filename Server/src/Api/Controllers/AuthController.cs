using API.Controllers.Dtos;
using API.Core.Services;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto requestDto,
        CancellationToken ct)
    {
        var result = await _userService.RegisterAsync(requestDto.ToRequest(), ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(
            result.GetErrors()
        ));
    }

    [HttpPost("signin")]
    public async Task<ActionResult<SingInResponseDto>> SingInAsync([FromBody] SigninRequestDto signinRequestDto,
        CancellationToken ct)
    {
        var result = await _userService.SingInAsync(signinRequestDto.ToModel(), ct);

        if (result.IsSuccess) return new SingInResponseDto(result.Value.AccessToken, result.Value.RefreshToken);

        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}