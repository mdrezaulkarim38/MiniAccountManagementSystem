using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IChartOfAccountService _accountService;
    private readonly IVoucherService _voucherService;
    private readonly IConfiguration _configuration;
    public AccountController(IConfiguration configuration, IChartOfAccountService accountService, IVoucherService voucherService)
    {
        _configuration = configuration;
        _accountService = accountService;
        _voucherService = voucherService;
    }

    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult ChartOfAccount()
    {
        var accounts = _accountService.GetAllAccounts();
        return View(accounts);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateAccount(AccountModel model)
    {
        if (ModelState.IsValid)
        {
            if (_accountService.CreateAccount(model, out string error))
            {
                TempData["Success"] = "Account created successfully.";
                return RedirectToAction("ChartOfAccount");
            }

            TempData["Error"] = "Error: " + error;
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult EditAccount(int id)
    {
        var account = _accountService.GetAccountById(id);
        if (account == null)
        {
            TempData["Error"] = "Account not found.";
            return RedirectToAction("ChartOfAccount");
        }

        return View(account);
    }

    [HttpPost]
    public IActionResult EditAccount(AccountModel model)
    {
        if (ModelState.IsValid)
        {
            if (_accountService.UpdateAccount(model, out string error))
            {
                TempData["Success"] = "Account updated successfully.";
                return RedirectToAction("ChartOfAccount");
            }

            TempData["Error"] = "Update failed: " + error;
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult DeleteAccount(int id)
    {
        if (_accountService.DeleteAccount(id, out string error))
        {
            TempData["Success"] = "Account deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Delete failed: " + error;
        }

        return RedirectToAction("ChartOfAccount");
    }

    public IActionResult VoucherList()
    {
        var list = _voucherService.GetVoucherList();
        ViewBag.Role = User.IsInRole("Admin") ? "Admin" :
                       User.IsInRole("Accountant") ? "Accountant" :
                       "Viewer";
        return View(list);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult CreateVoucher()
    {
        var model = new VoucherFormModel
        {
            VoucherDate = DateTime.Today,
            Entries = new List<VoucherEntryModel> { new(), new() }
        };

        ViewBag.VoucherTypes = _voucherService.GetVoucherTypes();
        ViewBag.Accounts = _voucherService.GetChartOfAccounts();
        return View(model);
    }

    [HttpPost]
    public IActionResult CreateVoucher(VoucherFormModel model)
    {
        if (ModelState.IsValid && _voucherService.CreateVoucher(model, out string error))
        {
            TempData["Success"] = "Voucher created successfully.";
            return RedirectToAction("VoucherList");
        }

        TempData["Error"] = "Voucher created unsuccessfully ";
        ViewBag.VoucherTypes = _voucherService.GetVoucherTypes();
        ViewBag.Accounts = _voucherService.GetChartOfAccounts();
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult EditVoucher(int id)
    {
        var model = _voucherService.GetVoucherById(id);
        if (model == null)
        {
            TempData["Error"] = "Voucher not found.";
            return RedirectToAction("VoucherList");
        }

        ViewBag.VoucherTypes = _voucherService.GetVoucherTypes();
        ViewBag.Accounts = _voucherService.GetChartOfAccounts();
        ViewBag.VoucherId = id;
        return View("EditVoucher", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult EditVoucher(int id, VoucherFormModel model)
    {
        if (ModelState.IsValid && _voucherService.UpdateVoucher(id, model, out string error))
        {
            TempData["Success"] = "Voucher updated successfully.";
            return RedirectToAction("VoucherList");
        }
        else
        {
            TempData["Error"] = "Voucher updated unsuccessfully.";        
        }

        ViewBag.VoucherTypes = _voucherService.GetVoucherTypes();
        ViewBag.Accounts = _voucherService.GetChartOfAccounts();
        ViewBag.VoucherId = id;
        return View("EditVoucher", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public IActionResult DeleteVoucher(int id)
    {
        if (_voucherService.DeleteVoucher(id, out string error))
        {
            TempData["Success"] = "Voucher deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Delete failed: " + error;
        }

        return RedirectToAction("VoucherList");
    }

}