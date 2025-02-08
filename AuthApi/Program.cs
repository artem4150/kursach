using AuthApi;
using Microsoft.EntityFrameworkCore;

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
        policy.WithOrigins("https://localhost:7251") // Укажите порт Blazor Server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Для работы с Cookie
    });
});


builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware для обработки запросов
app.UseRouting();


//веб страница API - будет по адресу http://localhost:5000/swagger/index.html
app.UseSwagger();
app.UseSwaggerUI();


// Настройка CORS
app.UseCors("AllowBlazorApp");
app.UseStaticFiles();

// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Маршруты для контроллеров
app.MapControllers();


app.Run();
