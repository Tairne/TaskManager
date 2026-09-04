using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetAccountInfo([FromRoute] int id, CancellationToken cancellationToken)
        {
            if (!User.IsInRole("Admin") && id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            var user = await _accountService.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                return NotFound($"Account id {id} not exists");
            }

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<User>> CreateAccount([FromBody] CreateUserDto accountData, CancellationToken cancellationToken)
        {
            var result = await _accountService.CreateAccountAsync(accountData, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] UpdateUserDto accountData, CancellationToken cancellationToken)
        {
            if (!User.IsInRole("Admin") && id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            bool result = await _accountService.UpdateAccountAsync(id, accountData, cancellationToken);

            if (!result)
            {
                return NotFound($"Account id {id} not exists");
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AllUsersDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _accountService.GetAllAsync(cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("cpwd")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto pwd, CancellationToken cancellationToken)
        {
            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            bool result = await _accountService.ChangePasswordAsync(id, pwd, cancellationToken);

            if (!result)
                return BadRequest("Invalid current password");

            return NoContent();
        }
    }
}
