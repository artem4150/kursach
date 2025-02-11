using kursach.Pages;
using RestSharp;

namespace kursach.Services.AuthAPIService;

public class AuthApiServiceService : IAuthAPIService
{
    private readonly RestClient _client;
    
    public AuthApiServiceService()
    {
        _client = new RestClient("http://localhost:5001");
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