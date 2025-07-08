using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountManagementSystem.Controllers;

[Authorize(Roles = "Admin,Accountant")]
public class AccountController : Controller
{
    public AccountController()
    {

    }
    public IActionResult ChartOfAccount()
    {
        return View();
    }
    
    public IActionResult VoucherList()
    {
        return View();
    }
}