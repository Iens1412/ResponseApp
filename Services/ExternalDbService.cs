using Microsoft.EntityFrameworkCore;
using ResponseApp.Data;
using ResponseApp.Models;

namespace ResponseApp.Services
{
    public class ExternalDbService : IExternalDbService
    {
        private readonly ApplicationDbContext _identityDb;

        public ExternalDbService(ApplicationDbContext identityDb)
        {
            _identityDb = identityDb;
        }

        public ExternalDbContext GetDbContext()
        {
            var dbPath = _identityDb.SystemSettings
                .AsNoTracking()
                .FirstOrDefault(s => s.Key == "ExternalDbPath")?.Value;

            if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                return null;

            var options = new DbContextOptionsBuilder<ExternalDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            return new ExternalDbContext(options);
        }
    }
}