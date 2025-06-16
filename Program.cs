using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Add custom services
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Custom route for short URLs - должен быть ПЕРЕД стандартными маршрутами
app.MapGet("/{shortCode}", async (string shortCode, ApplicationDbContext context) =>
{
    var urlMapping = await context.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    if (urlMapping != null)
    {
        urlMapping.ClickCount++;
        await context.SaveChangesAsync();
        return Results.Redirect(urlMapping.OriginalUrl);
    }
    return Results.NotFound();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Удаляем базу данных если она существует (только для разработки)
        if (app.Environment.IsDevelopment())
        {
            context.Database.EnsureDeleted();
        }
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Логируем ошибку
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при создании базы данных");
        throw;
    }
}

app.Run();
