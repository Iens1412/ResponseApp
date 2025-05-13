namespace ResponseApp.Services
{
    public interface IInviteService
    {
        Task<bool> IsTokenValidAsync(string token);
        Task<string> GenerateTokenAsync(TimeSpan? lifetime = null);
        Task MarkTokenUsedAsync(string token);
    }
}