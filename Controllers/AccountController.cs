using Microsoft.AspNetCore.Mvc;

namespace MiniAccountManagementSystem.Controllers;

public class AccountController : Controller
{
    public IActionResult Register()
    {
        return View();
    }
    
}