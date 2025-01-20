using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthApi.Controllers
{
    [EnableCors("AllowBlazorApp")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Получаем ID пользователя из ClaimsPrincipal
            var userIdFromCookie = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdFromCookie))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            // Находим пользователя по ID
            var userId = int.Parse(userIdFromCookie);
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            return Ok(new { user.Username, user.Email });
        }


    }
}
