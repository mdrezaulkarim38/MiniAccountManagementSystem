using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;
using System.Data;

namespace MiniAccountManagementSystem.Services;

public class VoucherService : IVoucherService
{
    private readonly IConfiguration _configuration;
    public VoucherService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<VoucherListModel> GetVoucherList()
    {
        var list = new List<VoucherListModel>();
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand(@"
                SELECT V.VoucherId, V.VoucherDate, V.ReferenceNo, T.TypeName AS VoucherType, 
                       SUM(T2.DebitAmount) AS VoucherAmount 
                FROM Vouchers V 
                INNER JOIN VoucherTypes T ON V.VoucherTypeId = T.Id 
                INNER JOIN VoucherEntries T2 ON T2.VoucherId = V.VoucherId 
                GROUP BY V.VoucherId, V.VoucherDate, V.ReferenceNo, T.TypeName 
                ORDER BY V.VoucherDate DESC", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new VoucherListModel
            {
                VoucherId = (int)reader["VoucherId"],
                VoucherDate = (DateTime)reader["VoucherDate"],
                ReferenceNo = reader["ReferenceNo"].ToString()!,
                VoucherType = reader["VoucherType"].ToString()!,
                VoucherAmount = (decimal)reader["VoucherAmount"]
            });
        }
        return list;
    }

    public bool CreateVoucher(VoucherFormModel model, out string error)
    {
        error = string.Empty;
        try
        {
            var dt = new DataTable();
            dt.Columns.Add("AccountId", typeof(int));
            dt.Columns.Add("DebitAmount", typeof(decimal));
            dt.Columns.Add("CreditAmount", typeof(decimal));
            foreach (var entry in model.Entries)
                dt.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_SaveVoucher", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
            cmd.Parameters.AddWithValue("@ReferenceNo", model.ReferenceNo);
            cmd.Parameters.AddWithValue("@VoucherTypeId", model.VoucherTypeId);

            var tvp = cmd.Parameters.AddWithValue("@VoucherEntries", dt);
            tvp.SqlDbType = SqlDbType.Structured;
            tvp.TypeName = "VoucherEntryType";

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

    public VoucherFormModel? GetVoucherById(int id)
    {
        var model = new VoucherFormModel { Entries = new List<VoucherEntryModel>() };
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        using (var cmd = new SqlCommand("SELECT * FROM Vouchers WHERE VoucherId = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.VoucherDate = (DateTime)reader["VoucherDate"];
                model.ReferenceNo = reader["ReferenceNo"].ToString()!;
                model.VoucherTypeId = (int)reader["VoucherTypeId"];
            }
            else return null;
            conn.Close();
        }

        using (var cmd = new SqlCommand(@"
                SELECT E.AccountId, A.AccountName, E.DebitAmount, E.CreditAmount 
                FROM VoucherEntries E
                INNER JOIN ChartOfAccounts A ON E.AccountId = A.AccountId
                WHERE E.VoucherId = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                model.Entries.Add(new VoucherEntryModel
                {
                    AccountId = (int)reader["AccountId"],
                    AccountName = reader["AccountName"].ToString(),
                    DebitAmount = (decimal)reader["DebitAmount"],
                    CreditAmount = (decimal)reader["CreditAmount"]
                });
            }
        }
        return model;
    }

    public bool UpdateVoucher(int id, VoucherFormModel model, out string error)
    {
        error = string.Empty;
        try
        {
            var dt = new DataTable();
            dt.Columns.Add("AccountId", typeof(int));
            dt.Columns.Add("DebitAmount", typeof(decimal));
            dt.Columns.Add("CreditAmount", typeof(decimal));
            foreach (var entry in model.Entries)
                dt.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using (var cmd = new SqlCommand(@"
                    UPDATE Vouchers 
                    SET VoucherDate = @VoucherDate, ReferenceNo = @ReferenceNo, VoucherTypeId = @VoucherTypeId 
                    WHERE VoucherId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@VoucherDate", model.VoucherDate);
                cmd.Parameters.AddWithValue("@ReferenceNo", model.ReferenceNo);
                cmd.Parameters.AddWithValue("@VoucherTypeId", model.VoucherTypeId);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand("DELETE FROM VoucherEntries WHERE VoucherId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            foreach (DataRow row in dt.Rows)
            {
                using var cmd = new SqlCommand(@"
                        INSERT INTO VoucherEntries (VoucherId, AccountId, DebitAmount, CreditAmount)
                        VALUES (@VoucherId, @AccountId, @Debit, @Credit)", conn);

                cmd.Parameters.AddWithValue("@VoucherId", id);
                cmd.Parameters.AddWithValue("@AccountId", row["AccountId"]);
                cmd.Parameters.AddWithValue("@Debit", row["DebitAmount"]);
                cmd.Parameters.AddWithValue("@Credit", row["CreditAmount"]);
                cmd.ExecuteNonQuery();
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool DeleteVoucher(int id, out string error)
    {
        error = string.Empty;
        try
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using var deleteEntries = new SqlCommand("DELETE FROM VoucherEntries WHERE VoucherId = @id", conn);
            deleteEntries.Parameters.AddWithValue("@id", id);
            deleteEntries.ExecuteNonQuery();

            using var deleteVoucher = new SqlCommand("DELETE FROM Vouchers WHERE VoucherId = @id", conn);
            deleteVoucher.Parameters.AddWithValue("@id", id);
            deleteVoucher.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public List<SelectListItem> GetVoucherTypes()
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

    public List<SelectListItem> GetChartOfAccounts()
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