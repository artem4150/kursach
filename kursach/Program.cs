using System.Net.Http.Headers;
using kursach.Services;
using kursach.Services.AuthAPIService;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Microsoft.JSInterop;



var builder = WebApplication.CreateBuilder(args);




builder.Services.AddScoped<IAuthAPIService, AuthApiServiceService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PublicationService>();
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



app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
