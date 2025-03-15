using AuthApi;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutfitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        public OutfitsController(ApplicationDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: api/Outfits
        [HttpGet]
        public async Task<IActionResult> GetAllOutfits()
        {
            var outfits = await _context.Outfits
                .Include(o => o.User)
                .Include(o => o.Likes)
                .Include(o => o.Comments)
                    .ThenInclude(c => c.User)
                .Include(o => o.OutfitTags)
                    .ThenInclude(ot => ot.Tag)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var posts = outfits.Select(o => new PostDto
            {
                OutfitId = o.OutfitId,
                Title = o.Title,
                Description = o.Description,
                ImageUrl = o.ImageUrl,
                AuthorUsername = o.User?.Username,
                AuthorId = o.UserId,   // Добавлено: заполняем AuthorId
                CreatedAt = o.CreatedAt,
                AuthorAvatar = o.User?.ProfilePicture,
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

        // GET: api/Outfits/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOutfits(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"Пользователь с id {userId} не найден.");

            var outfits = await _context.Outfits
                .Include(o => o.User)
                .Include(o => o.Likes)
                .Include(o => o.OutfitTags)
                    .ThenInclude(ot => ot.Tag)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var posts = outfits.Select(o => new PostDto
            {
                OutfitId = o.OutfitId,
                Title = o.Title,
                Description = o.Description,
                ImageUrl = o.ImageUrl,
                AuthorUsername = o.User?.Username,
                AuthorId = o.UserId,   // Добавлено: заполняем AuthorId
                CreatedAt = o.CreatedAt,
                AuthorAvatar = o.User?.ProfilePicture,
                LikesCount = o.Likes?.Count() ?? 0
            }).ToList();

            return Ok(posts);
        }

 [Authorize]     
[HttpPost("create-with-images")]
public async Task<IActionResult> CreateOutfitWithImages([FromForm] CreateOutfitWithImagesDto dto)
{
    if (dto == null)
    {
        return BadRequest("Некорректные данные.");
    }

    var imageUrls = new List<string>();

    if (dto.Images != null && dto.Images.Count > 0)
    {
        foreach (var imageFile in dto.Images)
        {
            if (imageFile.Length > 0)
            {
                // Загрузка изображения (например, в Cloudinary)
                var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
                imageUrls.Add(uploadResult.SecureUrl.ToString());
            }
        }
    }

    // Извлекаем идентификатор текущего пользователя из Claims
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
    {
        return Unauthorized("Не удалось определить пользователя.");
    }

    // Создаем новую публикацию (Outfit)
    var outfit = new Outfit
    {
        Title = dto.Title,
        Description = dto.Description,
        Season = dto.Season,
        Style = dto.Style,
        Gender = dto.Gender,
        Occasion = dto.Occasion,
        CreatedAt = DateTime.UtcNow,
        UserId = currentUserId,
        ImageUrl = imageUrls.Any() ? imageUrls[0] : null  // Используем первую фотографию как главное изображение
    };

    _context.Outfits.Add(outfit);
    await _context.SaveChangesAsync();

    // Если загружено более одного изображения, объединяем оставшиеся через разделитель "|"
    if (imageUrls.Count > 1)
    {
        var additionalImages = string.Join("|", imageUrls.Skip(1));
        outfit.ImageUrl += "|" + additionalImages;
        _context.Outfits.Update(outfit);
        await _context.SaveChangesAsync();
    }

    return Ok(outfit);
}



        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeOutfit(int id)
        {
            // Получаем текущего пользователя из Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Проверяем, существует ли публикация
            var outfit = await _context.Outfits
                .Include(o => o.Likes)
                .Include(o => o.Comments)
                    .ThenInclude(c => c.User)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OutfitId == id);
            if (outfit == null)
            {
                return NotFound("Публикация не найдена");
            }

            // Проверяем, поставлен ли уже лайк от этого пользователя
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.OutfitId == id && l.UserId == userId);
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    OutfitId = id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(like);
            }
            await _context.SaveChangesAsync();

            // Снова загружаем публикацию, чтобы включить комментарии
            var updatedOutfit = await _context.Outfits
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Likes)
                .Include(o => o.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(o => o.OutfitId == id);

            if (updatedOutfit == null)
                return NotFound("Публикация не найдена после обновления");

            var updatedDto = new PostDto
            {
                OutfitId = updatedOutfit.OutfitId,
                Title = updatedOutfit.Title,
                Description = updatedOutfit.Description,
                ImageUrl = updatedOutfit.ImageUrl,
                AuthorUsername = updatedOutfit.User?.Username,
                AuthorId = updatedOutfit.UserId,  // Добавлено: заполняем AuthorId
                CreatedAt = updatedOutfit.CreatedAt,
                AuthorAvatar = updatedOutfit.User?.ProfilePicture,
                LikesCount = updatedOutfit.Likes?.Count() ?? 0,
                Comments = updatedOutfit.Comments?.Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    Text = c.Text,
                    AuthorUsername = c.User?.Username,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<CommentDto>()
            };

            return Ok(updatedDto);
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto commentDto)
        {
            if (commentDto == null || string.IsNullOrWhiteSpace(commentDto.Text))
            {
                return BadRequest("Комментарий не может быть пустым.");
            }

            // Получаем идентификатор текущего пользователя из Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Проверяем, существует ли публикация
            var outfit = await _context.Outfits.FirstOrDefaultAsync(o => o.OutfitId == id);
            if (outfit == null)
            {
                return NotFound("Публикация не найдена.");
            }

            // Создаем новый комментарий
            var comment = new Comment
            {
                OutfitId = id,
                UserId = userId,
                Text = commentDto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Получаем имя пользователя для комментария
            var user = await _context.Users.FindAsync(userId);
            var result = new CommentDto
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                AuthorUsername = user?.Username,
                CreatedAt = comment.CreatedAt
            };

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPost(int id, [FromBody] EditPostDto dto)
        {
            var post = await _context.Outfits.FirstOrDefaultAsync(o => o.OutfitId == id);
            if (post == null)
            {
                return NotFound("Публикация не найдена.");
            }

            post.Title = dto.Title;
            post.Description = dto.Description;

            // Если вы хотите обновить изображение:
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                post.ImageUrl = dto.ImageUrl; // предполагается, что ImageUrl это строка
            }

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Outfits.FirstOrDefaultAsync(o => o.OutfitId == id);
            if (post == null)
            {
                return NotFound("Публикация не найдена.");
            }

            _context.Outfits.Remove(post);
            await _context.SaveChangesAsync();
            return Ok("Публикация удалена.");
        }


        public class EditPostDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; } // если вы хотите обновить изображение
        }




        // DTO-модель для передачи данных о публикации
        public class PostDto
        {
            public int OutfitId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string AuthorUsername { get; set; }
            public string AuthorAvatar { get; set; }
            public int AuthorId { get; set; }  // Добавлено
            public DateTime CreatedAt { get; set; }
            public int LikesCount { get; set; }
            public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        }

        public class AddCommentDto
        {
            public string Text { get; set; }
        }

        public class CommentDto
        {
            public int CommentId { get; set; }
            public string Text { get; set; }
            public string AuthorUsername { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class CreateOutfitWithImagesDto
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
            public string Season { get; set; }
            public string Style { get; set; }
            public string Gender { get; set; }
            public string Occasion { get; set; }
            // Теги, введённые пользователем, разделённые запятыми
            
            public List<IFormFile> Images { get; set; }
        }
    }
}
