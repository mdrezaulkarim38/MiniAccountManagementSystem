using Microsoft.AspNetCore.Identity;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<UserWithRoleViewModel>> GetAllUsersWithRolesAsync()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserWithRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserWithRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email!,
                CurrentRole = roles.FirstOrDefault() ?? "None"
            });
        }

        return model;
    }

    public Task<List<string>> GetAllRolesAsync()
    {
        return Task.FromResult(_roleManager.Roles.Select(r => r.Name!).ToList());
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, newRole);

        return result.Succeeded;
    }
}