using System.Net.Http.Headers;
using kursach.Services.AuthAPIService;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7251")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Обязательно для работы с куками
    });
});

builder.Services.AddScoped<IAuthAPIService, AuthApiServiceService>();

// Настройка HTTP клиента для взаимодействия с API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001"); //лучше вынеси URI в переменную окружения, или в appsettings. Поменяется адрес-порт, по всему проекту искать и менять их не приветствуется
    //client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "AppCookie";
        options.Cookie.SameSite = SameSiteMode.None; // Важно для разных портов
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Установка пути для входа и выхода на API
        options.LoginPath = new PathString("/api/auth/login");
        options.LogoutPath = new PathString("/api/auth/logout");

        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.UseStaticFiles();
// Настройка HTTPS
//app.UseHttpsRedirection();

// Настройка CORS
app.UseCors("AllowBlazorApp");

// Аутентификация и авторизация
//если у тебя клиент серверное приложение, на клиенте это лишнее, пока комментирую
//app.UseAuthentication();
//app.UseAuthorization();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
