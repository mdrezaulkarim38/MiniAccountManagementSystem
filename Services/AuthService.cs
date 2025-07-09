using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Interfaces;
using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            using (var cmd = new SqlCommand("sp_AssignUserToRole", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", user.Id);
                cmd.Parameters.AddWithValue("@RoleName", "Viewer");
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return (true, "Registration successful!");
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, false);
        return result.Succeeded
            ? (true, "Login Successful")
            : (false, "Invalid login attempt.");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}