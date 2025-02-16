using System.Net.Http.Headers;
using kursach.Services;
using kursach.Services.AuthAPIService;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
var builder = WebApplication.CreateBuilder(args);

// Настройка CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowBlazorApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:5001")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});


builder.Services.AddScoped<IAuthAPIService, AuthApiServiceService>();
builder.Services.AddScoped<AuthService>();
// Настройка HTTP клиента для взаимодействия с API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/"); //лучше вынеси URI в переменную окружения, или в appsettings. Поменяется адрес-порт, по всему проекту искать и менять их не приветствуется
    
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();




var app = builder.Build();
app.UseStaticFiles();
// Настройка HTTPS
//app.UseHttpsRedirection();

// Настройка CORS
//app.UseCors("AllowBlazorApp");

// Аутентификация и авторизация
//если у тебя клиент серверное приложение, на клиенте это лишнее, пока комментирую
//app.UseAuthentication();
//app.UseAuthorization();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
