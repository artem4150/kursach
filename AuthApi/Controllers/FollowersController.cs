using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthApi;  // Пространство имен, где определены ваши модели

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FollowersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
        {
            try
            {
                // Проверяем, существует ли уже подписка
                var existingFollower = await _context.Followers
                    .FirstOrDefaultAsync(f => f.UserId == dto.FollowerId && f.FollowedUserId == dto.SubscribedToId);

                if (existingFollower != null)
                {
                    return BadRequest("Вы уже подписаны на данного пользователя.");
                }

                // Проверяем, существует ли пользователь, на которого подписываются
                var followedUserExists = await _context.Users.AnyAsync(u => u.UserId == dto.SubscribedToId);
                if (!followedUserExists)
                {
                    return NotFound($"Пользователь с id {dto.SubscribedToId} не найден.");
                }

                var follower = new Follower
                {
                    UserId = dto.FollowerId,           // Пользователь, который подписывается
                    FollowedUserId = dto.SubscribedToId, // Пользователь, на которого подписываются
                    CreatedAt = DateTime.UtcNow
                };

                _context.Followers.Add(follower);
                await _context.SaveChangesAsync();

                return Ok(follower);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в методе Subscribe: {ex}");
                return StatusCode(500, "Ошибка при подписке: " + ex.Message);
            }
        }


        // DELETE: api/Followers/unsubscribe/{followerId}/{followedUserId}
        [HttpDelete("unsubscribe/{followerId}/{followedUserId}")]
        public async Task<IActionResult> Unsubscribe(int followerId, int followedUserId)
        {
            var follower = await _context.Followers
                .FirstOrDefaultAsync(f => f.UserId == followerId && f.FollowedUserId == followedUserId);

            if (follower == null)
            {
                return NotFound("Подписка не найдена.");
            }

            _context.Followers.Remove(follower);
            await _context.SaveChangesAsync();

            return Ok("Вы успешно отписались.");
        }

        // GET: api/Followers/feed/{followerId}
        [HttpGet("feed/{followerId}")]
        public async Task<IActionResult> GetSubscriptionsFeed(int followerId)
        {
            // Получаем список идентификаторов пользователей, на которых подписан текущий пользователь
            var followedUserIds = await _context.Followers
                .Where(f => f.UserId == followerId)
                .Select(f => f.FollowedUserId)
                .ToListAsync();

            if (!followedUserIds.Any())
            {
                // Возвращаем пустой список DTO вместо сущностей
                return Ok(new List<PostDto>());
            }

            // Получаем публикации (Outfit) от подписанных пользователей, сортируя по дате создания
            var outfits = await _context.Outfits
                .Where(o => followedUserIds.Contains(o.UserId))
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.User)  // Подгружаем данные автора
                .Include(o => o.Likes)
                .Include(o => o.Comments)
                    .ThenInclude(c => c.User)
                .ToListAsync();

            // Проецируем сущности в DTO для избежания циклических ссылок
            var posts = outfits.Select(o => new PostDto
            {
                OutfitId = o.OutfitId,
                Title = o.Title,
                Description = o.Description,
                ImageUrl = o.ImageUrl,
                AuthorUsername = o.User?.Username,
                AuthorId = o.UserId, // Заполняем поле AuthorId
                CreatedAt = o.CreatedAt,
                LikesCount = o.Likes?.Count() ?? 0,
                Comments = o.Comments?.Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    Text = c.Text,
                    AuthorUsername = c.User?.Username,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<CommentDto>()
            }).ToList();

            return Ok(posts);
        }

        // GET: api/Followers/isSubscribed/{subscriberId}/{followedUserId}
        [HttpGet("isSubscribed/{subscriberId}/{followedUserId}")]
        public async Task<IActionResult> IsSubscribed(int subscriberId, int followedUserId)
        {
            bool exists = await _context.Followers
                .AnyAsync(f => f.UserId == subscriberId && f.FollowedUserId == followedUserId);
            return Ok(exists);
        }
        // GET: api/Followers/count/{userId}
        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetFollowersCount(int userId)
        {
            int count = await _context.Followers.CountAsync(f => f.FollowedUserId == userId);
            return Ok(count);
        }
        // DTO для подписки
        public class SubscribeDto
        {
            // Идентификатор пользователя, который подписывается (Follower)
            public int FollowerId { get; set; }
            // Идентификатор пользователя, на которого подписываются (FollowedUser)
            public int SubscribedToId { get; set; }
        }


        // DTO для подписки, который можно определить в этом контроллере или использовать существующий DTO
        public class PostDto
        {
            public int OutfitId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string AuthorUsername { get; set; }
            public int AuthorId { get; set; } // Это поле важно для подписки
            public DateTime CreatedAt { get; set; }
            public int LikesCount { get; set; }
            public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        }

        public class CommentDto
        {
            public int CommentId { get; set; }
            public string Text { get; set; }
            public string AuthorUsername { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
