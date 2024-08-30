﻿using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
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
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto requestDto)
    {
       return await _userService.RegisterAsync(requestDto.ToRequest());
    }
}