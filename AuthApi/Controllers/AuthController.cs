﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AuthApi;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace AuthApi.Controllers
{
    [Route("api/auth/[controller]")] //очень длинный путь API получается. обычно делают /api/[controller]

    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Валидация входных данных
            if (string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password) || 
                string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { Message = "Все поля обязательны." });
            }

            // Проверка существующего пользователя
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new { Message = "Пользователь с таким email уже существует." });
            }

            try
            {
                // Хеширование пароля
                var passwordHash = HashPassword(request.Password);

                // Создание нового пользователя
                var newUser = new User
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Username = request.Username,
                    CreatedAt = DateTime.UtcNow // Пример дополнительного поля
                };

                // Добавление в базу
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Опционально: автоматическая авторизация после регистрации
                // var claims = ... (аналогично методу Login)
                // await HttpContext.SignInAsync(...);

                return Ok(new { 
                    Message = "Регистрация успешна", 
                    UserId = newUser.UserId 
                });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера." });
            }
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return Ok(new { Message = "Выход выполнен успешно" });
        }

        

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Логирование входящих данных для отладки
            Console.WriteLine($"Получен запрос на вход: Email={request.Email}, PasswordHash={request.Password}");

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Email и PasswordHash обязательны." });
            }

            // Проверка пользователя
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || HashPassword(request.Password) != user.PasswordHash)
            {
                return Unauthorized(new { Message = "Неверный email или пароль." });
            }

            // Успешный вход — создаём Claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username),
    };

            // Создаём Principal
            var claimsIdentity = new ClaimsIdentity(claims, "Identity.Application");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Устанавливаем Cookie
            //Пока закоменентил. Не совсем понимаю зачем нужны куки на стороне сервера, с HttpContext не знаком
            /*await HttpContext.SignInAsync("Identity.Application", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true, // Cookie будет сохраняться между сеансами
                ExpiresUtc = DateTime.UtcNow.AddHours(1) // Время истечения Cookie
            });*/

            return Ok(new { Message = "Успешный вход", UserId = user.UserId });
        }
        
        // добавил для теста, получение всех пользователей
        [HttpGet("get_users")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }



        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public class LoginRequest
        {
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }
        
        public class RegisterRequest
        {
            [Required]
            public string Email { get; set; }
    
            [Required]
            public string Password { get; set; }
    
            [Required]
            public string Username { get; set; }
    
            // Добавьте другие поля, если необходимо (например, подтверждение пароля)
        }
        
    }
}
