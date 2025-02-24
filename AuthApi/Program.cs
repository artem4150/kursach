using System.Text;
using System.Text.Json;
using AuthApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ��������� ����������� � ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���������� ������������
builder.Services.AddControllers();

// ������� Swagger ��� ������������ API
builder.Services.AddSwaggerGen();

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("http://localhost:8081") // ��������� ������ ������ � ����� ������
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
// ��������� ��������������
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
    // ����������� ������ ��������������
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

// Middleware ��� ��������� ��������
app.UseRouting();

// ��������� CORS
app.UseCors("AllowBlazorApp");
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(authHeader))
    {
        // ��������� �� ����� � ��������
        var parts = authHeader.Split(' ');
        if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            // �������������� ��������� � ���������� ���������
            context.Request.Headers["Authorization"] = $"Bearer {parts[1].Trim()}";
        }
        Console.WriteLine("����������� ��������� Authorization: " + context.Request.Headers["Authorization"]);
    }
    else
    {
        Console.WriteLine("��������� Authorization �����������.");
    }
    await next();
});
;
// �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// Swagger (��� ������������ API)
app.UseSwagger();
app.UseSwaggerUI();

// ����������� ����� (���� �����)
app.UseStaticFiles();

// �������� ��� ������������
app.MapControllers();

app.Run();