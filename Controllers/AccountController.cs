using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniAccountManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

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

    [HttpGet]
    [Authorize(Roles = "Admin,Accountant,Viewer")]
    public IActionResult VoucherList()
    {
        var list = new List<VoucherListModel>();

        using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        using (var cmd = new SqlCommand(@"
        SELECT V.VoucherId, V.VoucherDate, V.ReferenceNo, T.TypeName AS VoucherType
        FROM Vouchers V
        INNER JOIN VoucherTypes T ON V.VoucherTypeId = T.Id
        ORDER BY V.VoucherDate DESC", conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new VoucherListModel
                    {
                        VoucherId = (int)reader["VoucherId"],
                        VoucherDate = (DateTime)reader["VoucherDate"],
                        ReferenceNo = reader["ReferenceNo"].ToString()!,
                        VoucherType = reader["VoucherType"].ToString()!
                    });
                }
            }
        }

        ViewBag.Role = User.IsInRole("Admin") ? "Admin" :
                       User.IsInRole("Accountant") ? "Accountant" :
                       "Viewer";

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateVoucher()
    {
        var model = new VoucherFormModel
        {
            VoucherDate = DateTime.Today,
            Entries = new List<VoucherEntryModel> { new(), new() } // two empty lines
        };

        ViewBag.VoucherTypes = GetVoucherTypes();
        ViewBag.Accounts = GetChartOfAccounts();

        return View(model);
    }

    [HttpPost]
    public IActionResult CreateVoucher(VoucherFormModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("AccountId", typeof(int));
                dataTable.Columns.Add("DebitAmount", typeof(decimal));
                dataTable.Columns.Add("CreditAmount", typeof(decimal));

                foreach (var entry in model.Entries)
                {
                    dataTable.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);
                }

                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                using (var cmd = new SqlCommand("sp_SaveVoucher", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
                    cmd.Parameters.AddWithValue("@ReferenceNo", model.ReferenceNo);
                    cmd.Parameters.AddWithValue("@VoucherTypeId", model.VoucherTypeId);

                    var tvpParam = cmd.Parameters.AddWithValue("@VoucherEntries", dataTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "VoucherEntryType";

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Voucher created successfully.";
                return RedirectToAction("VoucherList");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
        }

        ViewBag.VoucherTypes = GetVoucherTypes();
        ViewBag.Accounts = GetChartOfAccounts();

        return View(model);
    }

    private List<SelectListItem> GetVoucherTypes()
    {
        var list = new List<SelectListItem>();
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("SELECT Id, TypeName FROM VoucherTypes", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new SelectListItem
            {
                Value = reader["Id"].ToString(),
                Text = reader["TypeName"].ToString()
            });
        }
        return list;
    }

    private List<SelectListItem> GetChartOfAccounts()
    {
        var list = new List<SelectListItem>();
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("SELECT AccountId, AccountName FROM ChartOfAccounts", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new SelectListItem
            {
                Value = reader["AccountId"].ToString(),
                Text = reader["AccountName"].ToString()
            });
        }
        return list;
    }
}