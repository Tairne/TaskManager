using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using System.Threading.Tasks;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetAccountInfo([FromRoute] int id, CancellationToken cancellationToken)
        {
            // TODO: Get id from Auth or Admin only
            var user = await _accountService.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                return NotFound($"Account id {id} not exists");
            }

            return Ok(user);
        }

        // AllowAnonimous
        [HttpPost]
        public async Task<ActionResult<User>> CreateAccount([FromBody] CreateUserDto accountData, CancellationToken cancellationToken)
        {
            var result = await _accountService.CreateAccountAsync(accountData, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] UpdateUserDto accountData, CancellationToken cancellationToken)
        {
            // TODO: Get id from Auth or Admin only
            bool result = await _accountService.UpdateAccountAsync(id, accountData, cancellationToken);

            if (!result)
            {
                return NotFound($"Account id {id} not exists");
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<List<AllUsersDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _accountService.GetAllAsync(cancellationToken);

            return Ok(result);
        }

        [HttpPost("cpwd/{id}")]
        public async Task<IActionResult> ChangePassword([FromRoute] int id, [FromBody] ChangePasswordDto pwd, CancellationToken cancellationToken)
        {
            bool result = await _accountService.ChangePasswordAsync(id, pwd, cancellationToken);

            if (!result)
                return BadRequest("Invalid current password");

            return NoContent();
        }
    }
}
