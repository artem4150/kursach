using AuthApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Настройка подключения к базе данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление контроллеров
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7251") // Укажите порт Blazor Server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Для работы с Cookie
    });
});

// Настройка Cookie-аутентификации
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "AppCookie";
        options.Cookie.SameSite = SameSiteMode.None; // Позволяет использовать Cookie между портами
        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/api/auth/login"; // Путь для авторизации
        options.LogoutPath = "/api/auth/logout"; // Путь для выхода
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Настройка HTTPS
app.UseHttpsRedirection();

// Middleware для обработки запросов
app.UseRouting();

// Настройка CORS
app.UseCors("AllowBlazorApp");
app.UseStaticFiles();
// Настройка HTTPS
app.UseHttpsRedirection();
// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Маршруты для контроллеров
app.MapControllers();
app.MapRazorPages();

app.Run();
