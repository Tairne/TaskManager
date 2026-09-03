using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _config;

        public LoginService(UserRepository repository, IPasswordService passwordService, IConfiguration config)
        {
            this._repository = repository;
            this._passwordService = passwordService;
            this._config = config;
        }

        public async Task<TokenResponseDto?> LoginAsync(string login, string password, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByLoginAsync(login, cancellationToken);

            if (user == null || !_passwordService.Verify(password, user.Password))
            {
                return null;
            }

            var result = new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await RefreshAsync(user, cancellationToken)
            };

            return result;
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto request, CancellationToken cancellationToken)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken, cancellationToken);

            if (user == null)
                return null;

            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await RefreshAsync(user, cancellationToken)
            };
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Position", user.Position)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:Key")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Jwt:Issuer"),
                audience: _config.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> RefreshAsync(User user, CancellationToken cancellationToken)
        {
            var refreshToken = GenerateRefreshToken();
            await _repository.SaveRefreshToken(user, refreshToken, cancellationToken);
            return refreshToken;
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken, CancellationToken cancellationToken)
        {
            var user = await _repository.GetAsync(userId, cancellationToken);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }

            return user;
        }
    }
}
