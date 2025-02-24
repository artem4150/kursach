using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AuthApi;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")] //очень длинный путь API получается. обычно делают /api/[controller]

    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class Search : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Search(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Поисковый запрос не может быть пустым.");
            }

            var users = await _context.Users
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query)) // поиск по имени пользователя и email
                .Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    ProfilePicture = u.ProfilePicture,
                    Description = u.Description,
                    FollowersCount = _context.Followers.Count(f => f.FollowedUserId == u.UserId)
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetUsers([FromQuery] string sortBy)
        {
            IQueryable<User> query = _context.Users;

            // Сортировка по количеству подписчиков или дате создания
            if (sortBy == "followers")
            {
                query = query.OrderByDescending(u => _context.Followers.Count(f => f.FollowedUserId == u.UserId));
            }
            else if (sortBy == "createdAt")
            {
                query = query.OrderBy(u => u.CreatedAt);
            }

            var users = await query
                .Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    ProfilePicture = u.ProfilePicture,
                    Description = u.Description,
                    FollowersCount = _context.Followers.Count(f => f.FollowedUserId == u.UserId)
                })
                .ToListAsync();

            return Ok(users);
        }




    }
}
