using API.Controllers.Dtos;
using API.Core.Services;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpDelete("{login}")]
    public async Task<IActionResult> Delete([FromRoute] string login, CancellationToken ct)
    {
        var result = await _userService.DeleteAsync(login, ct);
        if (result.IsSuccess) return NoContent();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateFirstLastName([FromBody] UpdateFirstLastNameRequestDto requestDto, CancellationToken ct)
    {
        var result = await _userService.UpdateFirstLastNameAsync(requestDto.ToModel(), ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}