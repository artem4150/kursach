namespace kursach.Services.TokenService;

public interface ITokenService
{
    public Task<string> GetTokenAsync();

    public Task SaveTokenAsync(string token);

    public Task RemoveTokenAsync();
}