using ResponseApp.Data;

namespace ResponseApp.Services
{
    public interface IExternalDbService
    {
        ExternalDbContext GetDbContext();
    }
}