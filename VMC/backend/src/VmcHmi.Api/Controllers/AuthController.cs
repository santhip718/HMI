using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VmcHmi.Application;
using VmcHmi.Application.DTOs;
using VmcHmi.Application.Interfaces;
using VmcHmi.Application.Queries;

namespace VmcHmi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRequestHandler<LoginQuery, LoginResponse> _loginHandler;
    private readonly IAppLogger<AuthController> _logger;

    public AuthController(IRequestHandler<LoginQuery, LoginResponse> loginHandler, IAppLogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        try
        {
            var response = await _loginHandler.HandleAsync(new LoginQuery
            {
                Username = request.Username,
                Password = request.Password
            });
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }
}
