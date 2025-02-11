using kursach.Pages;

namespace kursach.Services.AuthAPIService;

public interface IAuthAPIService
{
    public Task<bool> LoginUser(Login.LoginModel loginModel);
    public Task<bool> RegisterUser(Register.RegisterModel registerModel);
}