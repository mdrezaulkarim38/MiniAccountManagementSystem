using MiniAccountManagementSystem.Models;
namespace MiniAccountManagementSystem.Interfaces;

public interface IChartOfAccountService
{
    List<AccountModel> GetAllAccounts();
    AccountModel? GetAccountById(int id);
    bool CreateAccount(AccountModel model, out string error);
    bool UpdateAccount(AccountModel model, out string error);
    bool DeleteAccount(int id, out string error);
}