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

    
    
    public IActionResult VoucherList()
    {
        return View();
    }
}