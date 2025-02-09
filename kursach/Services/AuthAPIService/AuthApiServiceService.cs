using kursach.Pages;
using RestSharp;

namespace kursach.Services.AuthAPIService;

public class AuthApiServiceService : IAuthAPIService
{
    private readonly RestClient _client;
    
    public AuthApiServiceService()
    {
        _client = new RestClient("http://localhost:5000");
    }
    
    public async Task<bool> LoginUser(Login.LoginModel loginModel)
    {
        var request = new RestRequest("/api/auth/Auth/login");
        request.AddBody(loginModel);
        var response = await _client.ExecutePostAsync(request);
        if (response.IsSuccessful)
        {
            return true;
        }
        return false;
    }
}