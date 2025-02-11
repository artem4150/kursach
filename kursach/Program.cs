using System.Net.Http.Headers;
using kursach.Services.AuthAPIService;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7251")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ����������� ��� ������ � ������
    });
});

builder.Services.AddScoped<IAuthAPIService, AuthApiServiceService>();

// ��������� HTTP ������� ��� �������������� � API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001"); //����� ������ URI � ���������� ���������, ��� � appsettings. ���������� �����-����, �� ����� ������� ������ � ������ �� �� ��������������
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
        options.Cookie.SameSite = SameSiteMode.None; // ����� ��� ������ ������
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // ��������� ���� ��� ����� � ������ �� API
        options.LoginPath = new PathString("/api/auth/login");
        options.LogoutPath = new PathString("/api/auth/logout");

        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.UseStaticFiles();
// ��������� HTTPS
//app.UseHttpsRedirection();

// ��������� CORS
app.UseCors("AllowBlazorApp");

// �������������� � �����������
//���� � ���� ������ ��������� ����������, �� ������� ��� ������, ���� �����������
//app.UseAuthentication();
//app.UseAuthorization();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
