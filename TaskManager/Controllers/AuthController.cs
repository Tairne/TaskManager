using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public AuthController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto?>> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
        {
            var result = await _loginService.LoginAsync(request.Login, request.Password, cancellationToken);

            if (result == null)
            {
                return BadRequest("Invalid Credentials");
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshTokenDto request, CancellationToken cancellationToken)
        {
            var result = await _loginService.RefreshTokenAsync(request, cancellationToken);

            if (result == null || result.AccessToken == null || result.RefreshToken == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(result);
        }
    }
}
