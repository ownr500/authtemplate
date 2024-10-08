﻿using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
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
    public async Task<IActionResult> Change([FromBody] ChangeRequestDto requestDto, CancellationToken ct)
    {
        var result = await _userService.ChangeAsync(requestDto.ToRequest(), ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}