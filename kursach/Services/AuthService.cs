using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http;

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

        // Метод для удаления токена из localStorage
        public async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }
    }


}
