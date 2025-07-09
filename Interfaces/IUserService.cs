using MiniAccountManagementSystem.Models;
namespace MiniAccountManagementSystem.Interfaces;

public interface IUserService
{
    Task<List<UserWithRoleViewModel>> GetAllUsersWithRolesAsync();
    Task<List<string>> GetAllRolesAsync();
    Task<bool> ChangeUserRoleAsync(string userId, string newRole);
}