using System.ComponentModel.DataAnnotations;
namespace MiniAccountManagementSystem.Models;

public class AccountModel
{
    public int AccountId { get; set; }
    [Required(ErrorMessage = "Account Name is required.")]
    public string? AccountName { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
}
