using AuthApi;
using Microsoft.EntityFrameworkCore;

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
        policy.WithOrigins("https://localhost:7251") // ������� ���� Blazor Server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ��� ������ � Cookie
    });
});


builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware ��� ��������� ��������
app.UseRouting();


//��� �������� API - ����� �� ������ http://localhost:5000/swagger/index.html
app.UseSwagger();
app.UseSwaggerUI();


// ��������� CORS
app.UseCors("AllowBlazorApp");
app.UseStaticFiles();

// �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// �������� ��� ������������
app.MapControllers();


app.Run();
