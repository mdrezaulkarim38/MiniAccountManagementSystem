namespace MiniAccountManagementSystem.Models;

public class VoucherListModel
{
    public int VoucherId { get; set; }
    public DateTime VoucherDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? VoucherType { get; set; }
    public decimal? VoucherAmount { get; set;}
}