using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
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

        ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles); // remove all
        var result = await _userManager.AddToRoleAsync(user, newRole);

        if (result.Succeeded)
        {
            TempData["Success"] = $"Role updated to {newRole} for {user.Email}";
        }
        else
        {
            TempData["Error"] = "Failed to update role.";
        }

        return RedirectToAction("Index");
    }
}
