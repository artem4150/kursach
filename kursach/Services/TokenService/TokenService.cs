using Microsoft.JSInterop;

namespace kursach.Services.TokenService;

public class TokenService : ITokenService
{
    private readonly IJSRuntime _jsRuntime;

    public TokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
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