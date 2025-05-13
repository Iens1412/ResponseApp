using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResponseApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResponseApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardModel> _logger;
        private readonly IInviteService _inviteService;

        public DashboardModel(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger<DashboardModel> logger,
            IInviteService inviteService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _inviteService = inviteService;
        }

        [BindProperty(SupportsGet = true)]
        public string GeneratedInviteLink { get; set; }

        public List<IdentityUser> Users { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData.ContainsKey("GeneratedInviteLink"))
            {
                GeneratedInviteLink = TempData["GeneratedInviteLink"]?.ToString();
                ViewData["GeneratedInviteLink"] = GeneratedInviteLink;
            }

            Users = _userManager.Users.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostGenerateInviteAsync()
        {
            var token = await _inviteService.GenerateTokenAsync();
            string inviteLink = Url.Page("/Account/Register", pageHandler: null, values: new { area = "Identity", token }, protocol: Request.Scheme);

            TempData["GeneratedInviteLink"] = inviteLink;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            string newPassword = "NewUser123!";
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPassword);

            TempData["StatusMessage"] = $"Password for {user.Email} reset to 'NewUser123!'";
            return RedirectToPage();
        }
    }
}