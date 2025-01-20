using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AuthApi;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace AuthApi.Controllers
{
    [Route("api/auth/[controller]")]

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
        public async Task<IActionResult> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest(new { Message = "Пользователь с таким email уже существует." });
            }

            // Двойное хеширование: сервер хеширует уже хешированный пароль
            user.PasswordHash = HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { UserId = user.UserId });
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
            Console.WriteLine($"Получен запрос на вход: Email={request.Email}, PasswordHash={request.PasswordHash}");

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.PasswordHash))
            {
                return BadRequest(new { Message = "Email и PasswordHash обязательны." });
            }

            // Проверка пользователя
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || HashPassword(request.PasswordHash) != user.PasswordHash)
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
            await HttpContext.SignInAsync("Identity.Application", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true, // Cookie будет сохраняться между сеансами
                ExpiresUtc = DateTime.UtcNow.AddHours(1) // Время истечения Cookie
            });

            return Ok(new { Message = "Успешный вход", UserId = user.UserId });
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
            public string PasswordHash { get; set; }
        }
    }
}
