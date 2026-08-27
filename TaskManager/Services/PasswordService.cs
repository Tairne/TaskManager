using Microsoft.AspNetCore.Identity;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string password, string passwordHash)
        {
            return _hasher.VerifyHashedPassword(
                null!,
                passwordHash,
                password) == PasswordVerificationResult.Success;
        }
    }
}
