using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountManagementSystem.Controllers;

[Authorize(Roles = "Admin,Accountant")]
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public IActionResult ChartOfAccount()
    {
        List<AccountModel> accounts = new();
        string connStr = _configuration.GetConnectionString("DefaultConnection")!;

        using (var conn = new SqlConnection(connStr))
        {
            using (var cmd = new SqlCommand("SELECT A.AccountId, A.AccountName, A.ParentId, P.AccountName AS ParentName FROM ChartOfAccounts A LEFT JOIN ChartOfAccounts P ON A.ParentId = P.AccountId", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new AccountModel
                        {
                            AccountId = Convert.ToInt32(reader["AccountId"]),
                            AccountName = reader["AccountName"].ToString()!,
                            ParentId = reader["ParentId"] as int?,
                            ParentName = reader["ParentName"]?.ToString()
                        });
                    }
                }
            }
        }
        return View(accounts);
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult CreateAccount(AccountModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                    cmd.Parameters.AddWithValue("@ParentId", (object?)model.ParentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Operation", "INSERT");

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    TempData["Success"] = "Account created successfully.";
                    return RedirectToAction("ChartOfAccount");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
        }

        return View(model);
    }
    
    public IActionResult VoucherList()
    {
        return View();
    }
}