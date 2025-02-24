using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace kursach.Services
{
    public class PublicationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthService _authService;

        public PublicationService(IHttpClientFactory httpClientFactory, AuthService authService)
        {
            _httpClientFactory = httpClientFactory;
            _authService = authService;
        }

        // Метод для получения всех публикаций
        public async Task<List<PostDto>> GetPostsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _authService.SendRequestAsync("api/Outfits", HttpMethod.Get, null);
            if (response.IsSuccessStatusCode)
            {
                var posts = await response.Content.ReadFromJsonAsync<List<PostDto>>(cancellationToken: cancellationToken);
                return posts;
            }
            else
            {
                throw new HttpRequestException($"Ошибка при загрузке публикаций: {response.ReasonPhrase}");
            }
        }

        // Метод для отправки лайка публикации
        public async Task<PostDto> LikePostAsync(int outfitId)
        {
            var response = await _authService.SendRequestAsync($"api/Outfits/{outfitId}/like", HttpMethod.Post);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PostDto>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при лайке публикации: {response.ReasonPhrase}");
            }
        }

        // Метод для отправки комментария к публикации
        public async Task<CommentDto> SubmitCommentAsync(int outfitId, string commentText)
        {
            var commentContent = new { Text = commentText };
            var response = await _authService.SendRequestAsync($"api/Outfits/{outfitId}/comment", HttpMethod.Post, commentContent);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDto>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при отправке комментария: {response.ReasonPhrase}");
            }
        }

        // Новый метод: Получение публикаций от подписок (лента подписок)
        public async Task<List<PostDto>> GetSubscribedPostsAsync(int subscriberId, CancellationToken cancellationToken = default)
        {
            var response = await _authService.SendRequestAsync($"api/Followers/feed/{subscriberId}", HttpMethod.Get, null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PostDto>>(cancellationToken: cancellationToken);
            }
            else
            {
                throw new HttpRequestException($"Ошибка при загрузке подписанных публикаций: {response.ReasonPhrase}");
            }
        }

        // Новый метод: Подписка на пользователя
        public async Task SubscribeAsync(int subscriberId, int subscribedToId)
        {
            var dto = new SubscribeDto
            {
                FollowerId = subscriberId,
                SubscribedToId = subscribedToId
            };

            var response = await _authService.SendRequestAsync("api/Followers/subscribe", HttpMethod.Post, dto);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ошибка при подписке: {response.ReasonPhrase}");
            }
        }

        // Новый метод: Отписка от пользователя
        public async Task UnsubscribeAsync(int subscriberId, int subscribedToId)
        {
            var response = await _authService.SendRequestAsync($"api/Followers/unsubscribe/{subscriberId}/{subscribedToId}", HttpMethod.Delete, null);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ошибка при отписке: {response.ReasonPhrase}");
            }
        }
        
        public async Task<bool> IsSubscribedAsync(int subscriberId, int followedUserId)
        {
            var response = await _authService.SendRequestAsync($"api/Followers/isSubscribed/{subscriberId}/{followedUserId}", HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при проверке подписки: {response.ReasonPhrase}");
            }
        }
        
        public async Task<int> GetFollowersCountAsync(int userId)
        {
            var response = await _authService.SendRequestAsync($"api/Followers/count/{userId}", HttpMethod.Get, null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при получении количества подписчиков: {response.ReasonPhrase}");
            }
        }

        // Метод для редактирования поста
        public async Task<PostDto> EditPostAsync(int outfitId, string title, string description)
        {
            var response = await _authService.SendRequestAsync($"api/outfits/{outfitId}", HttpMethod.Put, new { Title = title, Description = description });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PostDto>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при редактировании публикации: {response.ReasonPhrase}");
            }
        }

        // Метод для удаления поста
        public async Task<bool> DeletePostAsync(int outfitId)
        {
            var response = await _authService.SendRequestAsync($"api/outfits/{outfitId}", HttpMethod.Delete);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PostDto>> GetUserPostsAsync(int userId)
        {
            var response = await _authService.SendRequestAsync($"api/Outfits/user/{userId}", HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PostDto>>();
            }
            else
            {
                throw new HttpRequestException($"Ошибка при загрузке публикаций пользователя: {response.ReasonPhrase}");
            }
        }


        // DTO модели

        public class PostDto
        {
            public int OutfitId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string AuthorUsername { get; set; }
            public DateTime CreatedAt { get; set; }
            public int LikesCount { get; set; }
            public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
            public string AuthorAvatar { get; set; }
            
            public int AuthorId { get; set; }
        }

        public class CommentDto
        {
            public int CommentId { get; set; }
            public string Text { get; set; }
            public string AuthorUsername { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class UserInfoDto
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string ProfilePicture { get; set; }
            public string Description { get; set; }
            public int FollowersCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsSubscribed { get; set; } // Можно инициализировать false по умолчанию
        }

        // DTO для подписки
        public class SubscribeDto
        {
            // Идентификатор пользователя, который подписывается
            public int FollowerId { get; set; }
            // Идентификатор пользователя, на которого подписываются
            public int SubscribedToId { get; set; }
        }
    }
}
