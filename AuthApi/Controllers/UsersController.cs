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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identifier");
            }

            var user = await _context.Users
                .AsNoTracking()
                .Select(u => new UserInfoDto // Используем DTO для безопасности
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user == null 
                ? NotFound("User not found") 
                : Ok(user);
        }

// DTO модель
        public class UserInfoDto
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }


    }
}
