using System.Net.Http.Headers;
using kursach.Services;
using kursach.Services.AuthAPIService;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
var builder = WebApplication.CreateBuilder(args);

// ��������� CORS
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
// ��������� HTTP ������� ��� �������������� � API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/"); //����� ������ URI � ���������� ���������, ��� � appsettings. ���������� �����-����, �� ����� ������� ������ � ������ �� �� ��������������
    
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();




var app = builder.Build();
app.UseStaticFiles();
// ��������� HTTPS
//app.UseHttpsRedirection();

// ��������� CORS
//app.UseCors("AllowBlazorApp");

// �������������� � �����������
//���� � ���� ������ ��������� ����������, �� ������� ��� ������, ���� �����������
//app.UseAuthentication();
//app.UseAuthorization();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
