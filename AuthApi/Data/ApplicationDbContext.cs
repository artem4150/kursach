using AuthApi;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AuthApi
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Таблицы в базе данных
        public DbSet<User> Users { get; set; }
        public DbSet<Outfit> Outfits { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<OutfitTag> OutfitTags { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follower> Followers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для таблицы OutfitTags (связь "многие ко многим")
            modelBuilder.Entity<OutfitTag>()
                .HasKey(ot => new { ot.OutfitId, ot.TagId });

            modelBuilder.Entity<OutfitTag>()
                .HasOne(ot => ot.Outfit)
                .WithMany(o => o.OutfitTags)
                .HasForeignKey(ot => ot.OutfitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OutfitTag>()
                .HasOne(ot => ot.Tag)
                .WithMany(t => t.OutfitTags)
                .HasForeignKey(ot => ot.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация для таблицы Followers (связь между пользователями)
            modelBuilder.Entity<Follower>()
                .HasOne(f => f.User)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.FollowedUser)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ограничения длины строк и уникальность Email
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Значение по умолчанию для CreatedAt (PostgreSQL: CURRENT_TIMESTAMP)
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Outfit>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Tag>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Like>()
                .Property(l => l.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Comment>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Follower>()
                .Property(f => f.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
