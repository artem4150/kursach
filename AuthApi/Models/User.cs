

namespace AuthApi
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        //public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Follower>? Followers { get; set; }  // Подписчики
        public ICollection<Follower>? Followings { get; set; }  // Подписки
        public ICollection<Outfit>? Outfits { get; set; }
        public ICollection<Like>? Likes { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }

    public class Tag
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OutfitTag>? OutfitTags { get; set; } // Связь с аутфитами
    }

    //хорошей практикой считается 1 файл - 1 класс. Вынеси классы ниже в отдельные файл
    public class Outfit
    {
        public int OutfitId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Season { get; set; }
        public string? Style { get; set; }
        public string? Gender { get; set; }
        public string? Occasion { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }

        public User User { get; set; }
        public ICollection<OutfitTag>? OutfitTags { get; set; } // Связь с тегами
        public ICollection<Like>? Likes { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }

    public class OutfitTag
    {
        public int OutfitId { get; set; }
        public Outfit Outfit { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }

    public class Like
    {
        public int LikeId { get; set; }
        public int OutfitId { get; set; }
        public Outfit Outfit { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Comment
    {
        public int CommentId { get; set; }
        public int OutfitId { get; set; }
        public Outfit Outfit { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Follower
    {
        public int FollowerId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int FollowedUserId { get; set; }
        public User FollowedUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
