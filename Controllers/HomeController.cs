using Microsoft.AspNetCore.Mvc;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public HomeController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
        }

        public async Task<IActionResult> Index()
        {
            var urlMappings = await _urlShortenerService.GetAllUrlMappingsAsync();
            return View(urlMappings);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrlMapping model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Проверяем корректность URL
                    if (!Uri.TryCreate(model.OriginalUrl, UriKind.Absolute, out _))
                    {
                        ModelState.AddModelError("OriginalUrl", "Введите корректный URL");
                        return View(model);
                    }

                    var shortCode = await _urlShortenerService.ShortenUrlAsync(model.OriginalUrl);
                    TempData["Success"] = $"URL успешно сокращен! Короткий код: {shortCode}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Произошла ошибка при создании короткой ссылки");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var urlMapping = await _urlShortenerService.GetUrlMappingByIdAsync(id);
            if (urlMapping == null)
            {
                return NotFound();
            }

            return View(urlMapping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UrlMapping model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Проверяем корректность URL
                    if (!Uri.TryCreate(model.OriginalUrl, UriKind.Absolute, out _))
                    {
                        ModelState.AddModelError("OriginalUrl", "Введите корректный URL");
                        return View(model);
                    }

                    var success = await _urlShortenerService.UpdateUrlMappingAsync(model);
                    if (success)
                    {
                        TempData["Success"] = "URL успешно обновлен!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Произошла ошибка при обновлении");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Произошла ошибка при обновлении URL");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _urlShortenerService.DeleteUrlMappingAsync(id);
                if (success)
                {
                    TempData["Success"] = "URL успешно удален!";
                }
                else
                {
                    TempData["Error"] = "Не удалось удалить URL";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Произошла ошибка при удалении URL";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
