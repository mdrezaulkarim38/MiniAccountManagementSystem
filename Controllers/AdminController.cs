using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Interfaces;

namespace MiniAccountManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _userService.GetAllUsersWithRolesAsync();
        ViewBag.AllRoles = await _userService.GetAllRolesAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var result = await _userService.ChangeUserRoleAsync(userId, newRole);
        TempData[result ? "Success" : "Error"] = result
            ? $"Role updated to {newRole}"
            : "Failed to update role.";

        return RedirectToAction("Index");
    }
}
