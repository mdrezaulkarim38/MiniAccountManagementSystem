using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Models;

public class AccountModel
{
    [Key]
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
}
