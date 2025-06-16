using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly ApplicationDbContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public UrlShortenerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ShortenUrlAsync(string originalUrl)
        {
            // Проверяем, существует ли уже такой URL
            var existingMapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);

            if (existingMapping != null)
            {
                return existingMapping.ShortCode;
            }

            // Генерируем уникальный короткий код
            string shortCode;
            do
            {
                shortCode = GenerateShortCode();
            } while (await _context.UrlMappings.AnyAsync(u => u.ShortCode == shortCode));

            // Создаем новую запись
            var urlMapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.Now, // Изменено с DateTime.UtcNow на DateTime.Now
                ClickCount = 0
            };

            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync();

            return shortCode;
        }

        public async Task<UrlMapping?> GetUrlMappingAsync(string shortCode)
        {
            return await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<List<UrlMapping>> GetAllUrlMappingsAsync()
        {
            return await _context.UrlMappings
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<UrlMapping?> GetUrlMappingByIdAsync(int id)
        {
            return await _context.UrlMappings.FindAsync(id);
        }

        public async Task<bool> UpdateUrlMappingAsync(UrlMapping urlMapping)
        {
            try
            {
                _context.UrlMappings.Update(urlMapping);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUrlMappingAsync(int id)
        {
            try
            {
                var urlMapping = await _context.UrlMappings.FindAsync(id);
                if (urlMapping != null)
                {
                    _context.UrlMappings.Remove(urlMapping);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateShortCode()
        {
            const int length = 7;
            var result = new StringBuilder(length);
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                
                for (int i = 0; i < length; i++)
                {
                    result.Append(Characters[bytes[i] % Characters.Length]);
                }
            }
            
            return result.ToString();
        }
    }
}
