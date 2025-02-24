using System.Text;
using System.Text.Json;
using AuthApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Настройка подключения к базе данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление контроллеров
builder.Services.AddControllers();

// Добавил Swagger для тестирование API
builder.Services.AddSwaggerGen();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("http://localhost:8081") // Разрешить доступ только с этого адреса
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddScoped<CloudinaryService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var cloudName = config["Cloudinary:CloudName"];
    var apiKey = config["Cloudinary:ApiKey"];
    var apiSecret = config["Cloudinary:ApiSecret"];

    return new CloudinaryService(cloudName, apiKey, apiSecret);
});
// Настройка аутентификации
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        //ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "http://localhost:5001",
        ValidAudience = "http://localhost:8081",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a5f43d82e5c34f8590d5a5b29d5b6a1d7273456d9c12f684f58709b0c9e3da60"))
    };
    // Логирование ошибок аутентификации
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully.");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Middleware для обработки запросов
app.UseRouting();

// Настройка CORS
app.UseCors("AllowBlazorApp");
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(authHeader))
    {
        // Разбиваем на схему и значение
        var parts = authHeader.Split(' ');
        if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            // Перезаписываем заголовок с обрезанным значением
            context.Request.Headers["Authorization"] = $"Bearer {parts[1].Trim()}";
        }
        Console.WriteLine("Проверенный заголовок Authorization: " + context.Request.Headers["Authorization"]);
    }
    else
    {
        Console.WriteLine("Заголовок Authorization отсутствует.");
    }
    await next();
});
;
// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Swagger (для тестирования API)
app.UseSwagger();
app.UseSwaggerUI();

// Статические файлы (если нужно)
app.UseStaticFiles();

// Маршруты для контроллеров
app.MapControllers();

app.Run();