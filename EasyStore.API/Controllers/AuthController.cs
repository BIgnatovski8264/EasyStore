using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EasyStore.Common.Requests.Auth;
using EasyStore.Common.Responses.Auth;
using EasyStore.Core.StaticClasses;
using EasyStore.Domain.Interfaces;

namespace EasyStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserRequest request)
    {
        RegisterUserResponse? user = await authService.RegisterAsync(request);

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginUserRequest request)
    {
        TokenResponse? result = await authService.LoginAsync(request);
        if (result is null)
        {
            return BadRequest("Invalid username or password.");
        }

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult<TokenResponse>> Logout()
    {
        if (!await authService.LogoutAsync())
        {
            return Unauthorized();
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        TokenResponse? result = await authService.RefreshTokensAsync(request);
        if (result is null || result.AccessToken is null || result.RefreshToken is null)
        {
            return Unauthorized("Invalid refresh token.");
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated!");
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are and admin!");
    }
}
