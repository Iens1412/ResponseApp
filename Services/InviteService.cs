using Microsoft.EntityFrameworkCore;
using ResponseApp.Data;
using ResponseApp.Models;

namespace ResponseApp.Services
{
    public class InviteService : IInviteService
    {
        private readonly ApplicationDbContext _context;

        public InviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var invite = await _context.InviteLinks
                .FirstOrDefaultAsync(i => i.Token == token && !i.IsUsed && i.ExpiryDate > DateTime.UtcNow);
            return invite != null;
        }

        public async Task<string> GenerateTokenAsync(TimeSpan? lifetime = null)
        {
            string token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(60));

            _context.InviteLinks.Add(new InviteLink
            {
                Token = token,
                ExpiryDate = expiry,
                IsUsed = false
            });

            await _context.SaveChangesAsync();
            return token;
        }

        public async Task MarkTokenUsedAsync(string token)
        {
            var invite = await _context.InviteLinks.FirstOrDefaultAsync(i => i.Token == token);
            if (invite != null)
            {
                invite.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}