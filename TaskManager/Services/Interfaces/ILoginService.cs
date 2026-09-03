using TaskManager.DB.DTO;

namespace TaskManager.Services.Interfaces
{
    public interface ILoginService
    {
        Task<TokenResponseDto?> LoginAsync(string login, string password, CancellationToken cancellationToken);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto request,  CancellationToken cancellationToken);
    }
}
