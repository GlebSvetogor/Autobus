using UrlShortener.Models;

namespace UrlShortener.Services
{
    public interface IUrlShortenerService
    {
        Task<string> ShortenUrlAsync(string originalUrl);
        Task<UrlMapping?> GetUrlMappingAsync(string shortCode);
        Task<List<UrlMapping>> GetAllUrlMappingsAsync();
        Task<UrlMapping?> GetUrlMappingByIdAsync(int id);
        Task<bool> UpdateUrlMappingAsync(UrlMapping urlMapping);
        Task<bool> DeleteUrlMappingAsync(int id);
    }
}
