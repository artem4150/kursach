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
    [Route("api/[controller]")]
    [ApiController]
    public class OutfitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OutfitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Outfits
        [HttpGet]
        public async Task<IActionResult> GetAllOutfits()
        {
            // Выбираем все аутфиты, включая пользователя (автора) и, при необходимости, связанные данные
            var outfits = await _context.Outfits
                .Include(o => o.User)
                .Include(o => o.Likes)       // если нужно количество лайков
                .Include(o => o.OutfitTags)  // если нужно теги
                    .ThenInclude(ot => ot.Tag)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Можно вернуть DTO, чтобы не выдавать лишнюю информацию.
            // Для простоты возвращаем прямо список
            return Ok(outfits);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOutfit([FromBody] CreateOutfitDto dto)
        {
            // Проверяем авторизацию (JWT/Identity). Предположим, получаем UserId из токена:
            // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Для упрощения, допустим, userId = 1
            int userId = 1;

            var newOutfit = new Outfit
            {
                Title = dto.Title,
                Description = dto.Description,
                Season = dto.Season,
                Style = dto.Style,
                Gender = dto.Gender,
                Occasion = dto.Occasion,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Outfits.Add(newOutfit);
            await _context.SaveChangesAsync();

            // Если нужны теги, их можно добавить после создания Outfit:
            // 1) найти/создать Tag'и
            // 2) связать их с Outfit через OutfitTags
            // Пример (опционально):
            /*
            if (dto.Tags != null)
            {
                foreach (var tagName in dto.Tags)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { TagName = tagName };
                        _context.Tags.Add(existingTag);
                        await _context.SaveChangesAsync();
                    }

                    var outfitTag = new OutfitTag
                    {
                        OutfitId = newOutfit.OutfitId,
                        TagId = existingTag.TagId
                    };
                    _context.OutfitTags.Add(outfitTag);
                }
                await _context.SaveChangesAsync();
            }
            */

            return Ok(newOutfit);
        }

        // GET: api/Outfits/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOutfits(int userId)
        {
            // Проверяем, есть ли такой пользователь
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with id {userId} not found.");
            }

            // Получаем Outfits пользователя
            var outfits = await _context.Outfits
                .Include(o => o.Likes)
                .Include(o => o.OutfitTags).ThenInclude(ot => ot.Tag)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(outfits);
        }

        // DTO-класс для создания Outfit
        public class CreateOutfitDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Season { get; set; }
            public string Style { get; set; }
            public string Gender { get; set; }
            public string Occasion { get; set; }
            public List<string>? Tags { get; set; } // Опционально
        }

    }


}
