using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string Message)> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
}