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

        /// <summary>
        /// Get user information by id (only admins and current user allowed)
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>User object <see cref="User"/></returns>
        /// <response code="200">Account information <a href='#model-User'>User</a></response>
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

        /// <summary>
        /// Create new account
        /// </summary>
        /// <param name="accountData">New user information <a href='#model-CreateUserDto'>CreateUserDto</a></param>
        /// <returns>User object <see cref="User"/></returns>
        /// <response code="200">New account information <a href='#model-User'>User</a></response>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<User>> CreateAccount([FromBody] CreateUserDto accountData, CancellationToken cancellationToken)
        {
            var result = await _accountService.CreateAccountAsync(accountData, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Update account information (only admins and current user allowed)
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="accountData">User information <a href='#model-UpdateUserDto'>UpdateUserDto</a></param>
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

        /// <summary>
        /// Get all accounts list
        /// </summary>
        /// <remarks>You can get just ids and usernames for user selection</remarks>
        /// <returns>User object <see cref="List{AllUsersDto}"/></returns>
        /// <response code="200">List of users ids and names List{<a href='#model-User'>User</a>}</response>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AllUsersDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _accountService.GetAllAsync(cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Change password (only current user allowed)
        /// </summary>
        /// <param name="pwd">Object with current and new password <a href='#model-ChangePasswordDto'>ChangePasswordDto</a></param>
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
