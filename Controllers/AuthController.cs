using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.RegisterAsync(model);
            TempData[success ? "Success" : "Error"] = message;

            if (success)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", message);
        }

        return View(model);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.LoginAsync(model);
            TempData[success ? "Success" : "Error"] = message;

            if (success)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", message);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
