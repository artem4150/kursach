using AuthApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ��������� ����������� � ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���������� ������������
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7251") // ������� ���� Blazor Server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ��� ������ � Cookie
    });
});

// ��������� Cookie-��������������
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "AppCookie";
        options.Cookie.SameSite = SameSiteMode.None; // ��������� ������������ Cookie ����� �������
        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/api/auth/login"; // ���� ��� �����������
        options.LogoutPath = "/api/auth/logout"; // ���� ��� ������
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ��������� HTTPS
app.UseHttpsRedirection();

// Middleware ��� ��������� ��������
app.UseRouting();

// ��������� CORS
app.UseCors("AllowBlazorApp");
app.UseStaticFiles();
// ��������� HTTPS
app.UseHttpsRedirection();
// �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// �������� ��� ������������
app.MapControllers();
app.MapRazorPages();

app.Run();
