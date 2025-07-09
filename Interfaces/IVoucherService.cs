using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Interfaces;

public interface IVoucherService
{
    List<VoucherListModel> GetVoucherList();
    bool CreateVoucher(VoucherFormModel model, out string error);
    VoucherFormModel? GetVoucherById(int id);
    bool UpdateVoucher(int id, VoucherFormModel model, out string error);
    bool DeleteVoucher(int id, out string error);
    List<SelectListItem> GetVoucherTypes();
    List<SelectListItem> GetChartOfAccounts();
}