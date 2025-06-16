using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Оригинальный URL обязателен")]
        [Url(ErrorMessage = "Введите корректный URL")]
        [Display(Name = "Оригинальный URL")]
        public string OriginalUrl { get; set; } = string.Empty;
        
        [Display(Name = "Короткий код")]
        public string ShortCode { get; set; } = string.Empty;
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Количество переходов")]
        public int ClickCount { get; set; }
    }
}
