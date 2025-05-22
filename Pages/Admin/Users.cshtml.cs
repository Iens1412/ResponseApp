using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ResponseApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserInfo> Users { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public class UserInfo
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
        }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            Users = new List<UserInfo>();

            foreach (var user in users)
            {
                var roles = (await _userManager.GetRolesAsync(user)).ToList();

                if (!roles.Contains("Admin"))
                {
                    Users.Add(new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = roles
                    });
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["StatusMessage"] = $"User {user.Email} deleted.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                TempData["StatusMessage"] = "Password cannot be empty.";
                return RedirectToPage();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"Password for {user.Email} reset.";
            }
            else
            {
                TempData["StatusMessage"] = $"Error: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToPage();
        }
    }
}