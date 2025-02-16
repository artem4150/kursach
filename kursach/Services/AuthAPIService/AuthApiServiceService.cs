using kursach.Pages;
using kursach.Services.TokenService;
using Microsoft.JSInterop;
using RestSharp;

namespace kursach.Services.AuthAPIService;

public class AuthApiServiceService : IAuthAPIService
{
    private readonly RestClient _client;
    private readonly ITokenService _tokenService;
    
    public AuthApiServiceService(ITokenService tokenService)
    {
        _client = new RestClient("http://localhost:5001");
        _tokenService = tokenService;
    }
    
    public async Task<bool> LoginUser(Login.LoginModel loginModel)
    {
        var request = new RestRequest("/api/auth/Auth/login");
        request.AddJsonBody(loginModel);
        var response = await _client.ExecutePostAsync(request);
        return response.IsSuccessful;
    }

    public async Task<bool> RegisterUser(Register.RegisterModel registerModel)
    {
        var request = new RestRequest("/api/auth/Auth/register");
        request.AddJsonBody(registerModel);
        var response = await _client.ExecutePostAsync(request);
        return response.IsSuccessful;
    }
    
}