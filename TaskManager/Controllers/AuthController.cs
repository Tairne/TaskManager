using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    /// <summary>
    /// Controller for authentification
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;

        /// <summary>
        /// Login and authentification
        /// </summary>
        /// <param name="loginService"></param>
        public AuthController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        /// <summary>
        /// Sign in API, get auth token
        /// </summary>
        /// <param name="request">Request object with login and password <see cref="LoginDto"/></param>
        /// <returns><see cref="TokenResponseDto"/></returns>
        /// <response code="200">Access and refresh tokens <a href='#model-TokenResponseDto'>TokenResponseDto</a></response>
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

        /// <summary>
        /// Getting new access and refresh token
        /// </summary>
        /// <param name="request">Object contains old refresh token and user Id <see cref="RefreshTokenDto"/></param>
        /// <returns><see cref="TokenResponseDto"/></returns>
        /// <response code="200">Access and refresh tokens <a href='#model-TokenResponseDto'>TokenResponseDto</a></response>
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
