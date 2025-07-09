using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Services;

public class ChartOfAccountService : IChartOfAccountService
{
    private readonly IConfiguration _configuration;

    public ChartOfAccountService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<AccountModel> GetAllAccounts()
    {
        List<AccountModel> accounts = new();
        string connStr = _configuration.GetConnectionString("DefaultConnection")!;

        using var conn = new SqlConnection(connStr);
        using var cmd = new SqlCommand(@"
                SELECT A.AccountId, A.AccountName, A.ParentId, P.AccountName AS ParentName 
                FROM ChartOfAccounts A 
                LEFT JOIN ChartOfAccounts P ON A.ParentId = P.AccountId", conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();
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

        return accounts;
    }

    public AccountModel? GetAccountById(int id)
    {
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE AccountId = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new AccountModel
            {
                AccountId = (int)reader["AccountId"],
                AccountName = reader["AccountName"].ToString()!,
                ParentId = reader["ParentId"] != DBNull.Value ? (int?)reader["ParentId"] : null
            };
        }

        return null;
    }

    public bool CreateAccount(AccountModel model, out string error)
    {
        error = string.Empty;
        try
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AccountId", DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
            cmd.Parameters.AddWithValue("@ParentId", (object?)model.ParentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Operation", "INSERT");

            conn.Open();
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool UpdateAccount(AccountModel model, out string error)
    {
        error = string.Empty;
        try
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
            cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
            cmd.Parameters.AddWithValue("@ParentId", (object?)model.ParentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Operation", "UPDATE");

            conn.Open();
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool DeleteAccount(int id, out string error)
    {
        error = string.Empty;
        try
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AccountId", id);
            cmd.Parameters.AddWithValue("@AccountName", DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
            cmd.Parameters.AddWithValue("@Operation", "DELETE");

            conn.Open();
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}