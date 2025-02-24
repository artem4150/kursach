using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Forms;

namespace kursach.Services
{
    public class LoginResponse
    {
        public string Token { get; set; }
    }
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IJSRuntime _jsRuntime;

        public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
        {
            _httpClientFactory = httpClientFactory;
            _jsRuntime = jsRuntime;
        }

        // Метод для получения токена из localStorage
        public async Task<string> GetTokenAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            token = token?.Trim(); // удаляем пробелы в начале и конце
                                   // Если есть подозрение на лишние кавычки:
            token = token?.Trim('\"');
            Console.WriteLine("Из localStorage получен токен: " + token);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Токен не найден или пустой.");
            }
            return token;
        }
        // Метод для отправки запроса с токеном
        public async Task<HttpResponseMessage> SendRequestAsync(string url, HttpMethod method, object content = null)
        {
            var token = await GetTokenAsync();  // Получаем токен

            // Проверка на null
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Токен не может быть пустым.");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var request = new HttpRequestMessage(method, url)
            {
                Content = content == null ? null : new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
            };

            // Добавляем токен в заголовок Authorization
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("Authorization header: " + request.Headers.Authorization);

            return await client.SendAsync(request);
        }
        // Метод для сохранения токена в localStorage
        public async Task SaveTokenAsync(string token)
        {
            Console.WriteLine("Saving token: " + token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        }

        public async Task<int> GetCurrentUserIdAsync()
        {
            var token = await GetTokenAsync();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Пытаемся получить claim с типом ClaimTypes.NameIdentifier или "nameid"
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");

            if (userIdClaim == null)
            {
                throw new InvalidOperationException("Идентификатор пользователя не найден в токене.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new InvalidOperationException("Невозможно преобразовать идентификатор пользователя к числу.");
            }

            return userId;
        }



        // Метод для удаления токена из localStorage
        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }



        // Метод для обновления описания пользователя
        public async Task<bool> UpdateUserDescriptionAsync(int userId, string newDescription)
        {
            var response = await SendRequestAsync($"api/users/{userId}/update-description", HttpMethod.Put, new { Description = newDescription });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDescriptionAsync(int userId, string newDescription)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PutAsJsonAsync($"api/users/{userId}/update-description", new { Description = newDescription });

            return response.IsSuccessStatusCode;
        }
        // Метод для удаления поста
        public async Task<bool> DeleteUserPostAsync(int postId)
        {
            var response = await SendRequestAsync($"api/outfits/{postId}", HttpMethod.Delete);
            return response.IsSuccessStatusCode;
        }



        public async Task<string> UploadProfileImageAsync(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();

            // Создаем MemoryStream, чтобы асинхронно читать файл
            using (var ms = new MemoryStream())
            {
                // Читаем файл в MemoryStream
                await file.OpenReadStream().CopyToAsync(ms);

                // Преобразуем поток в массив байт
                var fileBytes = ms.ToArray();

                // Добавляем байты в MultipartFormDataContent
                var fileContent = new ByteArrayContent(fileBytes);
                content.Add(fileContent, "file", file.Name);

                var response = await SendRequestAsync("api/users/upload-profile-image", HttpMethod.Post, content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"Ошибка при загрузке изображения профиля: {response.ReasonPhrase}");
                }
            }

        }

    }
}

        