using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResponseApp.Data;
using ResponseApp.Models;
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
        private readonly ILogger<DashboardModel> _logger;
        private readonly IInviteService _inviteService;
        private readonly ApplicationDbContext _appDbContext;

        public DashboardModel(
            UserManager<IdentityUser> userManager,
            ILogger<DashboardModel> logger,
            IInviteService inviteService,
            ApplicationDbContext appDbContext)
        {
            _userManager = userManager;
            _logger = logger;
            _inviteService = inviteService;
            _appDbContext = appDbContext;
        }

        [BindProperty(SupportsGet = true)]
        public string GeneratedInviteLink { get; set; }

        [BindProperty]
        public string CurrentDatabasePath { get; set; }

        public List<IdentityUser> Users { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData.ContainsKey("GeneratedInviteLink"))
            {
                GeneratedInviteLink = TempData["GeneratedInviteLink"]?.ToString();
                ViewData["GeneratedInviteLink"] = GeneratedInviteLink;
            }

            LoadCurrentDatabasePath();
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

        private void LoadCurrentDatabasePath()
        {
            CurrentDatabasePath = _appDbContext.SystemSettings.FirstOrDefault(s => s.Key == "ExternalDbPath")?.Value ?? string.Empty;
        }

        public async Task<IActionResult> OnPostSaveDbPathAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentDatabasePath) || !System.IO.File.Exists(CurrentDatabasePath))
            {
                TempData["StatusMessage"] = "Invalid or missing file path.";
                return RedirectToPage();
            }

            Console.WriteLine("Selected DB Path: " + CurrentDatabasePath);

            var setting = _appDbContext.SystemSettings.FirstOrDefault(s => s.Key == "ExternalDbPath");
            if (setting == null)
            {
                _appDbContext.SystemSettings.Add(new SystemSetting
                {
                    Key = "ExternalDbPath",
                    Value = CurrentDatabasePath
                });
                Console.WriteLine("New DB path setting created.");
            }
            else
            {
                setting.Value = CurrentDatabasePath;
                Console.WriteLine("Existing DB path updated.");
            }

            await _appDbContext.SaveChangesAsync();
            TempData["StatusMessage"] = "Database path saved successfully.";
            return RedirectToPage();
        }
    }
}