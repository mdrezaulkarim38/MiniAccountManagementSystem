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

    [HttpGet]
    public IActionResult EditAccount(int id)
    {
        AccountModel account = new();
        using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        using (var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE AccountId = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    account.AccountId = (int)reader["AccountId"];
                    account.AccountName = reader["AccountName"].ToString()!;
                    account.ParentId = reader["ParentId"] != DBNull.Value ? (int?)reader["ParentId"] : null;
                }
            }
        }

        return View(account);
    }

    [HttpPost]
    public IActionResult EditAccount(AccountModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
                    cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                    cmd.Parameters.AddWithValue("@ParentId", (object?)model.ParentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Operation", "UPDATE");

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    TempData["Success"] = "Account updated successfully.";
                    return RedirectToAction("ChartOfAccount");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
            }
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult DeleteAccount(int id)
    {
        try
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccountId", id);
                cmd.Parameters.AddWithValue("@AccountName", DBNull.Value);
                cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
                cmd.Parameters.AddWithValue("@Operation", "DELETE");

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Account deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Delete failed: " + ex.Message;
        }

        return RedirectToAction("ChartOfAccount");
    }

    public IActionResult VoucherList()
    {
        return View();
    }
}