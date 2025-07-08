using System.ComponentModel.DataAnnotations;
namespace MiniAccountManagementSystem.Models;
public class VoucherFormModel
{
    [Required]
    public DateTime VoucherDate { get; set; }

    [Required]
    public string? ReferenceNo { get; set; }

    [Required]
    public int VoucherTypeId { get; set; }

    public List<VoucherEntryModel> Entries { get; set; } = new();
}