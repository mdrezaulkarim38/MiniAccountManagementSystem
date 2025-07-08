using System.ComponentModel.DataAnnotations;
namespace MiniAccountManagementSystem.Models;
public class VoucherEntryModel
{
    public int AccountId { get; set; }
    public string? AccountName { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
