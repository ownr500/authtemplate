﻿using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IUserService _userService;

    public PasswordController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Change([FromBody] PasswordChangeDto passwordChangeDto)
    {
        var result = await _userService.PasswordChangeAsync(passwordChangeDto.ToModel(), HttpContext.RequestAborted);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [AllowAnonymous]
    [HttpPost("send-recovery-email")]
    public async Task<IActionResult> SendRecoveryEmail([FromBody] SendRecoveryEmailRequestDto requestDto)
    {
        var result = await _userService.SendRecoveryEmailAsync(requestDto.Email, HttpContext.RequestAborted);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}