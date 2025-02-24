using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowBlazorApp")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        public UsersController(ApplicationDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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
                .Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    ProfilePicture = u.ProfilePicture,   // Возвращаем аватарку
                    Description = u.Description,           // Возвращаем описание
                    FollowersCount = _context.Followers.Count(f => f.FollowedUserId == u.UserId)
                })
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user == null
                ? NotFound("User not found")
                : Ok(user);
        }

        // Изменение фото профиля
        [Authorize]
        [HttpPost("change-avatar")]
        public async Task<IActionResult> ChangeProfileImage([FromForm] ChangeAvatarDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user identifier");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // Загружаем изображение в Cloudinary
                ImageUploadResult uploadResult = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }

                user.ProfilePicture = uploadResult.SecureUrl.ToString(); // Обновляем URL фото
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Фото профиля обновлено", AvatarUrl = user.ProfilePicture });
        }

        // Изменить описание профиля
        [Authorize]
        [HttpPost("change-description")]
        public async Task<IActionResult> ChangeDescription([FromBody] ChangeDescriptionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user identifier");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Description = dto.Description;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Описание обновлено", Description = user.Description });
        }

        [HttpPut("users/{userId}/update-description")]
        public async Task<IActionResult> UpdateDescription(int userId, [FromBody] UpdateDescriptionRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            user.Description = request.Description;
            await _context.SaveChangesAsync();

            return Ok("Описание обновлено.");
        }

        [HttpGet("single/{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    ProfilePicture = u.ProfilePicture,  // Добавьте, если необходимо
                    Description = u.Description,          // Добавьте, если необходимо
                    FollowersCount = _context.Followers.Count(f => f.FollowedUserId == u.UserId)
                })
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            return Ok(user);
        }

        public class UpdateDescriptionRequest
        {
            public string Description { get; set; }
        }

        // DTO для изменения фото профиля
        public class ChangeAvatarDto
        {
            public IFormFile ImageFile { get; set; }
        }

        // DTO для изменения описания профиля
        public class ChangeDescriptionDto
        {
            public string Description { get; set; }
        }
    }
    // DTO модель
    public class UserInfoDto
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string ProfilePicture { get; set; }  // Добавлено
            public string Description { get; set; }       // Добавлено
            public int FollowersCount { get; set; }       // Если это нужно
            public DateTime CreatedAt { get; set; }
        }


}

