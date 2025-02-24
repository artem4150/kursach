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
// Ќастройка HTTP клиента дл€ взаимодействи€ с API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/"); //лучше вынеси URI в переменную окружени€, или в appsettings. ѕомен€етс€ адрес-порт, по всему проекту искать и мен€ть их не приветствуетс€
    
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
